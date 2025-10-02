using BoardHoardApi.Data;
using Boardgames.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Http;

[ApiController]
[Route("api/[controller]")]
public class ReviewsController(GamesDbContext db) : ControllerBase
{
    private string? UserId => User.FindFirstValue(ClaimTypes.NameIdentifier);
    private string? ResolveUserId()
    {
        var uid = UserId;
        if (!string.IsNullOrEmpty(uid)) return uid;
        if (Request.Headers.TryGetValue("X-UserId", out var hdr) && !string.IsNullOrWhiteSpace(hdr))
            return hdr.ToString();
        return null;
    }

    [AllowAnonymous]
    [HttpGet("{kind}/{gameId:int}/recent")]
    public async Task<ActionResult<IEnumerable<Review>>> GetRecent(GameKind kind, int gameId, [FromQuery] int take = 3)
    {
        try
        {
            var items = await db.Reviews
                .Where(r => r.Kind == kind && r.GameId == gameId)
                .OrderByDescending(r => r.UpdatedAt ?? r.CreatedAt)
                .Take(Math.Clamp(take, 1, 50))
                .ToListAsync();
            return Ok(items);
        }
        catch (SqlException ex) when (ex.Number == 208) // Invalid object name 'Reviews'
        {
            // Table not created yet; surface empty set instead of 500
            return Ok(Array.Empty<Review>());
        }
    }

    [AllowAnonymous]
    [HttpGet("{kind}/{gameId:int}/all")]
    public async Task<ActionResult<IEnumerable<Review>>> GetAll(GameKind kind, int gameId)
    {
        try
        {
            var items = await db.Reviews
                .Where(r => r.Kind == kind && r.GameId == gameId)
                .OrderByDescending(r => r.UpdatedAt ?? r.CreatedAt)
                .ToListAsync();
            return Ok(items);
        }
        catch (SqlException ex) when (ex.Number == 208)
        {
            return Ok(Array.Empty<Review>());
        }
    }

    [AllowAnonymous]
    [HttpGet("{kind}/{gameId:int}/mine")]
    public async Task<ActionResult<Review?>> GetMine(GameKind kind, int gameId)
    {
        var uid = ResolveUserId();
        if (uid is null) return Unauthorized();
        var mine = await db.Reviews.FirstOrDefaultAsync(r => r.UserId == uid && r.Kind == kind && r.GameId == gameId);
        return Ok(mine);
    }

    public record UpsertReviewDto(double Rating, string? Title, string? Body);

    [AllowAnonymous]
    [HttpPost("{kind}/{gameId:int}")]
    public async Task<ActionResult<Review>> Upsert(GameKind kind, int gameId, [FromBody] UpsertReviewDto dto)
    {
        var uid = ResolveUserId();
        if (uid is null) return Unauthorized();

        // Clamp 0–5 and snap to nearest 0.5
        var clamped = Math.Clamp(dto.Rating, 0, 5);
        var rating = Math.Round(clamped * 2, MidpointRounding.AwayFromZero) / 2.0;

        try
        {
            var existing = await db.Reviews.FirstOrDefaultAsync(r => r.UserId == uid && r.Kind == kind && r.GameId == gameId);
            if (existing is null)
            {
                var r = new Review
                {
                    UserId = uid,
                    Kind = kind,
                    GameId = gameId,
                    Rating = rating,
                    Title = dto.Title,
                    Body = dto.Body
                };
                db.Reviews.Add(r);
                await UpdateAggregatesOnInsert(kind, gameId, rating);
                await db.SaveChangesAsync();
                return CreatedAtAction(nameof(GetMine), new { kind, gameId }, r);
            }
            var old = existing.Rating;
            existing.Rating = rating;
            existing.Title = dto.Title;
            existing.Body = dto.Body;
            existing.UpdatedAt = DateTimeOffset.UtcNow;
            await UpdateAggregatesOnUpdate(kind, gameId, old, rating);
            await db.SaveChangesAsync();
            return Ok(existing);
        }
        catch (SqlException ex) when (ex.Number == 208)
        {
            return Problem(title: "Reviews storage is not initialized", detail: "The Reviews table is missing. Apply database migrations and retry.", statusCode: StatusCodes.Status503ServiceUnavailable);
        }
    }

