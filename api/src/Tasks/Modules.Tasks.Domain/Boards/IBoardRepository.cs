using Modules.Common.Domain.Results;

namespace Modules.Tasks.Domain.Boards;

public interface IBoardRepository
{
    Task<Result<Board>> GetByIdAsync(BoardId id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Board>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Board>> GetAllAsync(CancellationToken cancellationToken = default);

    Task AddAsync(Board board, CancellationToken cancellationToken = default);

    void Update(Board board);

    void Delete(Board board);

    Task<bool> ExistsAsync(BoardId id, CancellationToken cancellationToken = default);
}

