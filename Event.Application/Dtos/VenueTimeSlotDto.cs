using System;
using System.Collections.Generic;
using System.Text;

namespace Event.Application.Dtos
{
    public class VenueTimeSlotDto
    {
        public DayOfWeek Day { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public int Id { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
    }

    public class VenueTimeSlotUpsertDto
    {
        public int? Id { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