    [AllowAnonymous]
    [HttpDelete("{kind}/{gameId:int}")]
    public async Task<IActionResult> DeleteMine(GameKind kind, int gameId)
    {
        var uid = ResolveUserId();
        if (uid is null) return Unauthorized();
        try
        {
            var mine = await db.Reviews.FirstOrDefaultAsync(r => r.UserId == uid && r.Kind == kind && r.GameId == gameId);
            if (mine is null) return NotFound();
            db.Reviews.Remove(mine);
            await UpdateAggregatesOnDelete(kind, gameId, mine.Rating);
            await db.SaveChangesAsync();
            return NoContent();
        }
        catch (SqlException ex) when (ex.Number == 208)
        {
            return Problem(title: "Reviews storage is not initialized", detail: "The Reviews table is missing. Apply database migrations and retry.", statusCode: StatusCodes.Status503ServiceUnavailable);
        }
    }

    [AllowAnonymous]
    [HttpGet("avg/{kind}/{gameId:int}")]
    public async Task<ActionResult<double>> GetAverage(GameKind kind, int gameId)
    {
        try
        {
            var avg = await db.Reviews
                .Where(r => r.Kind == kind && r.GameId == gameId)
                .Select(r => (double?)r.Rating)
                .AverageAsync() ?? 0d;
            return Ok(Math.Round(avg, 2));
        }
        catch (SqlException ex) when (ex.Number == 208)
        {
            return Ok(0d);
        }
    }

    [AllowAnonymous]
    [HttpGet("count/{kind}/{gameId:int}")]
    public async Task<ActionResult<int>> GetCount(GameKind kind, int gameId)
    {
        try
        {
            return await db.Reviews.CountAsync(r => r.Kind == kind && r.GameId == gameId);
        }
        catch (SqlException ex) when (ex.Number == 208)
        {
            return Ok(0);
        }
    }

    // Public reviews by user
    [AllowAnonymous]
    [HttpGet("user/{userId}/{kind}/top")]
    public async Task<ActionResult<IEnumerable<Review>>> GetTopByUser(string userId, GameKind kind, [FromQuery] int take = 3)
    {
        try
        {
            var list = await db.Reviews.Where(r => r.UserId == userId && r.Kind == kind)
                .OrderByDescending(r => r.Rating)
                .ThenByDescending(r => r.UpdatedAt ?? r.CreatedAt)
                .Take(Math.Clamp(take, 1, 50))
                .ToListAsync();
            return Ok(list);
        }
        catch (SqlException ex) when (ex.Number == 208)
        {
            return Ok(Array.Empty<Review>());
        }
    }

    [AllowAnonymous]
    [HttpGet("user/{userId}/{kind}/all")]
    public async Task<ActionResult<IEnumerable<Review>>> GetAllByUser(string userId, GameKind kind)
    {
        try
        {
            var list = await db.Reviews.Where(r => r.UserId == userId && r.Kind == kind)
                .OrderByDescending(r => r.UpdatedAt ?? r.CreatedAt)
                .ToListAsync();
            return Ok(list);
        }
        catch (SqlException ex) when (ex.Number == 208)
        {
            return Ok(Array.Empty<Review>());
        }
    }

    // Top-rated games by user
    [AllowAnonymous]
    [HttpGet("user/{userId}/{kind}/top-games")]
    public async Task<ActionResult<IEnumerable<(int gameId, double avgRating, int count)>>> GetTopGamesByUser(string userId, GameKind kind, [FromQuery] int take = 3)
    {
        try
        {
            var list = await db.Reviews.Where(r => r.UserId == userId && r.Kind == kind)
                .GroupBy(r => r.GameId)
                .Select(g => new { gameId = g.Key, avgRating = g.Average(r => r.Rating), count = g.Count() })
                .OrderByDescending(x => x.avgRating)
                .ThenByDescending(x => x.count)
                .Take(Math.Clamp(take, 1, 50))
                .ToListAsync();
            return Ok(list.Select(x => (x.gameId, x.avgRating, x.count)));
        }
        catch (SqlException ex) when (ex.Number == 208)
        {
            return Ok(Array.Empty<(int, double, int)>());
        }
    }

