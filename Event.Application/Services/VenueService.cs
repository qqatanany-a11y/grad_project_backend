using Event.Application.Dtos;
using Event.Application.IServices;
using Event.Application.Helpers;
using events.domain.Entities;
using events.domain.Repos;

namespace Event.Application.Services
{
    public class VenueService : IVenueService
    {
        private readonly IVenueRepo _venueRepo;
        private readonly IUserRepo _userRepo;

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
            {
                throw new Exception("User not found");
            }

            var venues = await _venueRepo.GetByOwnerId(ownerId);
            return venues.Select(venue => MapVenue(venue)).ToList();
        }

        public async Task<VenueDto> AddAsync(int companyId, AddVenueDto dto)
        {
            var pricingType = PricingType.FixedSlots;
            decimal? pricePerHour = null;

            ValidateVenue(pricingType, pricePerHour, dto.DepositPercentage);
            VenueSlotSupport.ValidateVenuePricing(pricingType, pricePerHour, dto.TimeSlots);

            var venue = new Venue(
                dto.Name,
                dto.Description,
                dto.City,
                dto.Address,
                dto.Capacity,
                companyId,
                dto.Type,
                dto.Category,
                pricingType,
                pricePerHour,
                dto.DepositPercentage,
                dto.FacebookUrl,
                dto.InstagramUrl,
                dto.WebsiteUrl);

            var resolvedImageUrls = dto.GetResolvedImageUrls();
            if (resolvedImageUrls.Count > 0)
            {
                venue.AddImages(resolvedImageUrls);
            }

            if (dto.TimeSlots != null)
            {
                VenueSlotSupport.SyncSlots(venue, dto.TimeSlots);
            }

            await _venueRepo.AddAsync(venue);
            return MapVenue(venue);
        }

        public async Task<string> UpdateAsync(int ownerId, int venueId, UpdateVenueDto dto)
        {
            var venue = await _venueRepo.GetByIdAsync(venueId);
            if (venue == null)
            {
                throw new Exception("Venue not found");
            }

            if (venue.Company.UserId != ownerId)
            {
                throw new Exception("Not allowed");
            }

            var pricingType = PricingType.FixedSlots;
            decimal? pricePerHour = null;

            ValidateVenue(pricingType, pricePerHour, dto.DepositPercentage);
            VenueSlotSupport.ValidateVenuePricing(pricingType, pricePerHour, dto.TimeSlots);

            venue.Update(
                dto.Name,
                dto.Description,
                dto.City,
                dto.Address,
                dto.Capacity,
                dto.IsActive,
                dto.Type,
                dto.Category,
                pricingType,
                pricePerHour,
                dto.DepositPercentage,
                dto.FacebookUrl,
                dto.InstagramUrl,
                dto.WebsiteUrl);

            var resolvedImageUrls = dto.GetResolvedImageUrls();
            if (resolvedImageUrls.Count > 0)
            {
                venue.AddImages(resolvedImageUrls);
            }

            if (dto.TimeSlots != null)
            {
                VenueSlotSupport.SyncSlots(venue, dto.TimeSlots);
            }

            await _venueRepo.UpdateAsync(venue);
            return "Venue updated successfully.";
        }

        public async Task DeleteAsync(int venueId)
        {
            var venue = await _venueRepo.GetByIdAsync(venueId);
            if (venue == null)
            {
                throw new Exception("Venue not found");
            }

            await _venueRepo.DeleteAsync(venue);
        }

        public async Task<List<VenueDto>> GetAllAsync()
        {
            var venues = await _venueRepo.GetAllAsync();
            return venues.Select(venue => MapVenue(venue)).ToList();
        }

        public async Task<VenueDto> GetByIdAsync(int venueId)
        {
            var venue = await _venueRepo.GetByIdAsync(venueId);
            if (venue == null)
            {
                throw new Exception("Venue not found");
            }

            return MapVenue(venue);
        }

