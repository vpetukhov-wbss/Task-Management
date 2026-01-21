using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Modules.Common.Infrastructure.Database;
using Modules.Tasks.Domain.Boards;
using Modules.Tasks.Domain.Tasks;
using Modules.Tasks.Features.Tasks.GetTaskById;
using Modules.Tasks.Infrastructure.Database;
using Modules.Tasks.Infrastructure.Database.Repositories;

namespace Modules.Tasks.Features.Tests.Tasks;

public sealed class GetTaskByIdHandlerTests : IDisposable
{
	private readonly TasksDbContext _context;
	private readonly ITaskRepository _taskRepository;
	private readonly IGetTaskByIdHandler _handler;

	public GetTaskByIdHandlerTests()
	{
		var options = new DbContextOptionsBuilder<TasksDbContext>()
			.UseInMemoryDatabase($"TasksTestDb_{Guid.NewGuid()}")
			.AddInterceptors(new AuditableInterceptor())
			.Options;

		_context = new TasksDbContext(options);
		_taskRepository = new TaskRepository(_context, NullLogger<TaskRepository>.Instance);

		_handler = new GetTaskByIdHandler(
			_taskRepository,
			NullLogger<GetTaskByIdHandler>.Instance);
	}

	public void Dispose()
	{
		_context.Database.EnsureDeleted();
		_context.Dispose();
	}

	[Fact]
	public async Task HandleAsync_WithExistingTask_ShouldReturnTask()
	{
		// Arrange
		var board = CreateBoard("Test Board", "user1");
		await _context.Boards.AddAsync(board);
		
		var task = CreateTask("Test Task", board.Id, "user1", TaskPriority.High);
		await _context.Tasks.AddAsync(task);
		await _context.SaveChangesAsync();

		// Act
		var result = await _handler.HandleAsync(task.Id.Value, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value!.Id.Should().Be(task.Id.Value);
		result.Value.Title.Should().Be("Test Task");
		result.Value.Priority.Should().Be((int)TaskPriority.High);
		result.Value.UserId.Should().Be("user1");
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
	public async Task HandleAsync_WithExistingTask_ShouldReturnCorrectDetails()
	{
		// Arrange
		var board = CreateBoard("Test Board", "user1");
		await _context.Boards.AddAsync(board);
		
		var task = CreateTask("Detailed Task", board.Id, "user1", TaskPriority.Low);
		task.Update("Detailed Task", "Test Description", TaskPriority.Low);
		await _context.Tasks.AddAsync(task);
		await _context.SaveChangesAsync();

		// Act
		var result = await _handler.HandleAsync(task.Id.Value, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value!.Description.Should().Be("Test Description");
		result.Value.BoardId.Should().Be(board.Id.Value);
		result.Value.Status.Should().Be((int)Domain.Tasks.TaskStatus.Incomplete);
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

