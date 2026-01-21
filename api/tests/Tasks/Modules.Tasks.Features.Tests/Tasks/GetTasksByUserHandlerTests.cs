using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Modules.Common.Infrastructure.Database;
using Modules.Tasks.Domain.Boards;
using Modules.Tasks.Domain.Tasks;
using Modules.Tasks.Features.Tasks.GetTasksByUser;
using Modules.Tasks.Infrastructure.Database;
using Modules.Tasks.Infrastructure.Database.Repositories;

namespace Modules.Tasks.Features.Tests.Tasks;

public sealed class GetTasksByUserHandlerTests : IDisposable
{
	private readonly TasksDbContext _context;
	private readonly ITaskRepository _taskRepository;
	private readonly IGetTasksByUserHandler _handler;

	public GetTasksByUserHandlerTests()
	{
		var options = new DbContextOptionsBuilder<TasksDbContext>()
			.UseInMemoryDatabase($"TasksTestDb_{Guid.NewGuid()}")
			.AddInterceptors(new AuditableInterceptor())
			.Options;

		_context = new TasksDbContext(options);
		_taskRepository = new TaskRepository(_context, NullLogger<TaskRepository>.Instance);

		_handler = new GetTasksByUserHandler(
			_taskRepository,
			NullLogger<GetTasksByUserHandler>.Instance);
	}

	public void Dispose()
	{
		_context.Database.EnsureDeleted();
		_context.Dispose();
	}

	[Fact]
	public async Task HandleAsync_WithNoTasks_ShouldReturnEmptyList()
	{
		// Act
		var result = await _handler.HandleAsync("user1", CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().BeEmpty();
	}

	[Fact]
	public async Task HandleAsync_WithUserTasks_ShouldReturnOnlyUserTasks()
	{
		// Arrange
		var board = CreateBoard("Test Board", "user1");
		await _context.Boards.AddAsync(board);
		
		var task1 = CreateTask("User1 Task 1", board.Id, "user1");
		var task2 = CreateTask("User1 Task 2", board.Id, "user1");
		var task3 = CreateTask("User2 Task", board.Id, "user2");
		await _context.Tasks.AddRangeAsync(task1, task2, task3);
		await _context.SaveChangesAsync();

		// Act
		var result = await _handler.HandleAsync("user1", CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().HaveCount(2);
		result.Value.Should().OnlyContain(t => t.UserId == "user1");
	}

	[Fact]
	public async Task HandleAsync_WithMultipleTasks_ShouldReturnAllUserTasks()
	{
		// Arrange
		var board = CreateBoard("Test Board", "user1");
		await _context.Boards.AddAsync(board);
		
		var task1 = CreateTask("Task 1", board.Id, "user1", TaskPriority.High);
		var task2 = CreateTask("Task 2", board.Id, "user1", TaskPriority.Medium);
		var task3 = CreateTask("Task 3", board.Id, "user1", TaskPriority.Low);
		await _context.Tasks.AddRangeAsync(task1, task2, task3);
		await _context.SaveChangesAsync();

		// Act
		var result = await _handler.HandleAsync("user1", CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().HaveCount(3);
		result.Value.Should().Contain(t => t.Title == "Task 1");
		result.Value.Should().Contain(t => t.Title == "Task 2");
		result.Value.Should().Contain(t => t.Title == "Task 3");
	}

	[Fact]
	public async Task HandleAsync_WithDifferentUsers_ShouldIsolateUserTasks()
	{
		// Arrange
		var board = CreateBoard("Test Board", "user1");
		await _context.Boards.AddAsync(board);
		
		var user1Task1 = CreateTask("User1 Task 1", board.Id, "user1");
		var user1Task2 = CreateTask("User1 Task 2", board.Id, "user1");
		var user2Task1 = CreateTask("User2 Task 1", board.Id, "user2");
		var user2Task2 = CreateTask("User2 Task 2", board.Id, "user2");
		var user3Task = CreateTask("User3 Task", board.Id, "user3");
		await _context.Tasks.AddRangeAsync(user1Task1, user1Task2, user2Task1, user2Task2, user3Task);
		await _context.SaveChangesAsync();

		// Act
		var user1Result = await _handler.HandleAsync("user1", CancellationToken.None);
		var user2Result = await _handler.HandleAsync("user2", CancellationToken.None);
		var user3Result = await _handler.HandleAsync("user3", CancellationToken.None);

		// Assert
		user1Result.Value.Should().HaveCount(2);
		user2Result.Value.Should().HaveCount(2);
		user3Result.Value.Should().HaveCount(1);
	}

	private static Board CreateBoard(string name, string userId)
	{
		var boardResult = Board.Create(name, userId);
		return boardResult.Value!;
	}

	private static TaskItem CreateTask(string title, BoardId boardId, string userId, TaskPriority priority = TaskPriority.Medium)
	{
		var taskResult = TaskItem.Create(title, null, priority, userId, boardId);
		return taskResult.Value!;
	}
}

