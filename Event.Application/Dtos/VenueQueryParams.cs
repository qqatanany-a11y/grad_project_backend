using events.domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Event.Application.Dtos
{
    public class VenueQueryParams
    {
        public string? Search { get; set; }
        public string? City { get; set; }
        public VenueType? Type { get; set; }

        public int? MinCapacity { get; set; }
        public int? MaxCapacity { get; set; }

        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        // ⭐ Rating
        public double? MinRating { get; set; }

        // 📅 Availability
        public DateTime? Date { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }

        public string? SortBy { get; set; } = "name";
        public string? SortOrder { get; set; } = "asc";

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}