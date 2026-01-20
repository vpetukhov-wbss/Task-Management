using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Common.Application.Pagination;
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;
using Modules.Users.Domain.Users;
using Modules.Users.Features.Users.Shared;
using Modules.Users.Infrastructure.Database;

namespace Modules.Users.Features.Users.GetAllUsers;

internal interface IGetAllUsersHandler : IHandler
{
    Task<Result<PagedResult<UserResponse>>> HandleAsync(
        PagedRequest request,
        CancellationToken cancellationToken);
}

internal sealed class GetAllUsersHandler(
    UsersDbContext context,
    ILogger<GetAllUsersHandler> logger)
    : IGetAllUsersHandler
{
    public async Task<Result<PagedResult<UserResponse>>> HandleAsync(
        PagedRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = BuildQuery(request);
            var totalCount = await query.CountAsync(cancellationToken);
            var normalizedRequest = request.Normalize();

            var users = await query
                .Skip(normalizedRequest.Skip)
                .Take(normalizedRequest.PageSize)
                .ToListAsync(cancellationToken);

            var userResponses = MapToUserResponses(users);
            var pagedResult = PagedResult<UserResponse>.Create(
                userResponses,
                normalizedRequest.PageNumber,
                normalizedRequest.PageSize,
                totalCount);

            logger.LogInformation(
                "Retrieved {Count} users (page {Page} of {TotalPages})",
                userResponses.Count,
                pagedResult.PageNumber,
                pagedResult.TotalPages);

            return pagedResult;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving users");
            return PagedResult<UserResponse>.Empty();
        }
    }

    private IQueryable<User> BuildQuery(PagedRequest request)
    {
        var query = context.Users
            .AsNoTracking()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(u => u.Email!.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase));
        }

        // Apply sorting
        query = ApplySorting(query, request);

        return query;
    }

    private static IQueryable<User> ApplySorting(IQueryable<User> query, PagedRequest request)
    {
        return request.SortBy?.ToLower() switch
        {
            "email" => request.SortAscending
                ? query.OrderBy(u => u.Email)
                : query.OrderByDescending(u => u.Email),
            "createdat" or "createdatutc" => request.SortAscending
                ? query.OrderBy(u => u.CreatedAtUtc)
                : query.OrderByDescending(u => u.CreatedAtUtc),
            _ => query.OrderBy(u => u.Email)
        };
    }

    private static List<UserResponse> MapToUserResponses(List<User> users)
    {
        return users.Select(u => new UserResponse(
            u.Id,
            u.Email!,
            u.UserRoles.FirstOrDefault()?.Role?.Name ?? "User",
            u.CreatedAtUtc,
            u.UpdatedAtUtc))
            .ToList();
    }
}

