namespace BoardHoard.Data;

public class ProfileUpdateDto
{
    public string? DisplayName { get; set; }
    public string? PhotoUrl { get; set; }
    public int? Age { get; set; }
    public string? FavoriteGame { get; set; }
    public string? FavoriteMechanic { get; set; }
    public string? Bio { get; set; }
    public string? TopGamesJson { get; set; }
}

public class ProfileViewDto
{
    public string UserId { get; set; } = "";
    public string? DisplayName { get; set; }
    public string? PhotoUrl { get; set; }
    public int? Age { get; set; }
    public string? FavoriteGame { get; set; }
    public string? FavoriteMechanic { get; set; }
    public string? Bio { get; set; }
    public string? TopGamesJson { get; set; }
}
