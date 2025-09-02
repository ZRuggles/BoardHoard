using Boardgames.Shared;

namespace BoardHoard.Services;

public interface IBoardGameService
{
    Task<List<BoardGame>> GetAllAsync(CancellationToken ct = default);
    Task<BoardGame?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<BoardGame?> CreateAsync(BoardGame game, CancellationToken ct = default);
    Task<bool> UpdateAsync(int id, BoardGame game, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
