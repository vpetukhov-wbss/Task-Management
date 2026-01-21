using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Modules.Common.Domain.Events;
using Modules.Common.Infrastructure.Database;
using Modules.Tasks.Domain.Boards;
using Modules.Tasks.Domain.Tasks;
using Modules.Tasks.Domain.Tasks.Events;
using Modules.Tasks.Features.Tasks.CreateTask;
using Modules.Tasks.Infrastructure.Database;
using Modules.Tasks.Infrastructure.Database.Repositories;

namespace Modules.Tasks.Features.Tests.Tasks;

public sealed class CreateTaskHandlerTests : IDisposable
{
	private readonly TasksDbContext _context;
	private readonly ITaskRepository _taskRepository;
	private readonly IUnitOfWork _unitOfWork;
	private readonly Mock<IEventPublisher> _eventPublisherMock;
	private readonly ICreateTaskHandler _handler;

	public CreateTaskHandlerTests()
	{
		var options = new DbContextOptionsBuilder<TasksDbContext>()
			.UseInMemoryDatabase($"TasksTestDb_{Guid.NewGuid()}")
			.AddInterceptors(new AuditableInterceptor())
			.Options;

		_context = new TasksDbContext(options);
		_taskRepository = new TaskRepository(_context, NullLogger<TaskRepository>.Instance);
		_unitOfWork = new UnitOfWork(_context);
		_eventPublisherMock = new Mock<IEventPublisher>();

		_handler = new CreateTaskHandler(
			_taskRepository,
			_unitOfWork,
			_eventPublisherMock.Object,
			NullLogger<CreateTaskHandler>.Instance);
	}

	public void Dispose()
	{
		_context.Database.EnsureDeleted();
		_context.Dispose();
	}

	[Fact]
	public async Task HandleAsync_WithValidRequest_ShouldCreateTask()
	{
		// Arrange
		var board = CreateBoard("Test Board", "user1");
		await _context.Boards.AddAsync(board);
		await _context.SaveChangesAsync();

		var request = new CreateTaskRequest(
			Title: "Test Task",
			Description: "Test Description",
			Priority: (int)TaskPriority.High,
			UserId: "user1",
			BoardId: board.Id.Value);

		// Act
		var result = await _handler.HandleAsync(request, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value!.Title.Should().Be("Test Task");
		result.Value.Description.Should().Be("Test Description");
		result.Value.Priority.Should().Be((int)TaskPriority.High);
		result.Value.UserId.Should().Be("user1");
		result.Value.BoardId.Should().Be(board.Id.Value);
		result.Value.Status.Should().Be((int)Domain.Tasks.TaskStatus.Incomplete);
	}

	[Fact]
	public async Task HandleAsync_WithValidRequest_ShouldPersistToDatabase()
	{
		// Arrange
		var board = CreateBoard("Test Board", "user1");
		await _context.Boards.AddAsync(board);
		await _context.SaveChangesAsync();

		var request = new CreateTaskRequest(
			Title: "Persisted Task",
			Description: null,
			Priority: (int)TaskPriority.Medium,
			UserId: "user1",
			BoardId: board.Id.Value);

		// Act
		var result = await _handler.HandleAsync(request, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		
		var taskInDb = await _context.Tasks.FirstOrDefaultAsync(t => t.Id.Value == result.Value!.Id);
		taskInDb.Should().NotBeNull();
		taskInDb!.Title.Should().Be("Persisted Task");
		taskInDb.Description.Should().BeNull();
	}

	[Fact]
	public async Task HandleAsync_WithValidRequest_ShouldPublishEvent()
	{
		// Arrange
		var board = CreateBoard("Test Board", "user1");
		await _context.Boards.AddAsync(board);
		await _context.SaveChangesAsync();

		var request = new CreateTaskRequest(
			Title: "Event Task",
			Description: "Test",
			Priority: (int)TaskPriority.Low,
			UserId: "user1",
			BoardId: board.Id.Value);

		// Act
		var result = await _handler.HandleAsync(request, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		_eventPublisherMock.Verify(
			x => x.PublishAsync(It.IsAny<TaskCreatedEvent>(), It.IsAny<CancellationToken>()),
			Times.Once);
	}

	[Fact]
	public async Task HandleAsync_WithEmptyTitle_ShouldReturnError()
	{
		// Arrange
		var board = CreateBoard("Test Board", "user1");
		await _context.Boards.AddAsync(board);
		await _context.SaveChangesAsync();

		var request = new CreateTaskRequest(
			Title: "",
			Description: "Test",
			Priority: (int)TaskPriority.Medium,
			UserId: "user1",
			BoardId: board.Id.Value);

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
}

