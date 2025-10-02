using BoardHoard.Data;

public class UserProfile
{
    public Guid Id { get; set; }

    // link to Identity user
    public string UserId { get; set; } = "";
    public ApplicationUser? User { get; set; }
    public string DisplayName { get; set; } = "";
    public string? PhotoUrl { get; set; }
    public int? Age { get; set; }
    public string? FavoriteGame { get; set; }
    public string? FavoriteMechanic { get; set; }
    public string? Bio { get; set; }

    // JSON array of up to 3 items: [{ "kind": "board"|"card", "id": 123, "title": "..." }]
    public string? TopGamesJson { get; set; }
}

public class WallMessage
{
    public int Id { get; set; }

    public string OwnerUserId { get; set; } = "";

    public string AuthorUserId { get; set; } = "";

    public string Body { get; set; } = "";
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}
