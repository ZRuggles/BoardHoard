using Boardgames.Shared;

namespace BoardHoard.Services;

public interface ICardGameService
{
    Task<List<CardGame>> GetAllAsync(CancellationToken ct = default);
    Task<CardGame?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<CardGame?> CreateAsync(CardGame game, CancellationToken ct = default);
    Task<bool> UpdateAsync(int id, CardGame game, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
