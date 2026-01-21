namespace Modules.Tasks.Domain.Notifications;

/// <summary>
/// Service for sending notifications to users
/// </summary>
public interface INotificationService
{
	/// <summary>
	/// Sends a notification to a user
	/// </summary>
	/// <param name="userId">The ID of the user to notify</param>
	/// <param name="title">The notification title</param>
	/// <param name="message">The notification message</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>A task representing the asynchronous operation</returns>
	Task SendNotificationAsync(
		string userId,
		string title,
		string message,
		CancellationToken cancellationToken = default);
}

