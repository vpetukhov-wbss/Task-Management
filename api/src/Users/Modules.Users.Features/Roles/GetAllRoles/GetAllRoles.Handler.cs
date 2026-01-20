using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Common.Application.Pagination;
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;
using Modules.Users.Features.Roles.Shared;
using Modules.Users.Infrastructure.Database;

namespace Modules.Users.Features.Roles.GetAllRoles;

internal interface IGetAllRolesHandler : IHandler
{
    Task<Result<PagedResult<RoleResponse>>> HandleAsync(
        PagedRequest request,
        CancellationToken cancellationToken);
}

internal sealed class GetAllRolesHandler(
    UsersDbContext context,
    ILogger<GetAllRolesHandler> logger)
    : IGetAllRolesHandler
{
    public async Task<Result<PagedResult<RoleResponse>>> HandleAsync(
        PagedRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = BuildQuery(request);
            var totalCount = await query.CountAsync(cancellationToken);
            var normalizedRequest = request.Normalize();

            var roles = await query
                .Skip(normalizedRequest.Skip)
                .Take(normalizedRequest.PageSize)
                .ToListAsync(cancellationToken);

            var roleResponses = await MapToRoleResponses(roles, cancellationToken);
            var pagedResult = PagedResult<RoleResponse>.Create(
                roleResponses,
                normalizedRequest.PageNumber,
                normalizedRequest.PageSize,
                totalCount);

            logger.LogInformation(
                "Retrieved {Count} roles (page {Page} of {TotalPages})",
                roleResponses.Count,
                pagedResult.PageNumber,
                pagedResult.TotalPages);

            return pagedResult;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving roles");
            return PagedResult<RoleResponse>.Empty();
        }
    }

    private IQueryable<Domain.Users.Role> BuildQuery(PagedRequest request)
    {
        var query = context.Roles
            .AsNoTracking()
            .AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(r => r.Name!.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase));
        }

        // Apply sorting
        query = ApplySorting(query, request);

        return query;
    }

    private static IQueryable<Domain.Users.Role> ApplySorting(IQueryable<Domain.Users.Role> query, PagedRequest request)
    {
        return request.SortBy?.ToLower() switch
        {
            "name" => request.SortAscending
                ? query.OrderBy(r => r.Name)
                : query.OrderByDescending(r => r.Name),
            _ => query.OrderBy(r => r.Name)
        };
    }

    private async Task<List<RoleResponse>> MapToRoleResponses(
        List<Domain.Users.Role> roles,
        CancellationToken cancellationToken)
    {
        var roleResponses = new List<RoleResponse>();

        foreach (var role in roles)
        {
            var userCount = await context.UserRoles
                .Where(ur => ur.RoleId == role.Id)
                .CountAsync(cancellationToken);

            roleResponses.Add(new RoleResponse(role.Id, role.Name!, userCount));
        }

        return roleResponses;
    }
}

