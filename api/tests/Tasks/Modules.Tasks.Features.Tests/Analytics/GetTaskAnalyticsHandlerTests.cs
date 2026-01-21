using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Modules.Tasks.Domain.Boards;
using Modules.Tasks.Domain.Tasks;
using Modules.Tasks.Features.Analytics.GetTaskAnalytics;
using Modules.Tasks.Infrastructure.Database;

namespace Modules.Tasks.Features.Tests.Analytics;

public sealed class GetTaskAnalyticsHandlerTests : IDisposable
{
	private readonly TasksDbContext _context;
	private readonly GetTaskAnalyticsHandler _handler;

	public GetTaskAnalyticsHandlerTests()
	{
		var options = new DbContextOptionsBuilder<TasksDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;

		_context = new TasksDbContext(options);
		_handler = new GetTaskAnalyticsHandler(_context);
	}

	[Fact]
	public async Task Handle_WithNoTasks_ShouldReturnZeroMetrics()
	{
		// Act
		var result = await _handler.Handle(CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.TotalTasks.Should().Be(0);
		result.Value.TotalBoards.Should().Be(0);
		result.Value.RecentTasks.Should().Be(0);
		result.Value.CompletionRate.Should().Be(0);
	}

	[Fact]
	public async Task Handle_WithTasks_ShouldReturnCorrectTotalCount()
	{
		// Arrange
		var board = CreateBoard("Test Board", "user1");
		await _context.Boards.AddAsync(board);
		await _context.SaveChangesAsync();

		var task1 = CreateTask("Task 1", board.Id, "user1");
		var task2 = CreateTask("Task 2", board.Id, "user1");
		await _context.Tasks.AddRangeAsync(task1, task2);
		await _context.SaveChangesAsync();

		// Act
		var result = await _handler.Handle(CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.TotalTasks.Should().Be(2);
		result.Value.TotalBoards.Should().Be(1);
	}

	[Fact]
	public async Task Handle_WithCompletedTasks_ShouldCalculateCompletionRate()
	{
		// Arrange
		var board = CreateBoard("Test Board", "user1");
		await _context.Boards.AddAsync(board);
		await _context.SaveChangesAsync();

		var task1 = CreateTask("Task 1", board.Id, "user1", Domain.Tasks.TaskStatus.Complete);
		var task2 = CreateTask("Task 2", board.Id, "user1", Domain.Tasks.TaskStatus.Incomplete);
		await _context.Tasks.AddRangeAsync(task1, task2);
		await _context.SaveChangesAsync();

		// Act
		var result = await _handler.Handle(CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.CompletionRate.Should().Be(50.0);
	}

	[Fact]
	public async Task Handle_WithRecentTasks_ShouldCountTasksFromLast7Days()
	{
		// Arrange
		var board = CreateBoard("Test Board", "user1");
		await _context.Boards.AddAsync(board);
		await _context.SaveChangesAsync();

		var recentTask = CreateTask("Recent Task", board.Id, "user1");
		recentTask.CreatedAtUtc = DateTime.UtcNow.AddDays(-3);

		var oldTask = CreateTask("Old Task", board.Id, "user1");
		oldTask.CreatedAtUtc = DateTime.UtcNow.AddDays(-10);

		await _context.Tasks.AddRangeAsync(recentTask, oldTask);
		await _context.SaveChangesAsync();

		// Act
		var result = await _handler.Handle(CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.RecentTasks.Should().Be(1);
	}

	[Fact]
	public async Task Handle_WithTasksByStatus_ShouldGroupCorrectly()
	{
		// Arrange
		var board = CreateBoard("Test Board", "user1");
		await _context.Boards.AddAsync(board);
		await _context.SaveChangesAsync();

		var task1 = CreateTask("Task 1", board.Id, "user1", Domain.Tasks.TaskStatus.Complete);
		var task2 = CreateTask("Task 2", board.Id, "user1", Domain.Tasks.TaskStatus.Complete);
		var task3 = CreateTask("Task 3", board.Id, "user1", Domain.Tasks.TaskStatus.Incomplete);
		await _context.Tasks.AddRangeAsync(task1, task2, task3);
		await _context.SaveChangesAsync();

		// Act
		var result = await _handler.Handle(CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.TasksByStatus.Should().HaveCount(2);
		result.Value.TasksByStatus.Should().Contain(x => x.Status == "Complete" && x.Count == 2);
		result.Value.TasksByStatus.Should().Contain(x => x.Status == "Incomplete" && x.Count == 1);
	}

	[Fact]
	public async Task Handle_WithTasksByPriority_ShouldGroupCorrectly()
	{
		// Arrange
		var board = CreateBoard("Test Board", "user1");
		await _context.Boards.AddAsync(board);
		await _context.SaveChangesAsync();

		var task1 = CreateTask("Task 1", board.Id, "user1", priority: TaskPriority.High);
		var task2 = CreateTask("Task 2", board.Id, "user1", priority: TaskPriority.High);
		var task3 = CreateTask("Task 3", board.Id, "user1", priority: TaskPriority.Low);
		await _context.Tasks.AddRangeAsync(task1, task2, task3);
		await _context.SaveChangesAsync();

		// Act
		var result = await _handler.Handle(CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.TasksByPriority.Should().HaveCount(2);
		result.Value.TasksByPriority.Should().Contain(x => x.Priority == "High" && x.Count == 2);
		result.Value.TasksByPriority.Should().Contain(x => x.Priority == "Low" && x.Count == 1);
	}

	[Fact]
	public async Task Handle_WithTaskGrowth_ShouldReturnLast7Days()
	{
		// Arrange
		var board = CreateBoard("Test Board", "user1");
		await _context.Boards.AddAsync(board);
		await _context.SaveChangesAsync();

		var task = CreateTask("Task", board.Id, "user1");
		task.CreatedAtUtc = DateTime.UtcNow.Date;
		await _context.Tasks.AddAsync(task);
		await _context.SaveChangesAsync();

		// Act
		var result = await _handler.Handle(CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.TaskGrowth.Should().HaveCount(7);
	}

	public void Dispose()
	{
		_context.Dispose();
	}

	private static Board CreateBoard(string title, string userId)
	{
		var boardResult = Board.Create(title, userId);
		return boardResult.Value;
	}

	private static TaskItem CreateTask(
		string title,
		BoardId boardId,
		string userId,
		Domain.Tasks.TaskStatus status = Domain.Tasks.TaskStatus.Incomplete,
		TaskPriority priority = TaskPriority.Medium)
	{
		var taskResult = TaskItem.Create(title, null, priority, userId, boardId);
		var task = taskResult.Value;

		if (status != Domain.Tasks.TaskStatus.Incomplete)
		{
			task.UpdateStatus(status);
		}

		return task;
	}
}
