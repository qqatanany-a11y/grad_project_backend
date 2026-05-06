using events.domain.Entites;
using System;
using System.Collections.Generic;
using System.Text;

namespace events.domain.Entities
{
    public class VenueTimeSlot : BaseEntity
    {
        public int VenueId { get; private set; }

        public DayOfWeek Day { get; private set; }

        public TimeSpan StartTime { get; private set; }
        public TimeSpan EndTime { get; private set; }

        public Venue Venue { get; private set; } = null!;

        private VenueTimeSlot() { }

        public VenueTimeSlot(DayOfWeek day, TimeSpan start, TimeSpan end)
        {
            Day = day;
            StartTime = start;
            EndTime = end;
        }
    }
}
