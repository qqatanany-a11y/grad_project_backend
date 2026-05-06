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
    }
}
