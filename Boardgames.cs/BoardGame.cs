namespace Boardgames.Shared
{
    public class BoardGame : IGame
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public int Year { get; set; }
        public int MinPlayers { get; set; }
        public int MaxPlayers { get; set; }
        public int? ImageId { get; set; }
        public string ImageUrl { get; set; } = ""; 
        public string? Publisher { get; set; }
        public string? Description { get; set; }
        public int? ageRating { get; set; }
        public List<int> Ratings { get; set; } = new();
        public List<string> Tags { get; set; } = new();
        public decimal AvgRating { get; set; } = 0m;
        public int RatingsCount { get; set; } = 0;
        public decimal RarityScore { get; set; } = 0m;
        public decimal SentimentalScore { get; set; } = 0m;
        public string? CreatedByUserId { get; set; }
    }
}
