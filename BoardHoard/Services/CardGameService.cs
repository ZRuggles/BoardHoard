using System.Net.Http.Json;
using Boardgames.Shared;

namespace BoardHoard.Services;

public class CardGameService(HttpClient http) : ICardGameService
{
    private readonly HttpClient _http = http;
    private const string Base = "api/cardgames";

    public async Task<List<CardGame>> GetAllAsync(CancellationToken ct = default) =>
        await _http.GetFromJsonAsync<List<CardGame>>(Base, ct) ?? new();

    public async Task<CardGame?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await _http.GetFromJsonAsync<CardGame>($"{Base}/{id}", ct);

    public async Task<CardGame?> CreateAsync(CardGame game, CancellationToken ct = default)
    {
        var res = await _http.PostAsJsonAsync(Base, game, ct);
        if (!res.IsSuccessStatusCode) return null;
        return await res.Content.ReadFromJsonAsync<CardGame>(cancellationToken: ct);
    }

    public async Task<bool> UpdateAsync(int id, CardGame game, CancellationToken ct = default)
    {
        var res = await _http.PutAsJsonAsync($"{Base}/{id}", game, ct);
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var res = await _http.DeleteAsync($"{Base}/{id}", ct);
        return res.IsSuccessStatusCode;
    }
}
