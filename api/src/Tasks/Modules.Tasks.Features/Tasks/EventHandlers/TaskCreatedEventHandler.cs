using Microsoft.Extensions.Logging;
using Modules.Common.Domain.Events;
using Modules.Tasks.Domain.Notifications;
using Modules.Tasks.Domain.Tasks.Events;

namespace Modules.Tasks.Features.Tasks.EventHandlers;

/// <summary>
/// Event handler that sends a notification when a task is created
/// </summary>
public sealed class TaskCreatedEventHandler(
	INotificationService notificationService,
	ILogger<TaskCreatedEventHandler> logger)
	: IEventHandler<TaskCreatedEvent>
{
	public async Task HandleAsync(TaskCreatedEvent @event, CancellationToken cancellationToken)
	{
		logger.LogInformation(
			"Handling TaskCreatedEvent for Task {TaskId} - User {UserId}",
			@event.TaskId.Value,
			@event.UserId);

		const string notificationTitle = "New Task Created";
		var notificationMessage = $"Your task '{@event.Title}' has been created successfully with {@event.Priority} priority.";

		await notificationService.SendNotificationAsync(
			@event.UserId,
			notificationTitle,
			notificationMessage,
			cancellationToken);

		logger.LogInformation(
			"Successfully sent notification for Task {TaskId}",
			@event.TaskId.Value);
	}
}

