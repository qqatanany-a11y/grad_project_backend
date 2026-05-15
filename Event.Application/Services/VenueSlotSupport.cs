using Event.Application.Dtos;
using events.domain.Entities;

namespace Event.Application.Services
{
    internal static class VenueSlotSupport
    {
        public static List<VenueTimeSlotDto> MapSlots(
            IEnumerable<VenueTimeSlot>? timeSlots,
            bool activeOnly = false)
        {
            var source = timeSlots ?? Enumerable.Empty<VenueTimeSlot>();

            if (activeOnly)
            {
                source = source.Where(slot => slot.IsActive);
            }

            return source
                .OrderBy(slot => slot.StartTime)
                .ThenBy(slot => slot.EndTime)
                .Select(slot => new VenueTimeSlotDto
                {
                    Id = slot.Id,
                    StartTime = slot.StartTime,
                    EndTime = slot.EndTime,
                    Price = slot.Price,
                    IsActive = slot.IsActive
                })
                .ToList();
        }

        public static List<VenueTimeSlotUpsertDto> MapEditableSlots(IEnumerable<VenueTimeSlot>? timeSlots)
        {
            return (timeSlots ?? Enumerable.Empty<VenueTimeSlot>())
                .OrderBy(slot => slot.StartTime)
                .ThenBy(slot => slot.EndTime)
                .Select(slot => new VenueTimeSlotUpsertDto
                {
                    Id = slot.Id,
                    StartTime = slot.StartTime,
                    EndTime = slot.EndTime,
                    Price = slot.Price,
                    IsActive = slot.IsActive
                })
                .ToList();
        }

        public static void ValidateVenuePricing(
            PricingType pricingType,
            decimal? pricePerHour,
            List<VenueTimeSlotUpsertDto>? timeSlots)
        {
            if (timeSlots != null)
            {
                ValidateSlots(timeSlots);

                if (timeSlots.Count > 0)
                {
                    if (pricePerHour.HasValue && pricePerHour.Value < 0)
                    {
                        throw new Exception("Venue price must be greater than or equal to 0.");
                    }

                    return;
                }
            }

            if (pricePerHour.HasValue && pricePerHour.Value < 0)
            {
                throw new Exception("Venue price must be greater than or equal to 0.");
            }
        }

        public static void ValidateSlots(List<VenueTimeSlotUpsertDto> timeSlots)
        {
            var seenSlots = new HashSet<string>(StringComparer.Ordinal);

            foreach (var slot in timeSlots)
            {
                if (!slot.StartTime.HasValue)
                {
                    throw new Exception("Start time is required for every venue slot.");
                }

                if (!slot.EndTime.HasValue)
                {
                    throw new Exception("End time is required for every venue slot.");
                }

                if (slot.Price < 0)
                {
                    throw new Exception("Slot price must be greater than or equal to 0.");
                }

                if (slot.StartTime.Value >= slot.EndTime.Value)
                {
                    throw new Exception("Slot start time must be before the end time.");
                }

                var slotKey = $"{slot.StartTime.Value:c}|{slot.EndTime.Value:c}";

                if (!seenSlots.Add(slotKey))
                {
                    throw new Exception("Duplicate venue time slots are not allowed.");
                }
            }
        }

        public static void SyncSlots(Venue venue, List<VenueTimeSlotUpsertDto> timeSlots)
        {
            var existingById = venue.TimeSlots.ToDictionary(slot => slot.Id);
            var requestedIds = new HashSet<int>();

            foreach (var slotDto in timeSlots)
            {
                if (!slotDto.StartTime.HasValue || !slotDto.EndTime.HasValue)
                {
                    continue;
                }

                if (slotDto.Id.HasValue && existingById.TryGetValue(slotDto.Id.Value, out var existingSlot))
                {
                    existingSlot.Update(
                        slotDto.StartTime.Value,
                        slotDto.EndTime.Value,
                        slotDto.Price,
                        slotDto.IsActive);

                    requestedIds.Add(existingSlot.Id);
                    continue;
                }

                venue.TimeSlots.Add(new VenueTimeSlot(
                    slotDto.StartTime.Value,
                    slotDto.EndTime.Value,
                    slotDto.Price,
                    slotDto.IsActive));
            }

            var slotsToRemove = venue.TimeSlots
                .Where(slot => slot.Id > 0 && !requestedIds.Contains(slot.Id))
                .ToList();

            foreach (var slot in slotsToRemove)
            {
                venue.TimeSlots.Remove(slot);
            }
        }
    }
}
