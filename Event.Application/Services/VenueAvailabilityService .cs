using Event.Application.Dtos;
using Event.Application.IServices;
using events.domain.Entities;
using events.domain.Repos;

namespace Event.Application.Services
{
    public class VenueAvailabilityService : IVenueAvailabilityService
    {
        private readonly IVenueRepo _venueRepo;
        private readonly IVenueAvailabilityRepo _venueAvailabilityRepo;

        public VenueAvailabilityService(
            IVenueRepo venueRepo,
            IVenueAvailabilityRepo venueAvailabilityRepo)
        {
            _venueRepo = venueRepo;
            _venueAvailabilityRepo = venueAvailabilityRepo;
        }

        public async Task<VenueAvailabilityItemDto> AddAsync(int ownerId, CreateVenueAvailabilityDto dto)
        {
            var venue = await _venueRepo.GetByIdAsync(dto.VenueId);

            if (venue == null)
                throw new Exception("Venue not found");

            if (venue.Company.UserId != ownerId)
                throw new Exception("Access denied. You can only manage your own venues.");

            if (venue.PricingType != PricingType.FixedSlots)
                throw new Exception("Availability slots can only be added to fixed-slots venues.");

            if (dto.EndTime <= dto.StartTime)
                throw new Exception("End time must be after start time.");

            if (dto.Price <= 0)
                throw new Exception("Price must be greater than zero.");

            var existingSlot = await _venueAvailabilityRepo.GetSlotAsync(
                dto.VenueId,
                dto.Date,
                dto.StartTime,
                dto.EndTime);

            if (existingSlot != null)
                throw new Exception("This slot already exists.");

            var availability = new VenueAvailability(
                dto.VenueId,
                dto.Date,
                dto.StartTime,
                dto.EndTime,
                dto.Price
            );

            await _venueAvailabilityRepo.AddAsync(availability);
            await _venueAvailabilityRepo.SaveChangesAsync();

            return new VenueAvailabilityItemDto
            {
                Id = availability.Id,
                VenueId = availability.VenueId,
                Date = availability.Date,
                StartTime = availability.StartTime,
                EndTime = availability.EndTime,
                Price = availability.Price,
                IsBooked = availability.IsBooked
            };
        }

        public async Task<List<VenueAvailabilityItemDto>> GetByVenueIdAsync(int ownerId, int venueId)
        {
            var venue = await _venueRepo.GetByIdAsync(venueId);

            if (venue == null)
                throw new Exception("Venue not found");

            if (venue.Company.UserId != ownerId)
                throw new Exception("Access denied. You can only manage your own venues.");

            var slots = await _venueAvailabilityRepo.GetByVenueIdAsync(venueId);

            return slots.Select(x => new VenueAvailabilityItemDto
            {
                Id = x.Id,
                VenueId = x.VenueId,
                Date = x.Date,
                StartTime = x.StartTime,
                EndTime = x.EndTime,
                Price = x.Price,
                IsBooked = x.IsBooked
            }).ToList();
        }

        public async Task<List<VenueAvailabilityItemDto>> GetAvailableSlotsAsync(int venueId, DateOnly date)
        {
            var slots = await _venueAvailabilityRepo.GetAvailableByVenueAndDateAsync(venueId, date);

            return slots.Select(x => new VenueAvailabilityItemDto
            {
                Id = x.Id,
                VenueId = x.VenueId,
                Date = x.Date,
                StartTime = x.StartTime,
                EndTime = x.EndTime,
                Price = x.Price,
                IsBooked = x.IsBooked
            }).ToList();
        }
    }
}