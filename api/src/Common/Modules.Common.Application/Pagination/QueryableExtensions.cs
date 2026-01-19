using Microsoft.EntityFrameworkCore;

namespace Modules.Common.Application.Pagination;

/// <summary>
/// Extension methods for IQueryable to support pagination
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// Applies pagination to a queryable and returns a paged result
    /// </summary>
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        PagedRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedRequest = request.Normalize();

        var totalCount = await query.CountAsync(cancellationToken);

        if (totalCount == 0)
        {
            return PagedResult<T>.Empty(normalizedRequest.PageNumber, normalizedRequest.PageSize);
        }

        var items = await query
            .Skip(normalizedRequest.Skip)
            .Take(normalizedRequest.PageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<T>.Create(
            items,
            normalizedRequest.PageNumber,
            normalizedRequest.PageSize,
            totalCount);
    }

    /// <summary>
    /// Applies pagination to a queryable and returns a paged result with projection
    /// </summary>
    public static async Task<PagedResult<TResult>> ToPagedResultAsync<TSource, TResult>(
        this IQueryable<TSource> query,
        PagedRequest request,
        Func<TSource, TResult> selector,
        CancellationToken cancellationToken = default)
    {
        var normalizedRequest = request.Normalize();

        var totalCount = await query.CountAsync(cancellationToken);

        if (totalCount == 0)
        {
            return PagedResult<TResult>.Empty(normalizedRequest.PageNumber, normalizedRequest.PageSize);
        }

        var items = await query
            .Skip(normalizedRequest.Skip)
            .Take(normalizedRequest.PageSize)
            .ToListAsync(cancellationToken);

        var projectedItems = items.Select(selector).ToList();

        return PagedResult<TResult>.Create(
            projectedItems,
            normalizedRequest.PageNumber,
            normalizedRequest.PageSize,
            totalCount);
    }
}

