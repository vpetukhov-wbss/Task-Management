namespace Modules.Tasks.Features.Tasks.Shared.Routes;

public static class RouteConsts
{
	private const string BaseRoute = "/api/tasks";

	public const string Create = BaseRoute;
	public const string GetById = $"{BaseRoute}/{{taskId:guid}}";
	public const string GetByUser = $"{BaseRoute}/user/{{userId}}";
	public const string GetByStatus = $"{BaseRoute}/status/{{status:int}}";
	public const string GetByPriority = $"{BaseRoute}/priority/{{priority:int}}";
	public const string GetByBoard = $"{BaseRoute}/board/{{boardId:guid}}";
	public const string Update = $"{BaseRoute}/{{taskId:guid}}";
	public const string Patch = $"{BaseRoute}/{{taskId:guid}}";
	public const string Delete = $"{BaseRoute}/{{taskId:guid}}";
}

