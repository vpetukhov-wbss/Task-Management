using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Common.Domain.Results;
using Modules.Tasks.Domain.Boards;
using Modules.Tasks.Domain.Tasks;
using TaskStatus = Modules.Tasks.Domain.Tasks.TaskStatus;

namespace Modules.Tasks.Infrastructure.Database.Repositories;

public sealed class TaskRepository : ITaskRepository
{
    private readonly TasksDbContext _context;
    private readonly ILogger<TaskRepository> _logger;

    public TaskRepository(TasksDbContext context, ILogger<TaskRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<TaskItem>> GetByIdAsync(TaskId id, CancellationToken cancellationToken = default)
    {
        try
        {
            var task = await _context.Tasks
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

            return task ?? (Result<TaskItem>)TaskErrors.NotFound(id.Value);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Database connection error while getting task {TaskId}", id.Value);
            return TaskErrors.NotFound(id.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while getting task {TaskId}", id.Value);
            return TaskErrors.NotFound(id.Value);
        }
    }

    public async Task<IReadOnlyList<TaskItem>> GetByUserIdAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Tasks
                .AsNoTracking()
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAtUtc)
                .ToListAsync(cancellationToken);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Database connection error while getting tasks for user {UserId}", userId);
            return Array.Empty<TaskItem>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while getting tasks for user {UserId}", userId);
            return Array.Empty<TaskItem>();
        }
    }

    public async Task<IReadOnlyList<TaskItem>> GetByUserIdAndStatusAsync(
        string userId,
        TaskStatus status,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Tasks
                .AsNoTracking()
                .Where(t => t.UserId == userId && t.Status == status)
                .OrderByDescending(t => t.CreatedAtUtc)
                .ToListAsync(cancellationToken);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Database connection error while getting tasks for user {UserId} with status {Status}", userId, status);
            return Array.Empty<TaskItem>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while getting tasks for user {UserId} with status {Status}", userId, status);
            return Array.Empty<TaskItem>();
        }
    }

    public async Task<IReadOnlyList<TaskItem>> GetByUserIdAndPriorityAsync(
        string userId,
        TaskPriority priority,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Tasks
                .AsNoTracking()
                .Where(t => t.UserId == userId && t.Priority == priority)
                .OrderByDescending(t => t.CreatedAtUtc)
                .ToListAsync(cancellationToken);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Database connection error while getting tasks for user {UserId} with priority {Priority}", userId, priority);
            return Array.Empty<TaskItem>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while getting tasks for user {UserId} with priority {Priority}", userId, priority);
            return Array.Empty<TaskItem>();
        }
    }

    public async Task<IReadOnlyList<TaskItem>> GetByBoardIdAsync(
        BoardId boardId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Tasks
                .AsNoTracking()
                .Where(t => t.BoardId == boardId)
                .OrderByDescending(t => t.CreatedAtUtc)
                .ToListAsync(cancellationToken);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Database connection error while getting tasks for board {BoardId}", boardId.Value);
            return Array.Empty<TaskItem>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while getting tasks for board {BoardId}", boardId.Value);
            return Array.Empty<TaskItem>();
        }
    }

    public async Task AddAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        await _context.Tasks.AddAsync(task, cancellationToken);
    }

    public void Update(TaskItem task)
    {
        _context.Tasks.Update(task);
    }

    public void Delete(TaskItem task)
    {
        _context.Tasks.Remove(task);
    }

    public async Task<bool> ExistsAsync(TaskId id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Tasks
                .AsNoTracking()
                .AnyAsync(t => t.Id == id, cancellationToken);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Database connection error while checking if task {TaskId} exists", id.Value);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while checking if task {TaskId} exists", id.Value);
            return false;
        }
    }
}

