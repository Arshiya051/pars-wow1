using ParsWoW.Api.Application.Constants;

namespace ParsWoW.Api.Application.Dto.Shop;

public sealed class PurchaseRequest
{
    public int AccountId { get; set; }
    public ExpansionKind Expansion { get; set; } = ExpansionKind.WOTLK;
    public string ItemSku { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public string? PaymentReference { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal DeclaredAmount { get; set; }
}

public sealed class DeliverableDto
{
    public required string Kind { get; init; } // item, gold, mount, pet, title, toy, transmog, profession-service
    public int? ItemEntry { get; init; }
    public string? MountSpellId { get; init; }
    public string? PetCreatureEntry { get; init; }
    public int? TitleId { get; init; }
    public int? ToyItemEntry { get; init; }
    public int? TransmogEntry { get; init; }
    public string? Profession { get; init; }
    public long? GoldCopper { get; init; }
    public Dictionary<string, object>? Metadata { get; init; }
}

public sealed class PurchaseResultDto
{
    public required Guid PurchaseId { get; init; }
    public required int AccountId { get; init; }
    public required ExpansionKind Expansion { get; init; }
    public required string Sku { get; init; }
    public required string Status { get; init; } // success | failed | pending
    public required decimal ChargedAmount { get; init; }
    public required string Currency { get; init; }
    public required IReadOnlyList<DeliverableDto> Deliverables { get; init; }
    public string? Message { get; init; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
