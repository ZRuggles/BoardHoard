using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boardgames.Shared
{
    public interface IGame
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int Year { get; set; }
        public int? ageRating { get; set; }
        public int MinPlayers { get; set; }
        public int MaxPlayers { get; set; }
        int? ImageId { get; set; }

        public string ImageUrl { get; set; }
        public string? Publisher { get; set; }
        public string? Description { get; set; }
        // Aggregated rating fields stored on the game row
        List<string> Tags { get; set; }
        decimal AvgRating { get; set; }
        int RatingsCount { get; set; }
        decimal RarityScore { get; set; }
        decimal SentimentalScore { get; set; }
        string? CreatedByUserId { get; set; }
    }
}
