using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BoardHoardApi.Data;
using Boardgames.Shared;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// -------------------- Database: SQL Server (local) OR SQLite (cloud) --------------------
var useSqlite = Environment.GetEnvironmentVariable("USE_SQLITE") == "1";

if (useSqlite)
{
    // SQLite file inside the container (works on Render free tier).
    // For Docker/Render we bind to /app; create /app/data and put the db there.
    var sqlitePath = "/app/data/boardhoard.db";
    Directory.CreateDirectory(Path.GetDirectoryName(sqlitePath)!);

    builder.Services.AddDbContext<GamesDbContext>(o =>
        o.UseSqlite($"Data Source={sqlitePath}"));
}
else
{
    // Local dev: keep using SQL Server from appsettings
    builder.Services.AddDbContext<GamesDbContext>(o =>
        o.UseSqlServer(builder.Configuration.GetConnectionString("GamesDb")));
}

// -------------------- Core services --------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// -------------------- CORS (env-driven) --------------------
const string AllowFrontend = "AllowFrontend";

// Accept an array of origins via env vars:
//   ALLOWED_ORIGINS__0 = https://localhost:7003
//   ALLOWED_ORIGINS__1 = https://your-frontend.onrender.com
var allowedOrigins = new List<string>();

// Also support appsettings: "AllowedOrigins": ["..."]
builder.Configuration.GetSection("AllowedOrigins").Bind(allowedOrigins);

// Merge env array
for (int i = 0; ; i++)
{
    var v = Environment.GetEnvironmentVariable($"ALLOWED_ORIGINS__{i}");
    if (string.IsNullOrWhiteSpace(v)) break;
    allowedOrigins.Add(v);
}

// Reasonable default for local dev
if (allowedOrigins.Count == 0)
{
    allowedOrigins.Add("https://localhost:7003");
}

builder.Services.AddCors(options =>
{
    options.AddPolicy(AllowFrontend, p =>
        p.WithOrigins(allowedOrigins.ToArray())
         .AllowAnyHeader()
         .AllowAnyMethod()
         .AllowCredentials());
});

// -------------------- Auth (cookies if you add Identity later) --------------------
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
})
.AddIdentityCookies();

builder.Services.AddAuthorization();

var app = builder.Build();

// -------------------- Forwarded headers (behind Render/nginx proxies) --------------------
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// -------------------- Auto-migrate on startup if requested --------------------
if (Environment.GetEnvironmentVariable("AUTO_MIGRATE") == "1")
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<GamesDbContext>();
    db.Database.Migrate();
}

// -------------------- Aggregate backfill (safe skip if Reviews table missing) --------------------
try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<GamesDbContext>();

    var boardAgg = await db.Reviews
        .Where(r => r.Kind == GameKind.Board)
        .GroupBy(r => r.GameId)
        .Select(g => new { GameId = g.Key, Cnt = g.Count(), Avg = g.Average(x => x.Rating) })
        .ToListAsync();

    var cardAgg = await db.Reviews
        .Where(r => r.Kind == GameKind.Card)
        .GroupBy(r => r.GameId)
        .Select(g => new { GameId = g.Key, Cnt = g.Count(), Avg = g.Average(x => x.Rating) })
        .ToListAsync();

    if (boardAgg.Count > 0)
    {
        var ids = boardAgg.Select(a => a.GameId).ToList();
        var boards = await db.BoardGames.Where(g => ids.Contains(g.Id)).ToListAsync();
        foreach (var b in boards)
        {
            var ag = boardAgg.First(a => a.GameId == b.Id);
            b.RatingsCount = ag.Cnt;
            b.AvgRating = Math.Round((decimal)(ag.Avg * 2), MidpointRounding.AwayFromZero) / 2.0M;
        }
    }

    if (cardAgg.Count > 0)
    {
        var ids = cardAgg.Select(a => a.GameId).ToList();
        var cards = await db.CardGames.Where(g => ids.Contains(g.Id)).ToListAsync();
        foreach (var c in cards)
        {
            var ag = cardAgg.First(a => a.GameId == c.Id);
            c.RatingsCount = ag.Cnt;
            c.AvgRating = Math.Round((decimal)(ag.Avg * 2), MidpointRounding.AwayFromZero) / 2.0M;
        }
    }

    await db.SaveChangesAsync();
}
catch (SqlException ex) when (ex.Number == 208) // SQL Server: Invalid object name 'Reviews'
{
    // Table not found (first run). Ignore; will compute next start.
}
catch (DbUpdateException)
{
    // Likely underlying table missing on SQLite; skip quietly for first run
}
catch (Exception)
{
    // Non-fatal; skip aggregate if anything unexpected happens
}

// -------------------- Swagger in Development --------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// NOTE: behind Render we listen on HTTP in the container; https is terminated at the edge.
// Https redirection is still fine thanks to forwarded headers above.
app.UseHttpsRedirection();

app.UseCors(AllowFrontend);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Simple health endpoint for quick checks or uptime monitoring
app.MapGet("/api/ping", () => Results.Ok(new { ok = true, nowUtc = DateTimeOffset.UtcNow }));

app.Run();
