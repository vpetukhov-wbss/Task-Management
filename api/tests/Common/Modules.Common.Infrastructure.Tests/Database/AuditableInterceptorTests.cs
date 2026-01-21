using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Modules.Common.Domain;
using Modules.Common.Infrastructure.Database;

namespace Modules.Common.Infrastructure.Tests.Database;

public sealed class AuditableInterceptorTests : IDisposable
{
	private readonly TestDbContext _dbContext;
	private readonly AuditableInterceptor _interceptor;

	public AuditableInterceptorTests()
	{
		_interceptor = new AuditableInterceptor();
		
		var options = new DbContextOptionsBuilder<TestDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.AddInterceptors(_interceptor)
			.Options;

		_dbContext = new TestDbContext(options);
	}

	[Fact]
	public async Task SaveChangesAsync_WhenEntityAdded_ShouldSetCreatedAtUtc()
	{
		// Arrange
		var entity = new TestAuditableEntity { Name = "Test" };
		var beforeAdd = DateTime.UtcNow;

		// Act
		_dbContext.TestEntities.Add(entity);
		await _dbContext.SaveChangesAsync();
		var afterAdd = DateTime.UtcNow;

		// Assert
		entity.CreatedAtUtc.Should().BeOnOrAfter(beforeAdd);
		entity.CreatedAtUtc.Should().BeOnOrBefore(afterAdd);
		entity.UpdatedAtUtc.Should().BeNull();
	}

	[Fact]
	public async Task SaveChangesAsync_WhenEntityModified_ShouldSetUpdatedAtUtc()
	{
		// Arrange
		var entity = new TestAuditableEntity { Name = "Original" };
		_dbContext.TestEntities.Add(entity);
		await _dbContext.SaveChangesAsync();

		var beforeUpdate = DateTime.UtcNow;
		await Task.Delay(10);

		// Act
		entity.Name = "Modified";
		_dbContext.TestEntities.Update(entity);
		await _dbContext.SaveChangesAsync();
		var afterUpdate = DateTime.UtcNow;

		// Assert
		entity.UpdatedAtUtc.Should().NotBeNull();
		entity.UpdatedAtUtc.Should().BeOnOrAfter(beforeUpdate);
		entity.UpdatedAtUtc.Should().BeOnOrBefore(afterUpdate);
		entity.UpdatedAtUtc.Should().BeAfter(entity.CreatedAtUtc);
	}

	[Fact]
	public async Task SaveChangesAsync_WhenEntityUnchanged_ShouldNotModifyTimestamps()
	{
		// Arrange
		var entity = new TestAuditableEntity { Name = "Test" };
		_dbContext.TestEntities.Add(entity);
		await _dbContext.SaveChangesAsync();

		var originalCreatedAt = entity.CreatedAtUtc;
		var originalUpdatedAt = entity.UpdatedAtUtc;

		// Act
		await _dbContext.SaveChangesAsync();

		// Assert
		entity.CreatedAtUtc.Should().Be(originalCreatedAt);
		entity.UpdatedAtUtc.Should().Be(originalUpdatedAt);
	}

	[Fact]
	public async Task SaveChangesAsync_WithMultipleEntities_ShouldSetTimestampsForAll()
	{
		// Arrange
		var entity1 = new TestAuditableEntity { Name = "Entity1" };
		var entity2 = new TestAuditableEntity { Name = "Entity2" };
		var entity3 = new TestAuditableEntity { Name = "Entity3" };

		// Act
		_dbContext.TestEntities.AddRange(entity1, entity2, entity3);
		await _dbContext.SaveChangesAsync();

		// Assert
		entity1.CreatedAtUtc.Should().NotBe(default);
		entity2.CreatedAtUtc.Should().NotBe(default);
		entity3.CreatedAtUtc.Should().NotBe(default);
	}

	[Fact]
	public async Task SaveChangesAsync_WhenNonAuditableEntity_ShouldNotThrow()
	{
		// Arrange
		var entity = new NonAuditableEntity { Name = "Test" };

		// Act
		var act = async () =>
		{
			_dbContext.NonAuditableEntities.Add(entity);
			await _dbContext.SaveChangesAsync();
		};

		// Assert
		await act.Should().NotThrowAsync();
	}

	public void Dispose()
	{
		_dbContext.Dispose();
	}

	// Test entities
	private sealed class TestAuditableEntity : IAuditableEntity
	{
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public DateTime CreatedAtUtc { get; set; }
		public DateTime? UpdatedAtUtc { get; set; }
	}

	private sealed class NonAuditableEntity
	{
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
	}

	// Test DbContext
	private sealed class TestDbContext : DbContext
	{
		public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

		public DbSet<TestAuditableEntity> TestEntities => Set<TestAuditableEntity>();
		public DbSet<NonAuditableEntity> NonAuditableEntities => Set<NonAuditableEntity>();

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<TestAuditableEntity>().HasKey(e => e.Id);
			modelBuilder.Entity<NonAuditableEntity>().HasKey(e => e.Id);
		}
	}
}

