using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Common.Domain.Results;
using Modules.Tasks.Domain.Boards;

namespace Modules.Tasks.Infrastructure.Database.Repositories;

public sealed class BoardRepository : IBoardRepository
{
    private readonly TasksDbContext _context;
    private readonly ILogger<BoardRepository> _logger;

    public BoardRepository(TasksDbContext context, ILogger<BoardRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<Board>> GetByIdAsync(BoardId id, CancellationToken cancellationToken = default)
    {
        try
        {
            var board = await _context.Boards
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

            return board ?? (Result<Board>)BoardErrors.NotFound();
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Database connection error while getting board {BoardId}", id.Value);
            return BoardErrors.NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while getting board {BoardId}", id.Value);
            return BoardErrors.NotFound();
        }
    }

    public async Task<IReadOnlyList<Board>> GetByUserIdAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Boards
                .AsNoTracking()
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreatedAtUtc)
                .ToListAsync(cancellationToken);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Database connection error while getting boards for user {UserId}", userId);
            return Array.Empty<Board>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while getting boards for user {UserId}", userId);
            return Array.Empty<Board>();
        }
    }

    public async Task<IReadOnlyList<Board>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Boards
                .AsNoTracking()
                .OrderByDescending(b => b.CreatedAtUtc)
                .ToListAsync(cancellationToken);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Database connection error while getting all boards");
            return Array.Empty<Board>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while getting all boards");
            return Array.Empty<Board>();
        }
    }

    public async Task AddAsync(Board board, CancellationToken cancellationToken = default)
    {
        await _context.Boards.AddAsync(board, cancellationToken);
    }

    public void Update(Board board)
    {
        _context.Boards.Update(board);
    }

    public void Delete(Board board)
    {
        _context.Boards.Remove(board);
    }

    public async Task<bool> ExistsAsync(BoardId id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Boards
                .AsNoTracking()
                .AnyAsync(b => b.Id == id, cancellationToken);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Database connection error while checking if board {BoardId} exists", id.Value);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while checking if board {BoardId} exists", id.Value);
            return false;
        }
    }
}

