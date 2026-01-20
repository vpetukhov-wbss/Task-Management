namespace Modules.Users.Features.Roles.Shared.Routes;

internal static class RouteConsts
{
    private const string BaseRoute = "/api/roles";

    internal const string GetAll = BaseRoute;

    internal const string GetById = $"{BaseRoute}/{{roleId}}";

    internal const string CreateRole = BaseRoute;

    internal const string UpdateRole = $"{BaseRoute}/{{roleId}}";

    internal const string DeleteRole = $"{BaseRoute}/{{roleId}}";
}

