using Microsoft.EntityFrameworkCore;
using Modules.Tasks.Domain.Boards;
using Modules.Tasks.Domain.Tasks;

namespace Modules.Tasks.Infrastructure.Database;

public sealed class TasksDbContext : DbContext
{
    public DbSet<Board> Boards { get; set; } = null!;
    public DbSet<TaskItem> Tasks { get; set; } = null!;

    public TasksDbContext(DbContextOptions<TasksDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(DbConsts.Schema);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TasksDbContext).Assembly);
    }
}

