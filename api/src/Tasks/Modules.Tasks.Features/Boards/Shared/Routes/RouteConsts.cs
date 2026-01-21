namespace Modules.Tasks.Features.Boards.Shared.Routes;

internal static class RouteConsts
{
	private const string BaseRoute = "/api/boards";

	public const string Create = BaseRoute;
	public const string GetAll = BaseRoute;
	public const string GetById = $"{BaseRoute}/{{boardId:guid}}";
	public const string GetByUser = $"{BaseRoute}/user/{{userId}}";
	public const string Update = $"{BaseRoute}/{{boardId:guid}}";
	public const string Delete = $"{BaseRoute}/{{boardId:guid}}";
}

