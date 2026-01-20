using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;
using Modules.Common.Application.Pagination;
using Modules.Users.Domain.Policies;
using Modules.Users.Features.Users.Shared.Routes;

namespace Modules.Users.Features.Users.GetAllUsers;

public class GetAllUsersEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet(RouteConsts.GetAll, Handle)
            .RequireAuthorization(UserPolicyConsts.ReadPolicy)
            .WithTags("Users");
    }

    private static async Task<IResult> Handle(
        int pageNumber = 1,
        int pageSize = 10,
        string? searchTerm = null,
        string? sortBy = null,
        bool sortAscending = true,
        IGetAllUsersHandler handler = null!,
        CancellationToken cancellationToken = default)
    {
        var request = new PagedRequest
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm,
            SortBy = sortBy,
            SortAscending = sortAscending
        };

        var response = await handler.HandleAsync(request, cancellationToken);
        if (response.IsError)
        {
            return response.Errors.ToProblem();
        }

        return Results.Ok(response.Value);
    }
}

