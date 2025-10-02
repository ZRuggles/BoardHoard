using System.Net.Http.Json;
using Boardgames.Shared;

namespace BoardHoard.Services;

public class BoardGameService(HttpClient http) : IBoardGameService
{
    private readonly HttpClient _http = http;
    private const string Base = "api/boardgames";

    public async Task<List<BoardGame>> GetAllAsync(CancellationToken ct = default) =>
        await _http.GetFromJsonAsync<List<BoardGame>>(Base, ct) ?? new();

    public async Task<BoardGame?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await _http.GetFromJsonAsync<BoardGame>($"{Base}/{id}", ct);

    public async Task<BoardGame?> CreateAsync(BoardGame game, CancellationToken ct = default)
    {
        var res = await _http.PostAsJsonAsync(Base, game, ct);
        if (!res.IsSuccessStatusCode) return null;
        return await res.Content.ReadFromJsonAsync<BoardGame>(cancellationToken: ct);
    }

    public async Task<bool> UpdateAsync(int id, BoardGame game, CancellationToken ct = default)
    {
        var res = await _http.PutAsJsonAsync($"{Base}/{id}", game, ct);
        return res.IsSuccessStatusCode;
    }

    public async Task UpsertAsync(IGame model, CancellationToken ct = default)
    {
        // route based on type
        var isCard = model is CardGame;
        var basePath = isCard ? "api/cardgames" : "api/boardgames";

        if (model.Id == 0)
        {
            var res = await http.PostAsJsonAsync(basePath, model, ct);
            res.EnsureSuccessStatusCode();
        }
        else
        {
            var res = await http.PutAsJsonAsync($"{basePath}/{model.Id}", model, ct);
            res.EnsureSuccessStatusCode();
        }
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var res = await _http.DeleteAsync($"{Base}/{id}", ct);
        return res.IsSuccessStatusCode;
    }
}
