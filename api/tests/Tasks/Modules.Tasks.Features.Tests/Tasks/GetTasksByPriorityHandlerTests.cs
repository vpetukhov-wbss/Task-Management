using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Modules.Common.Infrastructure.Database;
using Modules.Tasks.Domain.Boards;
using Modules.Tasks.Domain.Tasks;
using Modules.Tasks.Features.Tasks.GetTasksByPriority;
using Modules.Tasks.Infrastructure.Database;
using Modules.Tasks.Infrastructure.Database.Repositories;

namespace Modules.Tasks.Features.Tests.Tasks;

public sealed class GetTasksByPriorityHandlerTests : IDisposable
{
	private readonly TasksDbContext _context;
	private readonly ITaskRepository _taskRepository;
	private readonly IGetTasksByPriorityHandler _handler;

	public GetTasksByPriorityHandlerTests()
	{
		var options = new DbContextOptionsBuilder<TasksDbContext>()
			.UseInMemoryDatabase($"TasksTestDb_{Guid.NewGuid()}")
			.AddInterceptors(new AuditableInterceptor())
			.Options;

		_context = new TasksDbContext(options);
		_taskRepository = new TaskRepository(_context, NullLogger<TaskRepository>.Instance);

		_handler = new GetTasksByPriorityHandler(
			_taskRepository,
			NullLogger<GetTasksByPriorityHandler>.Instance);
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
		var result = await _handler.HandleAsync("user1", (int)TaskPriority.High, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().BeEmpty();
	}

	[Fact]
	public async Task HandleAsync_WithHighPriorityTasks_ShouldReturnOnlyHighPriorityTasks()
	{
		// Arrange
		var board = CreateBoard("Test Board", "user1");
		await _context.Boards.AddAsync(board);
		
		var task1 = CreateTask("High Task 1", board.Id, "user1", TaskPriority.High);
		var task2 = CreateTask("High Task 2", board.Id, "user1", TaskPriority.High);
		var task3 = CreateTask("Medium Task", board.Id, "user1", TaskPriority.Medium);
		await _context.Tasks.AddRangeAsync(task1, task2, task3);
		await _context.SaveChangesAsync();

		// Act
		var result = await _handler.HandleAsync("user1", (int)TaskPriority.High, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().HaveCount(2);
		result.Value.Should().OnlyContain(t => t.Priority == (int)TaskPriority.High);
	}

	[Fact]
	public async Task HandleAsync_WithMediumPriorityTasks_ShouldReturnOnlyMediumPriorityTasks()
	{
		// Arrange
		var board = CreateBoard("Test Board", "user1");
		await _context.Boards.AddAsync(board);
		
		var task1 = CreateTask("Medium Task 1", board.Id, "user1", TaskPriority.Medium);
		var task2 = CreateTask("Medium Task 2", board.Id, "user1", TaskPriority.Medium);
		var task3 = CreateTask("Low Task", board.Id, "user1", TaskPriority.Low);
		await _context.Tasks.AddRangeAsync(task1, task2, task3);
		await _context.SaveChangesAsync();

		// Act
		var result = await _handler.HandleAsync("user1", (int)TaskPriority.Medium, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().HaveCount(2);
		result.Value.Should().OnlyContain(t => t.Priority == (int)TaskPriority.Medium);
	}

	[Fact]
	public async Task HandleAsync_WithLowPriorityTasks_ShouldReturnOnlyLowPriorityTasks()
	{
		// Arrange
		var board = CreateBoard("Test Board", "user1");
		await _context.Boards.AddAsync(board);
		
		var task1 = CreateTask("Low Task 1", board.Id, "user1", TaskPriority.Low);
		var task2 = CreateTask("Low Task 2", board.Id, "user1", TaskPriority.Low);
		var task3 = CreateTask("High Task", board.Id, "user1", TaskPriority.High);
		await _context.Tasks.AddRangeAsync(task1, task2, task3);
		await _context.SaveChangesAsync();

		// Act
		var result = await _handler.HandleAsync("user1", (int)TaskPriority.Low, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().HaveCount(2);
		result.Value.Should().OnlyContain(t => t.Priority == (int)TaskPriority.Low);
	}

	[Fact]
	public async Task HandleAsync_WithInvalidPriority_ShouldReturnError()
	{
		// Act
		var result = await _handler.HandleAsync("user1", 999, CancellationToken.None);

		// Assert
		result.IsError.Should().BeTrue();
	}

	[Fact]
	public async Task HandleAsync_ShouldFilterByUserAndPriority()
	{
		// Arrange
		var board = CreateBoard("Test Board", "user1");
		await _context.Boards.AddAsync(board);
		
		var user1High = CreateTask("User1 High", board.Id, "user1", TaskPriority.High);
		var user1Low = CreateTask("User1 Low", board.Id, "user1", TaskPriority.Low);
		var user2High = CreateTask("User2 High", board.Id, "user2", TaskPriority.High);
		await _context.Tasks.AddRangeAsync(user1High, user1Low, user2High);
		await _context.SaveChangesAsync();

		// Act
		var result = await _handler.HandleAsync("user1", (int)TaskPriority.High, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().HaveCount(1);
		result.Value.First().UserId.Should().Be("user1");
		result.Value.First().Priority.Should().Be((int)TaskPriority.High);
	}

	private static Board CreateBoard(string name, string userId)
	{
		var boardResult = Board.Create(name, userId);
		return boardResult.Value!;
	}

	private static TaskItem CreateTask(string title, BoardId boardId, string userId, TaskPriority priority)
	{
		var taskResult = TaskItem.Create(title, null, priority, userId, boardId);
		return taskResult.Value!;
	}
}

