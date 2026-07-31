using ParsWoW.Api.Application.Common;
using ParsWoW.Api.Application.Dto.Shop;

namespace ParsWoW.Api.Application.Abstractions.Services;

public interface IShopService
{
    Task<Result<PurchaseResultDto>> PurchaseAsync(PurchaseRequest request, CancellationToken ct = default);
}
