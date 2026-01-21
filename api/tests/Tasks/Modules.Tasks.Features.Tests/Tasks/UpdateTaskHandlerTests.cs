using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Modules.Common.Infrastructure.Database;
using Modules.Tasks.Domain.Boards;
using Modules.Tasks.Domain.Tasks;
using Modules.Tasks.Features.Tasks.UpdateTask;
using Modules.Tasks.Infrastructure.Database;
using Modules.Tasks.Infrastructure.Database.Repositories;

namespace Modules.Tasks.Features.Tests.Tasks;

public sealed class UpdateTaskHandlerTests : IDisposable
{
	private readonly TasksDbContext _context;
	private readonly ITaskRepository _taskRepository;
	private readonly IUnitOfWork _unitOfWork;
	private readonly IUpdateTaskHandler _handler;

	public UpdateTaskHandlerTests()
	{
		var options = new DbContextOptionsBuilder<TasksDbContext>()
			.UseInMemoryDatabase($"TasksTestDb_{Guid.NewGuid()}")
			.AddInterceptors(new AuditableInterceptor())
			.Options;

		_context = new TasksDbContext(options);
		
		_taskRepository = new TaskRepository(_context, NullLogger<TaskRepository>.Instance);
		_unitOfWork = new UnitOfWork(_context);
		

		_handler = new UpdateTaskHandler(
			_taskRepository,
			_unitOfWork,
			NullLogger<UpdateTaskHandler>.Instance);
	}

	public void Dispose()
	{
		_context.Database.EnsureDeleted();
		_context.Dispose();
	}

	[Fact]
	public async Task HandleAsync_WithValidRequest_ShouldUpdateTask()
	{
		// Arrange
		var board = CreateBoard("Test Board", "user1");
		await _context.Boards.AddAsync(board);

		var task = CreateTask("Original Title", board.Id, "user1");
		await _context.Tasks.AddAsync(task);
		await _context.SaveChangesAsync();
		_context.ChangeTracker.Clear(); // Clear tracked entities to avoid conflicts

		var request = new UpdateTaskRequest(
			TaskId: task.Id.Value,
			Title: "Updated Title",
			Description: "Updated Description",
			Priority: (int)TaskPriority.High);

		// Act
		var result = await _handler.HandleAsync(request, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value!.Title.Should().Be("Updated Title");
		result.Value.Description.Should().Be("Updated Description");
		result.Value.Priority.Should().Be((int)TaskPriority.High);
	}

	[Fact]
	public async Task HandleAsync_WithValidRequest_ShouldPersistChanges()
	{
		// Arrange
		var board = CreateBoard("Test Board", "user1");
		await _context.Boards.AddAsync(board);

		var task = CreateTask("Original", board.Id, "user1");
		await _context.Tasks.AddAsync(task);
		await _context.SaveChangesAsync();
		_context.ChangeTracker.Clear(); // Clear tracked entities to avoid conflicts

		var request = new UpdateTaskRequest(
			TaskId: task.Id.Value,
			Title: "Persisted Update",
			Description: "New Description",
			Priority: (int)TaskPriority.Low);

		// Act
		await _handler.HandleAsync(request, CancellationToken.None);

		// Assert
		var updatedTask = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == task.Id);
		updatedTask.Should().NotBeNull();
		updatedTask!.Title.Should().Be("Persisted Update");
		updatedTask.Description.Should().Be("New Description");
		updatedTask.Priority.Should().Be(TaskPriority.Low);
	}

	[Fact]
	public async Task HandleAsync_WithNonExistentTask_ShouldReturnError()
	{
		// Arrange
		var request = new UpdateTaskRequest(
			TaskId: Guid.NewGuid(),
			Title: "Updated",
			Description: "Test",
			Priority: (int)TaskPriority.Medium);

		// Act
		var result = await _handler.HandleAsync(request, CancellationToken.None);

		// Assert
		result.IsError.Should().BeTrue();
	}

	[Fact]
	public async Task HandleAsync_WithEmptyTitle_ShouldReturnError()
	{
		// Arrange
		var board = CreateBoard("Test Board", "user1");
		await _context.Boards.AddAsync(board);
		
		var task = CreateTask("Original", board.Id, "user1");
		await _context.Tasks.AddAsync(task);
		await _context.SaveChangesAsync();

		var request = new UpdateTaskRequest(
			TaskId: task.Id.Value,
			Title: "",
			Description: "Test",
			Priority: (int)TaskPriority.Medium);

		// Act
		var result = await _handler.HandleAsync(request, CancellationToken.None);

		// Assert
		result.IsError.Should().BeTrue();
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

