using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Modules.Common.Infrastructure.Database;
using Modules.Tasks.Domain.Boards;
using Modules.Tasks.Domain.Tasks;
using Modules.Tasks.Features.Tasks.GetTasksByStatus;
using Modules.Tasks.Infrastructure.Database;
using Modules.Tasks.Infrastructure.Database.Repositories;

namespace Modules.Tasks.Features.Tests.Tasks;

public sealed class GetTasksByStatusHandlerTests : IDisposable
{
	private readonly TasksDbContext _context;
	private readonly ITaskRepository _taskRepository;
	private readonly IGetTasksByStatusHandler _handler;

	public GetTasksByStatusHandlerTests()
	{
		var options = new DbContextOptionsBuilder<TasksDbContext>()
			.UseInMemoryDatabase($"TasksTestDb_{Guid.NewGuid()}")
			.AddInterceptors(new AuditableInterceptor())
			.Options;

		_context = new TasksDbContext(options);
		_taskRepository = new TaskRepository(_context, NullLogger<TaskRepository>.Instance);

		_handler = new GetTasksByStatusHandler(
			_taskRepository,
			NullLogger<GetTasksByStatusHandler>.Instance);
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
		var result = await _handler.HandleAsync("user1", (int)Domain.Tasks.TaskStatus.Incomplete, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().BeEmpty();
	}

	[Fact]
	public async Task HandleAsync_WithIncompleteTasks_ShouldReturnOnlyIncompleteTasks()
	{
		// Arrange
		var board = CreateBoard("Test Board", "user1");
		await _context.Boards.AddAsync(board);
		
		var task1 = CreateTask("Incomplete Task 1", board.Id, "user1", Domain.Tasks.TaskStatus.Incomplete);
		var task2 = CreateTask("Incomplete Task 2", board.Id, "user1", Domain.Tasks.TaskStatus.Incomplete);
		var task3 = CreateTask("Complete Task", board.Id, "user1", Domain.Tasks.TaskStatus.Complete);
		await _context.Tasks.AddRangeAsync(task1, task2, task3);
		await _context.SaveChangesAsync();

		// Act
		var result = await _handler.HandleAsync("user1", (int)Domain.Tasks.TaskStatus.Incomplete, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().HaveCount(2);
		result.Value.Should().OnlyContain(t => t.Status == (int)Domain.Tasks.TaskStatus.Incomplete);
	}

	[Fact]
	public async Task HandleAsync_WithCompleteTasks_ShouldReturnOnlyCompleteTasks()
	{
		// Arrange
		var board = CreateBoard("Test Board", "user1");
		await _context.Boards.AddAsync(board);
		
		var task1 = CreateTask("Complete Task 1", board.Id, "user1", Domain.Tasks.TaskStatus.Complete);
		var task2 = CreateTask("Complete Task 2", board.Id, "user1", Domain.Tasks.TaskStatus.Complete);
		var task3 = CreateTask("Incomplete Task", board.Id, "user1", Domain.Tasks.TaskStatus.Incomplete);
		await _context.Tasks.AddRangeAsync(task1, task2, task3);
		await _context.SaveChangesAsync();

		// Act
		var result = await _handler.HandleAsync("user1", (int)Domain.Tasks.TaskStatus.Complete, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().HaveCount(2);
		result.Value.Should().OnlyContain(t => t.Status == (int)Domain.Tasks.TaskStatus.Complete);
	}

	[Fact]
	public async Task HandleAsync_WithInvalidStatus_ShouldReturnError()
	{
		// Act
		var result = await _handler.HandleAsync("user1", 999, CancellationToken.None);

		// Assert
		result.IsError.Should().BeTrue();
	}

	[Fact]
	public async Task HandleAsync_ShouldFilterByUserAndStatus()
	{
		// Arrange
		var board = CreateBoard("Test Board", "user1");
		await _context.Boards.AddAsync(board);
		
		var user1Incomplete = CreateTask("User1 Incomplete", board.Id, "user1", Domain.Tasks.TaskStatus.Incomplete);
		var user1Complete = CreateTask("User1 Complete", board.Id, "user1", Domain.Tasks.TaskStatus.Complete);
		var user2Incomplete = CreateTask("User2 Incomplete", board.Id, "user2", Domain.Tasks.TaskStatus.Incomplete);
		await _context.Tasks.AddRangeAsync(user1Incomplete, user1Complete, user2Incomplete);
		await _context.SaveChangesAsync();

		// Act
		var result = await _handler.HandleAsync("user1", (int)Domain.Tasks.TaskStatus.Incomplete, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().HaveCount(1);
		result.Value.First().UserId.Should().Be("user1");
		result.Value.First().Status.Should().Be((int)Domain.Tasks.TaskStatus.Incomplete);
	}

	private static Board CreateBoard(string name, string userId)
	{
		var boardResult = Board.Create(name, userId);
		return boardResult.Value!;
	}

	private static TaskItem CreateTask(string title, BoardId boardId, string userId, Domain.Tasks.TaskStatus status)
	{
		var taskResult = TaskItem.Create(title, null, TaskPriority.Medium, userId, boardId);
		var task = taskResult.Value!;
		
		if (status != Domain.Tasks.TaskStatus.Incomplete)
		{
			task.UpdateStatus(status);
		}
		
		return task;
	}
}

