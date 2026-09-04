namespace Lawllit.Model.Common;

public sealed class Pagination
{
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 50;

    public int Offset => (Math.Max(CurrentPage, 1) - 1) * PageSize;
}

public sealed class PagedResult<TItem>
{
    public List<TItem> Items { get; set; } = [];
    public int TotalItems { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 50;

    public int TotalPages => PageSize > 0
        ? (int)Math.Ceiling(TotalItems / (double)PageSize)
        : 0;

    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
}
