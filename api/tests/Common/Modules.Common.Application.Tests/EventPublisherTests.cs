using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Modules.Common.Application;
using Modules.Common.Domain.Events;
using Moq;

namespace Modules.Common.Application.Tests;

public sealed class EventPublisherTests
{
	private readonly Mock<IServiceProvider> _serviceProviderMock;
	private readonly Mock<ILogger<EventPublisher>> _loggerMock;
	private readonly EventPublisher _sut;

	public EventPublisherTests()
	{
		_serviceProviderMock = new Mock<IServiceProvider>();
		_loggerMock = new Mock<ILogger<EventPublisher>>();
		_sut = new EventPublisher(_serviceProviderMock.Object, _loggerMock.Object);
	}

	[Fact]
	public async Task PublishAsync_WithNoHandlers_ShouldNotThrow()
	{
		// Arrange
		var testEvent = new TestEvent("Test");
		_serviceProviderMock
			.Setup(sp => sp.GetService(typeof(IEnumerable<IEventHandler<TestEvent>>)))
			.Returns(Enumerable.Empty<IEventHandler<TestEvent>>());

		// Act
		var act = async () => await _sut.PublishAsync(testEvent, CancellationToken.None);

		// Assert
		await act.Should().NotThrowAsync();
	}

	[Fact]
	public async Task PublishAsync_WithSingleHandler_ShouldInvokeHandler()
	{
		// Arrange
		var testEvent = new TestEvent("Test");
		var handlerMock = new Mock<IEventHandler<TestEvent>>();

		_serviceProviderMock
			.Setup(sp => sp.GetService(typeof(IEnumerable<IEventHandler<TestEvent>>)))
			.Returns(new[] { handlerMock.Object });

		// Act
		await _sut.PublishAsync(testEvent, CancellationToken.None);

		// Assert
		handlerMock.Verify(h => h.HandleAsync(testEvent, It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task PublishAsync_WithMultipleHandlers_ShouldInvokeAllHandlers()
	{
		// Arrange
		var testEvent = new TestEvent("Test");
		var handler1Mock = new Mock<IEventHandler<TestEvent>>();
		var handler2Mock = new Mock<IEventHandler<TestEvent>>();
		var handler3Mock = new Mock<IEventHandler<TestEvent>>();

		_serviceProviderMock
			.Setup(sp => sp.GetService(typeof(IEnumerable<IEventHandler<TestEvent>>)))
			.Returns(new[] { handler1Mock.Object, handler2Mock.Object, handler3Mock.Object });

		// Act
		await _sut.PublishAsync(testEvent, CancellationToken.None);

		// Assert
		handler1Mock.Verify(h => h.HandleAsync(testEvent, It.IsAny<CancellationToken>()), Times.Once);
		handler2Mock.Verify(h => h.HandleAsync(testEvent, It.IsAny<CancellationToken>()), Times.Once);
		handler3Mock.Verify(h => h.HandleAsync(testEvent, It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task PublishAsync_WithCancellationToken_ShouldPassTokenToHandlers()
	{
		// Arrange
		var testEvent = new TestEvent("Test");
		var cancellationToken = new CancellationToken();
		var handlerMock = new Mock<IEventHandler<TestEvent>>();

		_serviceProviderMock
			.Setup(sp => sp.GetService(typeof(IEnumerable<IEventHandler<TestEvent>>)))
			.Returns(new[] { handlerMock.Object });

		// Act
		await _sut.PublishAsync(testEvent, cancellationToken);

		// Assert
		handlerMock.Verify(h => h.HandleAsync(testEvent, cancellationToken), Times.Once);
	}

	[Fact]
	public async Task PublishAsync_WhenHandlerThrows_ShouldPropagateException()
	{
		// Arrange
		var testEvent = new TestEvent("Test");
		var handlerMock = new Mock<IEventHandler<TestEvent>>();
		handlerMock
			.Setup(h => h.HandleAsync(It.IsAny<TestEvent>(), It.IsAny<CancellationToken>()))
			.ThrowsAsync(new InvalidOperationException("Handler failed"));

		_serviceProviderMock
			.Setup(sp => sp.GetService(typeof(IEnumerable<IEventHandler<TestEvent>>)))
			.Returns(new[] { handlerMock.Object });

		// Act
		var act = async () => await _sut.PublishAsync(testEvent, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<AggregateException>()
			.Where(ex => ex.InnerExceptions.Any(inner => inner is InvalidOperationException));
	}

	// Test event for testing purposes
	private sealed record TestEvent(string Message) : IEvent;
}

