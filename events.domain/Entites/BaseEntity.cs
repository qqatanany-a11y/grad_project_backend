using System;
using System.Collections.Generic;
using System.Text;

namespace events.domain.Entites
{
    public class BaseEntity
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
