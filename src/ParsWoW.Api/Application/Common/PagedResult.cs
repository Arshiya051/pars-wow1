namespace ParsWoW.Api.Application.Common;

/// <summary>Page request descriptor shared by list endpoints.</summary>
public sealed class PageRequest
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 25;

    public int NormalizedPage => Math.Max(1, Page);
    public int NormalizedPageSize => Math.Clamp(PageSize, 1, 200);
    public int Offset => (NormalizedPage - 1) * NormalizedPageSize;
}

/// <summary>Paged result envelope.</summary>
public sealed class PagedResult<T>
{
    public required IReadOnlyList<T> Items { get; init; }
    public required int Page { get; init; }
    public required int PageSize { get; init; }
    public required long Total { get; init; }
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling((double)Total / PageSize);
}
