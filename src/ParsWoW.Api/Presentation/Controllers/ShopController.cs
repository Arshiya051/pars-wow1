using Microsoft.AspNetCore.Mvc;
using ParsWoW.Api.Application.Abstractions.Services;
using ParsWoW.Api.Application.Common;
using ParsWoW.Api.Application.Dto.Shop;

namespace ParsWoW.Api.Presentation.Controllers;

/// <summary>Controller for in-game shop purchases: validate account, SKU, and payment, then deliver goods.</summary>
[ApiController]
[Route("api/shop")]
public sealed class ShopController : ControllerBase
{
    private readonly IShopService _shop;
    /// <summary>Initialises a new <see cref="ShopController"/>.</summary>
    /// <param name="shop">Shop service.</param>
    public ShopController(IShopService shop) => _shop = shop;

    /// <summary>Process a purchase: validate account, SKU, payment status, then deliver goods and log the transaction.</summary>
    /// <param name="req">Purchase request with account, expansion, SKU, quantity, and payment reference.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpPost("purchase")]
    [ProducesResponseType(typeof(ApiResponse<PurchaseResultDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<PurchaseResultDto>), 400)]
    public async Task<IActionResult> Purchase([FromBody] PurchaseRequest req, CancellationToken ct)
    {
        var r = await _shop.PurchaseAsync(req, ct);
        return r.IsSuccess
            ? Ok(ApiResponse<PurchaseResultDto>.Ok(r.Value!, 200))
            : StatusCode(400, ApiResponse<PurchaseResultDto>.Fail(400, r.Code ?? "ERROR", r.Error ?? "Purchase failed."));
    }
}
