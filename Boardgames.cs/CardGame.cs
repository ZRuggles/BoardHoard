using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boardgames.Shared
{
    public class CardGame : IGame
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public int Year { get; set; }
        public int MinPlayers { get; set; }
        public int MaxPlayers { get; set; }
        public string ImageUrl { get; set; } = "";
        public int? ImageId { get; set; }
        public string? Publisher { get; set; }
        public string? Description { get; set; }
        public int? ageRating { get; set; }
        public List<string> Tags { get; set; } = new();
        public decimal AvgRating { get; set; } = 0m;
        public int RatingsCount { get; set; } = 0;
        public decimal RarityScore { get; set; } = 0m;
        public decimal SentimentalScore { get; set; } = 0m;
        public long cardCount { get; set; } = 0;
        public string? CreatedByUserId { get; set; }
    }
}
