using Boardgames.Shared;
using BoardHoardAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace BoardHoardApi.Data;

public class GamesDbContext(DbContextOptions<GamesDbContext> options) : DbContext(options)
{
    public DbSet<BoardGame> BoardGames => Set<BoardGame>();
    public DbSet<CardGame> CardGames => Set<CardGame>();
    public DbSet<ImageBlob> Images => Set<ImageBlob>();
    public DbSet<Review> Reviews => Set<Review>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        b.Entity<BoardGame>().HasIndex(g => g.Title);
        b.Entity<CardGame>().HasIndex(g => g.Title);
        b.Entity<BoardGame>().Property(g => g.RarityScore).HasPrecision(6, 2);
        b.Entity<BoardGame>().Property(g => g.SentimentalScore).HasPrecision(6, 2);
        b.Entity<CardGame>().Property(g => g.RarityScore).HasPrecision(6, 2);
        b.Entity<CardGame>().Property(g => g.SentimentalScore).HasPrecision(6, 2);
        // Aggregated rating fields
        b.Entity<BoardGame>().Property(g => g.AvgRating).HasPrecision(3, 2).HasDefaultValue(0);
        b.Entity<BoardGame>().Property(g => g.RatingsCount).HasDefaultValue(0);
        b.Entity<CardGame>().Property(g => g.AvgRating).HasPrecision(3, 2).HasDefaultValue(0);
        b.Entity<CardGame>().Property(g => g.RatingsCount).HasDefaultValue(0);
        var jsonOpts = new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase };
        var tagsConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<List<string>, string>(
        v => System.Text.Json.JsonSerializer.Serialize(v ?? new List<string>(), jsonOpts),
        v => string.IsNullOrWhiteSpace(v) ? new List<string>() :
        (System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, jsonOpts) ?? new List<string>()));
        
        b.Entity<BoardGame>().Property(nameof(BoardGame.Tags))
                .HasConversion(tagsConverter)
                .HasColumnType("nvarchar(max)");
        
        b.Entity<CardGame>().Property(nameof(CardGame.Tags))
                .HasConversion(tagsConverter)
                .HasColumnType("nvarchar(max)");

        b.Entity<ImageBlob>(g =>
        {
            g.HasKey(e => e.Id);
            g.Property(e => e.FileName).HasMaxLength(255);
            g.Property(e => e.ContentType).HasMaxLength(100);
        });
        b.Entity<Review>().HasIndex(r => new { r.UserId, r.Kind, r.GameId }).IsUnique();
    }
}
