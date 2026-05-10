using System;
using System.Collections.Generic;
using System.Text;

namespace Event.Application.Dtos
{
    public class AddReviewDto
    {
        public int BookingId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }
}
