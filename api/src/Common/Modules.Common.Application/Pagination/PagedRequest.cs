namespace Modules.Common.Application.Pagination;

/// <summary>
/// Represents a request for paginated data
/// </summary>
public sealed record PagedRequest
{
    /// <summary>
    /// Page number (1-based)
    /// </summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; init; } = 10;

    /// <summary>
    /// Optional search term
    /// </summary>
    public string? SearchTerm { get; init; }

    /// <summary>
    /// Optional sort field
    /// </summary>
    public string? SortBy { get; init; }

    /// <summary>
    /// Sort direction (true = ascending, false = descending)
    /// </summary>
    public bool SortAscending { get; init; } = true;

    /// <summary>
    /// Validates and normalizes the paged request
    /// </summary>
    public PagedRequest Normalize()
    {
        var normalizedPageNumber = PageNumber < 1 ? 1 : PageNumber;
        var normalizedPageSize = PageSize switch
        {
            < 1 => 10,
            > 100 => 100,
            _ => PageSize
        };

        return this with
        {
            PageNumber = normalizedPageNumber,
            PageSize = normalizedPageSize
        };
    }

    /// <summary>
    /// Calculates the number of items to skip
    /// </summary>
    public int Skip => (PageNumber - 1) * PageSize;
}

