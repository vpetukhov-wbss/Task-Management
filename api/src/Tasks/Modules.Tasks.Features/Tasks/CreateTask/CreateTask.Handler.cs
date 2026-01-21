using Microsoft.Extensions.Logging;
using Modules.Common.Domain.Events;
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;
using Modules.Tasks.Domain.Boards;
using Modules.Tasks.Domain.Tasks;
using Modules.Tasks.Domain.Tasks.Events;
using Modules.Tasks.Features.Tasks.Shared;
using Modules.Tasks.Infrastructure.Database;

namespace Modules.Tasks.Features.Tasks.CreateTask;

internal interface ICreateTaskHandler : IHandler
{
	Task<Result<TaskResponse>> HandleAsync(CreateTaskRequest request, CancellationToken cancellationToken);
}

internal sealed class CreateTaskHandler(
	ITaskRepository taskRepository,
	IUnitOfWork unitOfWork,
	IEventPublisher eventPublisher,
	ILogger<CreateTaskHandler> logger)
	: ICreateTaskHandler
{
	public async Task<Result<TaskResponse>> HandleAsync(
		CreateTaskRequest request,
		CancellationToken cancellationToken)
	{
		var taskResult = TaskItem.Create(
			request.Title,
			request.Description,
			(TaskPriority)request.Priority,
			request.UserId,
			new BoardId(request.BoardId));

		if (taskResult.IsError)
		{
			logger.LogWarning("Failed to create task: {Errors}", taskResult.Errors);
			return taskResult.Errors;
		}

		await taskRepository.AddAsync(taskResult.Value!, cancellationToken);
		await unitOfWork.SaveChangesAsync(cancellationToken);

		logger.LogInformation("Task created with ID: {TaskId}", taskResult.Value!.Id.Value);

		// Publish TaskCreatedEvent
		var taskCreatedEvent = new TaskCreatedEvent(
			taskResult.Value!.Id,
			taskResult.Value.Title,
			taskResult.Value.Description,
			taskResult.Value.Priority,
			taskResult.Value.UserId,
			DateTimeOffset.UtcNow);

		await eventPublisher.PublishAsync(taskCreatedEvent, cancellationToken);

		return taskResult.Value!.ToResponse();
	}
}

