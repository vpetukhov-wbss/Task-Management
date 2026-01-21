using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Modules.Common.Infrastructure.Database;
using Modules.Tasks.Domain.Boards;
using Modules.Tasks.Domain.Tasks;
using Modules.Tasks.Features.Tasks.GetTasksByBoard;
using Modules.Tasks.Infrastructure.Database;
using Modules.Tasks.Infrastructure.Database.Repositories;

namespace Modules.Tasks.Features.Tests.Tasks;

public sealed class GetTasksByBoardHandlerTests : IDisposable
{
	private readonly TasksDbContext _context;
	private readonly ITaskRepository _taskRepository;
	private readonly IBoardRepository _boardRepository;
	private readonly IGetTasksByBoardHandler _handler;

	public GetTasksByBoardHandlerTests()
	{
		var options = new DbContextOptionsBuilder<TasksDbContext>()
			.UseInMemoryDatabase($"TasksTestDb_{Guid.NewGuid()}")
			.AddInterceptors(new AuditableInterceptor())
			.Options;

		_context = new TasksDbContext(options);
		_taskRepository = new TaskRepository(_context, NullLogger<TaskRepository>.Instance);
		_boardRepository = new BoardRepository(_context, NullLogger<BoardRepository>.Instance);

		_handler = new GetTasksByBoardHandler(
			_taskRepository,
			_boardRepository,
			NullLogger<GetTasksByBoardHandler>.Instance);
	}

	public void Dispose()
	{
		_context.Database.EnsureDeleted();
		_context.Dispose();
	}

	[Fact]
	public async Task HandleAsync_WithNonExistentBoard_ShouldReturnError()
	{
		// Arrange
		var nonExistentBoardId = Guid.NewGuid();

		// Act
		var result = await _handler.HandleAsync(nonExistentBoardId, CancellationToken.None);

		// Assert
		result.IsError.Should().BeTrue();
	}

	[Fact]
	public async Task HandleAsync_WithEmptyBoard_ShouldReturnEmptyList()
	{
		// Arrange
		var board = CreateBoard("Empty Board", "user1");
		await _context.Boards.AddAsync(board);
		await _context.SaveChangesAsync();

		// Act
		var result = await _handler.HandleAsync(board.Id.Value, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().BeEmpty();
	}

	[Fact]
	public async Task HandleAsync_WithBoardTasks_ShouldReturnOnlyBoardTasks()
	{
		// Arrange
		var board1 = CreateBoard("Board 1", "user1");
		var board2 = CreateBoard("Board 2", "user1");
		await _context.Boards.AddRangeAsync(board1, board2);
		await _context.SaveChangesAsync();

		var task1 = CreateTask("Board1 Task 1", board1.Id, "user1");
		var task2 = CreateTask("Board1 Task 2", board1.Id, "user1");
		var task3 = CreateTask("Board2 Task", board2.Id, "user1");
		await _context.Tasks.AddRangeAsync(task1, task2, task3);
		await _context.SaveChangesAsync();

		// Act
		var result = await _handler.HandleAsync(board1.Id.Value, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().HaveCount(2);
		result.Value.Should().OnlyContain(t => t.BoardId == board1.Id.Value);
	}

	[Fact]
	public async Task HandleAsync_WithMultipleUsers_ShouldReturnAllTasksForBoard()
	{
		// Arrange
		var board = CreateBoard("Shared Board", "user1");
		await _context.Boards.AddAsync(board);
		await _context.SaveChangesAsync();

		var task1 = CreateTask("User1 Task", board.Id, "user1");
		var task2 = CreateTask("User2 Task", board.Id, "user2");
		var task3 = CreateTask("User3 Task", board.Id, "user3");
		await _context.Tasks.AddRangeAsync(task1, task2, task3);
		await _context.SaveChangesAsync();

		// Act
		var result = await _handler.HandleAsync(board.Id.Value, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().HaveCount(3);
		result.Value.Should().Contain(t => t.UserId == "user1");
		result.Value.Should().Contain(t => t.UserId == "user2");
		result.Value.Should().Contain(t => t.UserId == "user3");
	}

	[Fact]
	public async Task HandleAsync_WithMixedPriorities_ShouldReturnAllTasksForBoard()
	{
		// Arrange
		var board = CreateBoard("Test Board", "user1");
		await _context.Boards.AddAsync(board);
		await _context.SaveChangesAsync();

		var task1 = CreateTask("High Task", board.Id, "user1", TaskPriority.High);
		var task2 = CreateTask("Medium Task", board.Id, "user1", TaskPriority.Medium);
		var task3 = CreateTask("Low Task", board.Id, "user1", TaskPriority.Low);
		await _context.Tasks.AddRangeAsync(task1, task2, task3);
		await _context.SaveChangesAsync();

		// Act
		var result = await _handler.HandleAsync(board.Id.Value, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().HaveCount(3);
		result.Value.Should().Contain(t => t.Priority == (int)TaskPriority.High);
		result.Value.Should().Contain(t => t.Priority == (int)TaskPriority.Medium);
		result.Value.Should().Contain(t => t.Priority == (int)TaskPriority.Low);
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

