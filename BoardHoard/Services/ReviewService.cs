using Boardgames.Shared;
using static System.Net.WebRequestMethods;
using System.Net;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;
using System.Security.Claims;

namespace BoardHoard.Services
{
    public class ReviewService(HttpClient http, AuthenticationStateProvider auth) : IReviewsService
    {
        private async Task<string?> GetUserIdAsync()
        {
            var state = await auth.GetAuthenticationStateAsync();
            var uid = state.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return string.IsNullOrWhiteSpace(uid) ? null : uid;
        }

        private async Task<HttpRequestMessage> WithUserAsync(HttpMethod method, string url)
        {
            var uid = await GetUserIdAsync();
            var req = new HttpRequestMessage(method, url);
            if (!string.IsNullOrWhiteSpace(uid))
            {
                req.Headers.Add("X-UserId", uid);
            }
            return req;
        }

        public async Task<List<Review>> GetRecentAsync(GameKind kind, int gameId, int take = 3, CancellationToken ct = default)
        {
            var res = await http.GetAsync($"api/reviews/{kind}/{gameId}/recent?take={take}", ct);
            if (res.StatusCode == HttpStatusCode.Unauthorized) return new(); // allow anonymous UX
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<List<Review>>(cancellationToken: ct) ?? new();
        }

        public Task<List<Review>> GetAllAsync(GameKind kind, int gameId) =>
            http.GetFromJsonAsync<List<Review>>($"api/reviews/{kind}/{gameId}/all")!;

        public async Task<Review?> GetMineAsync(GameKind kind, int gameId, CancellationToken ct = default)
        {
            var req = await WithUserAsync(HttpMethod.Get, $"api/reviews/{kind}/{gameId}/mine");
            var res = await http.SendAsync(req, ct);
            if (res.StatusCode == HttpStatusCode.Unauthorized) return null; // not signed in to API
            if (res.StatusCode == HttpStatusCode.NotFound) return null;
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<Review?>(cancellationToken: ct);
        }

        public async Task<Review?> UpsertAsync(GameKind kind, int gameId, double rating, string? title, string? body)
        {
            var dto = new { Rating = rating, Title = title, Body = body };
            var req = await WithUserAsync(HttpMethod.Post, $"api/reviews/{kind}/{gameId}");
            req.Content = JsonContent.Create(dto);
            var resp = await http.SendAsync(req);
            if (!resp.IsSuccessStatusCode) return null;
            return await resp.Content.ReadFromJsonAsync<Review>();
        }

        public async Task DeleteMineAsync(GameKind kind, int gameId)
        {
            var req = await WithUserAsync(HttpMethod.Delete, $"api/reviews/{kind}/{gameId}");
            var resp = await http.SendAsync(req);
            resp.EnsureSuccessStatusCode();
        }

        public async Task<double> GetAverageAsync(GameKind kind, int gameId, CancellationToken ct = default)
        {
            return await http.GetFromJsonAsync<double>($"api/reviews/avg/{kind}/{gameId}", ct);
        }

        public async Task<int> GetCountAsync(GameKind kind, int gameId, CancellationToken ct = default)
        {
            return await http.GetFromJsonAsync<int>($"api/reviews/count/{kind}/{gameId}", ct);
        }

        public Task<List<Review>> GetTopByUserAsync(string userId, GameKind kind, int take = 3, CancellationToken ct = default) =>
            http.GetFromJsonAsync<List<Review>>($"api/reviews/user/{userId}/{kind}/top?take={take}", ct)!;

        public Task<List<Review>> GetAllByUserAsync(string userId, GameKind kind, CancellationToken ct = default) =>
            http.GetFromJsonAsync<List<Review>>($"api/reviews/user/{userId}/{kind}/all", ct)!;
    }
}
