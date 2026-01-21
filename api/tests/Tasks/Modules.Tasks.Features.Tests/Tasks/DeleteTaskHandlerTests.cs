using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Modules.Common.Infrastructure.Database;
using Modules.Tasks.Domain.Boards;
using Modules.Tasks.Domain.Tasks;
using Modules.Tasks.Features.Tasks.DeleteTask;
using Modules.Tasks.Infrastructure.Database;
using Modules.Tasks.Infrastructure.Database.Repositories;

namespace Modules.Tasks.Features.Tests.Tasks;

public sealed class DeleteTaskHandlerTests : IDisposable
{
	private readonly TasksDbContext _context;
	private readonly ITaskRepository _taskRepository;
	private readonly IUnitOfWork _unitOfWork;
	private readonly IDeleteTaskHandler _handler;

	public DeleteTaskHandlerTests()
	{
		var options = new DbContextOptionsBuilder<TasksDbContext>()
			.UseInMemoryDatabase($"TasksTestDb_{Guid.NewGuid()}")
			.AddInterceptors(new AuditableInterceptor())
			.Options;

		_context = new TasksDbContext(options);
		_taskRepository = new TaskRepository(_context, NullLogger<TaskRepository>.Instance);
		_unitOfWork = new UnitOfWork(_context);

		_handler = new DeleteTaskHandler(
			_taskRepository,
			_unitOfWork,
			NullLogger<DeleteTaskHandler>.Instance);
	}

	public void Dispose()
	{
		_context.Database.EnsureDeleted();
		_context.Dispose();
	}

	[Fact]
	public async Task HandleAsync_WithExistingTask_ShouldDeleteTask()
	{
		// Arrange
		var board = CreateBoard("Test Board", "user1");
		await _context.Boards.AddAsync(board);

		var task = CreateTask("Task to Delete", board.Id, "user1");
		await _context.Tasks.AddAsync(task);
		await _context.SaveChangesAsync();
		_context.ChangeTracker.Clear(); // Clear tracked entities to avoid conflicts

		// Act
		var result = await _handler.HandleAsync(task.Id.Value, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
	}

	[Fact]
	public async Task HandleAsync_WithExistingTask_ShouldRemoveFromDatabase()
	{
		// Arrange
		var board = CreateBoard("Test Board", "user1");
		await _context.Boards.AddAsync(board);

		var task = CreateTask("Task to Delete", board.Id, "user1");
		await _context.Tasks.AddAsync(task);
		await _context.SaveChangesAsync();
		_context.ChangeTracker.Clear(); // Clear tracked entities to avoid conflicts

		var taskId = task.Id;

		// Act
		await _handler.HandleAsync(taskId.Value, CancellationToken.None);

		// Assert
		var deletedTask = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == taskId);
		deletedTask.Should().BeNull();
	}

	[Fact]
	public async Task HandleAsync_WithNonExistentTask_ShouldReturnError()
	{
		// Arrange
		var nonExistentTaskId = Guid.NewGuid();

		// Act
		var result = await _handler.HandleAsync(nonExistentTaskId, CancellationToken.None);

		// Assert
		result.IsError.Should().BeTrue();
	}

	[Fact]
	public async Task HandleAsync_WithMultipleTasks_ShouldDeleteOnlySpecifiedTask()
	{
		// Arrange
		var board = CreateBoard("Test Board", "user1");
		await _context.Boards.AddAsync(board);

		var task1 = CreateTask("Task 1", board.Id, "user1");
		var task2 = CreateTask("Task 2", board.Id, "user1");
		var task3 = CreateTask("Task 3", board.Id, "user1");
		await _context.Tasks.AddRangeAsync(task1, task2, task3);
		await _context.SaveChangesAsync();
		_context.ChangeTracker.Clear(); // Clear tracked entities to avoid conflicts

		// Act
		await _handler.HandleAsync(task2.Id.Value, CancellationToken.None);

		// Assert
		var remainingTasks = await _context.Tasks.ToListAsync();
		remainingTasks.Should().HaveCount(2);
		remainingTasks.Should().Contain(t => t.Id == task1.Id);
		remainingTasks.Should().Contain(t => t.Id == task3.Id);
		remainingTasks.Should().NotContain(t => t.Id == task2.Id);
	}

	private static Board CreateBoard(string name, string userId)
	{
		var boardResult = Board.Create(name, userId);
		return boardResult.Value!;
	}

	private static TaskItem CreateTask(string title, BoardId boardId, string userId)
	{
		var taskResult = TaskItem.Create(title, null, TaskPriority.Medium, userId, boardId);
		return taskResult.Value!;
	}
}