        public async Task<List<VenueDto>> SearchAsync(VenueQueryParams query)
        {
            var venues = await _venueRepo.GetAllActiveAsync();
            var result = venues.AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.Trim().ToLowerInvariant();
                result = result.Where(v =>
                    v.Name.ToLower().Contains(search) ||
                    v.City.ToLower().Contains(search) ||
                    v.Description.ToLower().Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(query.City))
            {
                var city = query.City.Trim().ToLowerInvariant();
                result = result.Where(v => v.City.ToLower() == city);
            }

            if (query.Type.HasValue)
            {
                result = result.Where(v => v.Type == query.Type.Value);
            }

            if (query.MinCapacity.HasValue)
            {
                result = result.Where(v => v.Capacity >= query.MinCapacity.Value);
            }

            if (query.MaxCapacity.HasValue)
            {
                result = result.Where(v => v.Capacity <= query.MaxCapacity.Value);
            }

            if (query.MinPrice.HasValue)
            {
                result = result.Where(v => v.PricePerHour.HasValue && v.PricePerHour.Value >= query.MinPrice.Value);
            }

            if (query.MaxPrice.HasValue)
            {
                result = result.Where(v => v.PricePerHour.HasValue && v.PricePerHour.Value <= query.MaxPrice.Value);
            }

            if (query.MinRating.HasValue)
            {
                result = result.Where(v => v.Reviews.Any() && v.Reviews.Average(r => r.Rating) >= query.MinRating.Value);
            }

            if (query.Date.HasValue && query.StartTime.HasValue && query.EndTime.HasValue)
            {
                var date = query.Date.Value.Date;
                var startTime = query.StartTime.Value;
                var endTime = query.EndTime.Value;

                result = result.Where(v =>
                    (!v.TimeSlots.Any(slot => slot.IsActive) ||
                     v.TimeSlots.Any(slot =>
                        slot.IsActive &&
                        startTime >= slot.StartTime &&
                        endTime <= slot.EndTime)) &&
                    !v.Bookings.Any(b =>
                        b.BookingDate.Date == date &&
                        b.Status == BookingStatusEnum.Confirmed &&
                        startTime < b.EndTime &&
                        endTime > b.StartTime));
            }

            result = query.SortBy?.ToLowerInvariant() switch
            {
                "price" => query.SortOrder == "desc"
                    ? result.OrderByDescending(v => v.PricePerHour)
                    : result.OrderBy(v => v.PricePerHour),
                "capacity" => query.SortOrder == "desc"
                    ? result.OrderByDescending(v => v.Capacity)
                    : result.OrderBy(v => v.Capacity),
                "rating" => query.SortOrder == "desc"
                    ? result.OrderByDescending(v => v.Reviews.Any() ? v.Reviews.Average(r => r.Rating) : 0)
                    : result.OrderBy(v => v.Reviews.Any() ? v.Reviews.Average(r => r.Rating) : 0),
                _ => query.SortOrder == "desc"
                    ? result.OrderByDescending(v => v.Name)
                    : result.OrderBy(v => v.Name)
            };

            var page = query.Page <= 0 ? 1 : query.Page;
            var pageSize = query.PageSize <= 0 ? 10 : query.PageSize;

            return result
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(venue => MapVenue(venue, true))
                .ToList();
        }

        public async Task<List<VenueDto>> GetVenuesForGuestAsync()
        {
            var venues = await _venueRepo.GetAllActiveAsync();
            return venues.Select(venue => MapVenue(venue, true)).ToList();
        }

        private static VenueDto MapVenue(Venue venue, bool activeSlotsOnly = false)
        {
            var orderedImageUrls = VenueImageRequestHelper.OrderExistingImages(venue.Images);

            return new VenueDto
            {
                Id = venue.Id,
                Name = venue.Name,
                Description = venue.Description,
                City = venue.City,
                Address = venue.Address,
                Capacity = venue.Capacity,
                IsActive = venue.IsActive,
                CompanyName = venue.Company?.Name ?? string.Empty,
                CompanyId = venue.CompanyId,
                Type = venue.Type,
                Category = venue.Category,
                PricingType = PricingType.FixedSlots,
                PricePerHour = null,
                DepositPercentage = venue.DepositPercentage,
                FacebookUrl = venue.FacebookUrl,
                InstagramUrl = venue.InstagramUrl,
                WebsiteUrl = venue.WebsiteUrl,
                AverageRating = venue.Reviews.Any() ? venue.Reviews.Average(r => r.Rating) : 0,
                CoverPhotoUrl = venue.Images
                    .OrderByDescending(image => image.IsCover)
                    .ThenBy(image => image.Id)
                    .Select(image => image.ImageUrl)
                    .FirstOrDefault(),
                ImageUrls = orderedImageUrls,
                TimeSlots = VenueSlotSupport.MapSlots(venue.TimeSlots, activeSlotsOnly)
            };
        }

        private static void ValidateVenue(PricingType pricingType, decimal? pricePerHour, decimal depositPercentage)
        {
            if (pricePerHour.HasValue && pricePerHour.Value < 0)
            {
                throw new Exception("Venue price must be greater than or equal to 0.");
            }

            // depositPercentage = 0 is allowed; payment logic will apply a 10% fallback automatically
            if (depositPercentage < 0 || depositPercentage > 100)
            {
                throw new Exception("Deposit percentage must be between 0 and 100.");
            }
        }
    }
}
