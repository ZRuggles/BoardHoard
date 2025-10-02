namespace Boardgames.Shared;

public enum GameKind { Board, Card }

public class Review
{
    public int Id { get; set; }
    public GameKind Kind { get; set; }   // Board or Card
    public int GameId { get; set; }      // Id of BoardGame or CardGame
    public string UserId { get; set; } = default!;
    public double Rating { get; set; }      // 0–5, supports .5 steps
    public string? Title { get; set; }
    public string? Body { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
}
