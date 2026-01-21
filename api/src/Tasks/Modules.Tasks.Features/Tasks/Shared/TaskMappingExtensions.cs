using Modules.Tasks.Domain.Tasks;

namespace Modules.Tasks.Features.Tasks.Shared;

internal static class TaskMappingExtensions
{
	public static TaskResponse ToResponse(this TaskItem task)
	{
		return new TaskResponse(
			task.Id.Value,
			task.Title,
			task.Description,
			(int)task.Status,
			task.Status.ToString(),
			(int)task.Priority,
			task.Priority.ToString(),
			task.UserId,
			task.BoardId.Value,
			task.CreatedAtUtc,
			task.UpdatedAtUtc);
	}
}

