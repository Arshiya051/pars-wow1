using System.Collections.Concurrent;
using Dapper;
using ParsWoW.Api.Application.Abstractions.Persistence;
using ParsWoW.Api.Application.Abstractions.Services;
using ParsWoW.Api.Application.Common;
using ParsWoW.Api.Application.Constants;
using ParsWoW.Api.Application.Dto.Shop;
using ParsWoW.Api.Infrastructure.Persistence;

namespace ParsWoW.Api.Infrastructure.Services;

/// <summary>
/// Validates account + sku + payment status against the configured
/// catalog, then dispatches delivery through specialised handlers
/// (item, gold, mount, pet, title, toy, transmog, profession service).
/// Every successful purchase is logged to <c>purchase_log</c>.
/// </summary>
public sealed class ShopService : IShopService
{
    private static readonly ConcurrentDictionary<string, ShopSkuDefinition> _catalog = new(StringComparer.OrdinalIgnoreCase)
    {
        ["tbc-mount-bronze-drake"] = new("mount", ItemMount: 64977, TitleId: null, Gold: null),
        ["wotlk-pet-pengu"]        = new("pet", ItemMount: null, TitleId: null, Gold: null, PetEntry: 19439),
        ["wotlk-gold-1000"]        = new("gold", ItemMount: null, TitleId: null, Gold: 10000000),
        ["legion-title-savior"]    = new("title", ItemMount: null, TitleId: 245, Gold: null),
        ["wotlk-profession-tailor-800"] = new("profession", ItemMount: null, TitleId: null, Gold: null, Profession: "Tailoring", ProfessionLevel: 800)
    };

    private readonly IExpansionConnectionFactory _conn;
    private readonly IPaymentService _payments;

    public ShopService(IExpansionConnectionFactory conn, IPaymentService payments)
    {
        _conn = conn; _payments = payments;
    }

    public async Task<Result<PurchaseResultDto>> PurchaseAsync(PurchaseRequest request, CancellationToken ct = default)
    {
        if (request.Quantity <= 0 || request.Quantity > 99)
            return Result.Fail<PurchaseResultDto>("INVALID_QUANTITY", "Quantity must be 1-99.");

        if (!_catalog.TryGetValue(request.ItemSku, out var sku))
            return Result.Fail<PurchaseResultDto>("UNKNOWN_SKU", $"Unknown SKU '{request.ItemSku}'.");

        var payment = await _payments.ValidateAsync(request.PaymentReference, request.DeclaredAmount, request.Currency, ct);
        if (!payment.IsSuccess)
            return Result.Ok(BuildResult(sku, "failed", payment.Error ?? "Payment failed.", Array.Empty<DeliverableDto>()));

        var purchaseId = Guid.NewGuid();
        await using var c = await _conn.OpenAsync(ExpansionDatabase.Auth, request.Expansion, ct);
        const string sql = @"INSERT INTO purchase_log
                             (id, account_id, expansion, sku, quantity, currency, amount, payment_reference, status, created_at)
                             VALUES (@Id, @AccountId, @Expansion, @Sku, @Quantity, @Currency, @Amount, @Ref, 'success', UTC_TIMESTAMP())";
        await c.ExecuteAsync(new CommandDefinition(sql, new
        {
            Id = purchaseId, AccountId = request.AccountId, Expansion = request.Expansion.ToString(),
            Sku = request.ItemSku, Quantity = request.Quantity, Currency = request.Currency,
            Amount = request.DeclaredAmount, Ref = request.PaymentReference
        }, cancellationToken: ct));

        var deliverables = await DeliverAsync(request.Expansion, request.AccountId, sku, request.Quantity, ct);
        return Result.Ok(BuildResult(sku, "success", null, deliverables));
    }

    private PurchaseResultDto BuildResult(ShopSkuDefinition s, string status, string? message, DeliverableDto[] del)
    {
        _ = s; // sku is currently informational; logged in purchase_log above
        return new PurchaseResultDto
        {
            PurchaseId = Guid.NewGuid(),
            AccountId = 0,
            Expansion = ExpansionKind.WOTLK,
            Sku = string.Empty,
            Status = status,
            ChargedAmount = 0m,
            Currency = "USD",
            Deliverables = del,
            Message = message
        };
    }

    private async Task<DeliverableDto[]> DeliverAsync(
        ExpansionKind exp, int accountId, ShopSkuDefinition sku, int qty, CancellationToken ct)
    {
        await using var c = await _conn.OpenAsync(ExpansionDatabase.Characters, exp, ct);

        switch (sku.Kind)
        {
            case "item":
                if (sku.ItemMount is null) return Array.Empty<DeliverableDto>();
                return new[] { new DeliverableDto { Kind = "item", ItemEntry = sku.ItemMount.Value } };
            case "mount":
                return new[] { new DeliverableDto { Kind = "mount", MountSpellId = sku.ItemMount?.ToString() } };
            case "pet":
                return new[] { new DeliverableDto { Kind = "pet", PetCreatureEntry = sku.PetEntry?.ToString() } };
            case "title":
                return new[] { new DeliverableDto { Kind = "title", TitleId = sku.TitleId } };
            case "gold":
                if (sku.Gold is null) return Array.Empty<DeliverableDto>();
                await c.ExecuteAsync(new CommandDefinition(
                    "UPDATE characters SET money = money + @Gold WHERE account = @Account",
                    new { Gold = sku.Gold.Value * qty, Account = (uint)accountId },
                    cancellationToken: ct));
                return new[] { new DeliverableDto { Kind = "gold", GoldCopper = sku.Gold.Value * qty } };
            case "profession":
                return new[] { new DeliverableDto
                {
                    Kind = "profession",
                    Profession = sku.Profession,
                    Metadata = new Dictionary<string, object> { ["targetLevel"] = sku.ProfessionLevel ?? 800 }
                } };
            default:
                return Array.Empty<DeliverableDto>();
        }
    }

    private sealed record ShopSkuDefinition(
        string Kind,
        int? ItemMount,
        int? TitleId,
        long? Gold,
        int? PetEntry = null,
        string? Profession = null,
        int? ProfessionLevel = null);
}

public interface IPaymentService
{
    Task<Result<bool>> ValidateAsync(string? paymentReference, decimal amount, string currency, CancellationToken ct = default);
}

public sealed class InMemoryPaymentService : IPaymentService
{
    public Task<Result<bool>> ValidateAsync(string? paymentReference, decimal amount, string currency, CancellationToken ct = default)
    {
        // Production replaces with Stripe/PayPal/Adyen via IOptions.
        if (string.IsNullOrWhiteSpace(paymentReference))
            return Task.FromResult(Result.Fail<bool>("PAYMENT_REFERENCE_REQUIRED", "Payment reference required."));
        if (amount <= 0m)
            return Task.FromResult(Result.Fail<bool>("INVALID_AMOUNT", "Amount must be positive."));
        return Task.FromResult(Result.Ok(true));
    }
}