    [AllowAnonymous]
    [HttpGet("user/{userId}/{kind}/all-games")]
    public async Task<ActionResult<IEnumerable<(int gameId, double avgRating, int count)>>> GetAllGamesByUser(string userId, GameKind kind)
    {
        try
        {
            var list = await db.Reviews.Where(r => r.UserId == userId && r.Kind == kind)
                .GroupBy(r => r.GameId)
                .Select(g => new { gameId = g.Key, avgRating = g.Average(r => r.Rating), count = g.Count() })
                .OrderByDescending(x => x.avgRating)
                .ThenByDescending(x => x.count)
                .ToListAsync();
            return Ok(list.Select(x => (x.gameId, x.avgRating, x.count)));
        }
        catch (SqlException ex) when (ex.Number == 208)
        {
            return Ok(Array.Empty<(int, double, int)>());
        }
    }

    private async Task UpdateAggregatesOnInsert(GameKind kind, int gameId, double newRating)
    {
        if (kind == GameKind.Board)
        {
            var g = await db.BoardGames.FirstOrDefaultAsync(x => x.Id == gameId);
            if (g is null) return;
            var count = g.RatingsCount;
            var total = g.AvgRating * count; // decimal
            var newCount = count + 1;
            g.RatingsCount = newCount;
            g.AvgRating = newCount == 0
                ? 0m
                : Math.Round((total + (decimal)newRating) / newCount, 2, MidpointRounding.AwayFromZero);
        }
        else
        {
            var g = await db.CardGames.FirstOrDefaultAsync(x => x.Id == gameId);
            if (g is null) return;
            var count = g.RatingsCount;
            var total = g.AvgRating * count; // decimal
            var newCount = count + 1;
            g.RatingsCount = newCount;
            g.AvgRating = newCount == 0
                ? 0m
                : Math.Round((total + (decimal)newRating) / newCount, 2, MidpointRounding.AwayFromZero);
        }
    }

    private async Task UpdateAggregatesOnUpdate(GameKind kind, int gameId, double oldRating, double newRating)
    {
        if (kind == GameKind.Board)
        {
            var g = await db.BoardGames.FirstOrDefaultAsync(x => x.Id == gameId);
            if (g is null || g.RatingsCount == 0) return;
            var total = g.AvgRating * g.RatingsCount; // decimal
            g.AvgRating = Math.Round((total - (decimal)oldRating + (decimal)newRating) / g.RatingsCount, 2, MidpointRounding.AwayFromZero);
        }
        else
        {
            var g = await db.CardGames.FirstOrDefaultAsync(x => x.Id == gameId);
            if (g is null || g.RatingsCount == 0) return;
            var total = g.AvgRating * g.RatingsCount; // decimal
            g.AvgRating = Math.Round((total - (decimal)oldRating + (decimal)newRating) / g.RatingsCount, 2, MidpointRounding.AwayFromZero);
        }
    }

    private async Task UpdateAggregatesOnDelete(GameKind kind, int gameId, double oldRating)
    {
        if (kind == GameKind.Board)
        {
            var g = await db.BoardGames.FirstOrDefaultAsync(x => x.Id == gameId);
            if (g is null) return;
            var count = g.RatingsCount;
            if (count <= 1)
            {
                g.RatingsCount = 0;
                g.AvgRating = 0m;
            }
            else
            {
                var total = g.AvgRating * count; // decimal
                var newCount = count - 1;
                g.RatingsCount = newCount;
                g.AvgRating = Math.Round((total - (decimal)oldRating) / newCount, 2, MidpointRounding.AwayFromZero);
            }
        }
        else
        {
            var g = await db.CardGames.FirstOrDefaultAsync(x => x.Id == gameId);
            if (g is null) return;
            var count = g.RatingsCount;
            if (count <= 1)
            {
                g.RatingsCount = 0;
                g.AvgRating = 0m;
            }
            else
            {
                var total = g.AvgRating * count; // decimal
                var newCount = count - 1;
                g.RatingsCount = newCount;
                g.AvgRating = Math.Round((total - (decimal)oldRating) / newCount, 2, MidpointRounding.AwayFromZero);
            }
        }
    }
}
