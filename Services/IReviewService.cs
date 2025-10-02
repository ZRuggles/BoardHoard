using Boardgames.Shared;

namespace BoardHoard.Services
{

    public interface IReviewsService
    {
        Task<List<Review>> GetRecentAsync(GameKind kind, int gameId, int take = 3, CancellationToken ct = default);
        Task<List<Review>> GetAllAsync(GameKind kind, int gameId);
        Task<Review?> GetMineAsync(GameKind kind, int gameId, CancellationToken ct = default);
        Task<Review?> UpsertAsync(GameKind kind, int gameId, double rating, string? title, string? body);
        Task DeleteMineAsync(GameKind kind, int gameId);
        Task<double> GetAverageAsync(GameKind kind, int gameId, CancellationToken ct = default);
        Task<int> GetCountAsync(GameKind kind, int gameId, CancellationToken ct = default);
        Task<List<Review>> GetTopByUserAsync(string userId, GameKind kind, int take = 3, CancellationToken ct = default);
        Task<List<Review>> GetAllByUserAsync(string userId, GameKind kind, CancellationToken ct = default);
    }
}
