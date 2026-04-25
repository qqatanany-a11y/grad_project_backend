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

        private static VenueDto MapVenue(Venue venue)
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
                PricePerHour = venue.PricePerHour
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

            return venues.Select(MapVenue).ToList();
        }

        public async Task<List<VenueDto>> GetByOwnerIdAsync(int ownerId)
        {
            var user = await _userRepo.GetUserByIdAsync(ownerId);

            if (user == null)
                throw new Exception("user not found");

            if (user.Role.Name != "Owner")
                throw new Exception("You do not have permission to access this resource.");

            var venues = await _venueRepo.GetByOwnerId(ownerId);

            return venues.Select(MapVenue).ToList();
        }

        public async Task<VenueDto> AddAsync(int companyId, AddVenueDto dto)
        {
            if (dto.ImageUrls == null || dto.ImageUrls.Count < 10)
                throw new Exception("You must upload at least 10 images.");

            if (dto.PricingType == PricingType.Hourly)
            {
                if (!dto.PricePerHour.HasValue || dto.PricePerHour <= 0)
                    throw new Exception("Price per hour must be greater than 0 for hourly venues.");
            }
            else
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

            await _venueRepo.AddAsync(venue);

            return MapVenue(venue);
        }

        public async Task<VenueDto> UpdateAsync(int venueId, UpdateVenueDto dto)
        {
            var venue = await _venueRepo.GetByIdAsync(venueId);

            if (venue == null)
                throw new Exception("القاعة غير موجودة");

            if (dto.PricingType == PricingType.Hourly)
            {
                if (!dto.PricePerHour.HasValue || dto.PricePerHour <= 0)
                    throw new Exception("Price per hour must be greater than 0 for hourly venues.");
            }
            else
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

            return venues.Select(MapVenue).ToList();
        }

        public async Task<List<VenueDto>> GetVenuesForGuestAsync()
        {
            var venues = await _venueRepo.GetAllActiveAsync();

            return venues.Select(MapVenue).ToList();
        }
    }
}
