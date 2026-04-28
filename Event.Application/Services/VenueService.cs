using Event.Application.Dtos;
using Event.Application.IServices;
using events.domain.Entities;
using events.domain.Repos;

namespace Event.Application.Services
{
    public class VenueService : IVenueService
    {
        private readonly IVenueRepo _venueRepo;
        private readonly IUserRepo _userRepo;

        private static VenueDto MapVenue(Venue venue, bool activeSlotsOnly = false)
        {
            return new VenueDto
            {
                Id = venue.Id,
                Name = venue.Name,
                Description = venue.Description,
                City = venue.City,
                Address = venue.Address,
                Capacity = venue.Capacity,
                IsActive = venue.IsActive,
                CompanyName = venue.Company?.Name,
                CompanyId = venue.CompanyId,
                Category = venue.Category,
                PricingType = venue.PricingType,
                PricePerHour = venue.PricePerHour,
                TimeSlots = VenueSlotSupport.MapSlots(venue.TimeSlots, activeSlotsOnly)
            };
        }

        public VenueService(IVenueRepo venueRepo, IUserRepo userRepo)
        {
            _venueRepo = venueRepo;
            _userRepo = userRepo;
        }

        public async Task<List<VenueDto>> GetByCompanyIdAsync(int companyId)
        {
            var venues = await _venueRepo.GetByCompanyIdAsync(companyId);

            return venues.Select(venue => MapVenue(venue)).ToList();
        }

        public async Task<List<VenueDto>> GetByOwnerIdAsync(int ownerId)
        {
            var user = await _userRepo.GetUserByIdAsync(ownerId);

            if (user == null)
                throw new Exception("user not found");

            if (user.Role.Name != "Owner")
                throw new Exception("You do not have permission to access this resource.");

            var venues = await _venueRepo.GetByOwnerId(ownerId);

            return venues.Select(venue => MapVenue(venue)).ToList();
        }

        public async Task<VenueDto> AddAsync(int companyId, AddVenueDto dto)
        {
            if (dto.ImageUrls == null || dto.ImageUrls.Count < 10)
                throw new Exception("You must upload at least 10 images.");

            VenueSlotSupport.ValidateVenuePricing(dto.PricingType, dto.PricePerHour, dto.TimeSlots);

            if (dto.PricingType != PricingType.Hourly && (dto.TimeSlots == null || dto.TimeSlots.Count == 0))
            {
                dto.PricePerHour = null;
            }

            var venue = new Venue(
                dto.Name,
                dto.Description,
                dto.City,
                dto.Address,
                dto.Capacity,
                companyId,
                dto.Category,
                dto.PricingType,
                dto.PricePerHour
            );

            venue.AddImages(dto.ImageUrls);

            if (dto.TimeSlots != null)
            {
                VenueSlotSupport.SyncSlots(venue, dto.TimeSlots);
            }

            await _venueRepo.AddAsync(venue);

            return MapVenue(venue);
        }

        public async Task<VenueDto> UpdateAsync(int venueId, UpdateVenueDto dto)
        {
            var venue = await _venueRepo.GetByIdAsync(venueId);

            if (venue == null)
                throw new Exception("القاعة غير موجودة");

            VenueSlotSupport.ValidateVenuePricing(dto.PricingType, dto.PricePerHour, dto.TimeSlots);

            if (dto.PricingType != PricingType.Hourly && (dto.TimeSlots == null || dto.TimeSlots.Count == 0))
            {
                dto.PricePerHour = null;
            }

            venue.Update(
                 dto.Name,
                 dto.Description,
                 dto.City,
                 dto.Address,
                 dto.Capacity,
                 dto.IsActive,
                 dto.Category,
                 dto.PricingType,
                 dto.PricePerHour
                );

            if (dto.TimeSlots != null)
            {
                VenueSlotSupport.SyncSlots(venue, dto.TimeSlots);
            }

            await _venueRepo.UpdateAsync(venue);

            return MapVenue(venue);
        }

        public async Task<VenueDto> GetByIdAsync(int venueId)
        {
            var venue = await _venueRepo.GetByIdAsync(venueId);

            if (venue == null)
                throw new Exception("القاعة غير موجودة");
            return MapVenue(venue);
        }

        public async Task DeleteAsync(int venueId)
        {
            var venue = await _venueRepo.GetByIdAsync(venueId);

            if (venue == null)
                throw new Exception("القاعة غير موجودة");

            await _venueRepo.DeleteAsync(venue);
        }

        public async Task<List<VenueDto>> GetAllAsync()
        {
            var venues = await _venueRepo.GetAllAsync();

            return venues.Select(venue => MapVenue(venue)).ToList();
        }

        public async Task<List<VenueDto>> GetVenuesForGuestAsync()
        {
            var venues = await _venueRepo.GetAllActiveAsync();

            return venues.Select(venue => MapVenue(venue, true)).ToList();
        }
    }
}
