using Event.Application.Dtos;
using Event.Application.IServices;
using Event.Infrastructure.Repos;
using events.domain.Entities;
using events.domain.Repos;
using System.Text.Json;
namespace Event.Application.Services
{
    public class VenueService : IVenueService
    {
        private readonly IVenueRepo _venueRepo;
        private readonly IUserRepo _userRepo;
        private readonly IEmailService _emailService;
        private readonly IEditRequestRepo _editRequestRepo;
        public VenueService(
     IVenueRepo venueRepo,
     IUserRepo userRepo,
     IEmailService emailService,
     IEditRequestRepo editRequestRepo)
        {
            _venueRepo = venueRepo;
            _userRepo = userRepo;
            _emailService = emailService;
            _editRequestRepo = editRequestRepo;
        }

        public async Task<List<VenueDto>> GetByCompanyIdAsync(int companyId)
        {
            var venues = await _venueRepo.GetByCompanyIdAsync(companyId);

            return venues.Select(v => MapToDto(v)).ToList();
        }

        public async Task<List<VenueDto>> GetByOwnerIdAsync(int ownerId)
        {
            var user = await _userRepo.GetUserByIdAsync(ownerId);

            if (user == null)
            {
                throw new Exception("user not found");
            }

            if (user.Role.Name != "Owner")
                throw new Exception("You do not have permission to access this resource.");

            var venues = await _venueRepo.GetByOwnerId(ownerId);

            return venues.Select(v => MapToDto(v)).ToList();
        }

        public async Task<VenueDto> AddAsync(int companyId, AddVenueDto dto)
        {
            ValidateVenue(dto.PricingType, dto.PricePerHour, dto.DepositPercentage);

           


            if (dto.ImageUrls == null || dto.ImageUrls.Count < 10)
                throw new Exception("You must upload at least 10 images.");

            if (dto.PricingType != PricingType.Hourly)
                dto.PricePerHour = null;

            var venue = new Venue(
                dto.Name,
                dto.Description,
                dto.City,
                dto.Address,
                dto.Capacity,
                companyId,
                dto.Type,
                dto.PricingType,
                dto.PricePerHour,
                dto.DepositPercentage,
                dto.FacebookUrl,
    dto.InstagramUrl,
    dto.WebsiteUrl
            );
            if (dto.TimeSlots != null && dto.TimeSlots.Any())
            {
                var slots = dto.TimeSlots.Select(s =>
                    new VenueTimeSlot(s.Day, s.StartTime, s.EndTime)
                ).ToList();

                venue.SetTimeSlots(slots);
            }

            venue.AddImages(dto.ImageUrls);

            await _venueRepo.AddAsync(venue);

            var owner = await _userRepo.GetUserByIdAsync(venue.Company.UserId);

            await _emailService.SendEmailAsync(
                owner.Email,
                "Venue Submitted",
                $@"
    <p>Dear {owner.FirstName},</p>

    <p>Your venue has been submitted successfully.</p>

    <p>Our team will conduct a field inspection visit to verify your venue.</p>

    <p>Best regards,<br/>Events Team</p>
    "
            );

            return MapToDto(venue);
        }

        public async Task<string> UpdateAsync(int ownerId, int venueId, UpdateVenueDto dto)
        {
            var venue = await _venueRepo.GetByIdAsync(venueId);

            if (venue == null)
                throw new Exception("Venue not found");

            
            if (venue.Company.UserId != ownerId)
                throw new Exception("Not allowed");

            ValidateVenue(dto.PricingType, dto.PricePerHour, dto.DepositPercentage);

            if (dto.PricingType != PricingType.Hourly)
                dto.PricePerHour = null;

            var jsonData = JsonSerializer.Serialize(dto);

            var editRequest = new EditRequest(
                ownerId,
                EditRequestTypeEnum.VenueUpdate,
                venueId,
                jsonData
            );

            await _editRequestRepo.AddAsync(editRequest);

            var owner = await _userRepo.GetUserByIdAsync(ownerId);

            if (owner == null)
                throw new Exception("Owner not found");

            await _emailService.SendEmailAsync(
                owner.Email,
                "Update Request Received",
                $@"
        <p>Dear {owner.FirstName},</p>

        <p>Your venue update request has been received successfully.</p>

        <p>We will review your request within <strong>24-48 hours</strong>.</p>

        <p>Best regards,<br/>Events Team</p>
        "
            );

            return "Your update request has been sent for review.";
        }

        public async Task<VenueDto> GetByIdAsync(int venueId)
        {
            var venue = await _venueRepo.GetByIdAsync(venueId);

            if (venue == null)
                throw new Exception("venue not exist");

            return MapToDto(venue);
        }

        public async Task DeleteAsync(int venueId)
        {
            var venue = await _venueRepo.GetByIdAsync(venueId);

            if (venue == null)
                throw new Exception("venue is not exist");

            await _venueRepo.DeleteAsync(venue);
        }

        public async Task<List<VenueDto>> GetAllAsync()
        {
            var venues = await _venueRepo.GetAllAsync();

            return venues.Select(v => MapToDto(v)).ToList();
        }

        public async Task<List<VenueDto>> GetVenuesForGuestAsync()
        {
            var venues = await _venueRepo.GetAllActiveAsync();

            return venues.Select(venue => MapVenue(venue, true)).ToList();
        }

        private static void ValidateVenue(
            PricingType pricingType,
            decimal? pricePerHour,
            decimal depositPercentage)
        {
            if (pricingType == PricingType.Hourly)
            {
                if (!pricePerHour.HasValue || pricePerHour <= 0)
                    throw new Exception("Price per hour must be greater than 0 for hourly venues.");
            }

            if (depositPercentage <= 0 || depositPercentage > 100)
                throw new Exception("Deposit percentage must be between 1 and 100.");
        }

        private static VenueDto MapToDto(Venue venue)
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
                CompanyName = venue.Company?.Name ?? "N/A",
                CompanyId = venue.CompanyId,
                Type = venue.Type,
                PricingType = venue.PricingType,
                PricePerHour = venue.PricePerHour,
                DepositPercentage = venue.DepositPercentage,
                FacebookUrl = venue.FacebookUrl,
                InstagramUrl = venue.InstagramUrl,
                WebsiteUrl = venue.WebsiteUrl,
            };
        }

        private static VenueDto MapVenue(Venue venue, bool v)
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

                PricingType = venue.PricingType,
                PricePerHour = venue.PricePerHour,
                DepositPercentage = venue.DepositPercentage,
                Type = venue.Type,

                FacebookUrl = venue.FacebookUrl,
                InstagramUrl = venue.InstagramUrl,
                WebsiteUrl = venue.WebsiteUrl,

                AverageRating = venue.Reviews.Any()
                    ? venue.Reviews.Average(r => r.Rating)
                    : 0,

                TimeSlots = venue.TimeSlots.Select(t => new VenueTimeSlotDto
                {
                    Day = t.Day,
                    StartTime = t.StartTime,
                    EndTime = t.EndTime
                }).ToList()
            };
        }
        public async Task<List<VenueDto>> SearchAsync(VenueQueryParams query)
        {
            var venues = await _venueRepo.GetAllActiveAsync();

            var result = venues.AsQueryable();


            if (query.MinRating.HasValue)
            {
                result = result.Where(v =>
                    v.Reviews.Any() &&
                    v.Reviews.Average(r => r.Rating) >= query.MinRating.Value
                );
            }

            // 🔍 SEARCH
            if (!string.IsNullOrEmpty(query.Search))
            {
                var search = query.Search.ToLower();

                result = result.Where(v =>
                    v.Name.ToLower().Contains(search) ||
                    v.City.ToLower().Contains(search) ||
                    v.Description.ToLower().Contains(search)
                
                );
            }

            // 🎯 FILTER
            if (!string.IsNullOrEmpty(query.City))
                result = result.Where(v => v.City.ToLower() == query.City.ToLower());

            if (query.Type.HasValue)
                result = result.Where(v => v.Type == query.Type);

            if (query.MinCapacity.HasValue)
                result = result.Where(v => v.Capacity >= query.MinCapacity);

            if (query.MaxCapacity.HasValue)
                result = result.Where(v => v.Capacity <= query.MaxCapacity);

            if (query.MinPrice.HasValue)
                result = result.Where(v => v.PricePerHour >= query.MinPrice);

            if (query.MaxPrice.HasValue)
                result = result.Where(v => v.PricePerHour <= query.MaxPrice);

            if (query.Date.HasValue && query.StartTime.HasValue && query.EndTime.HasValue)
            {
                result = result.Where(v =>

                    // ✅ أولاً: لازم يكون ضمن TimeSlot
                    v.TimeSlots.Any(t =>
                        t.Day == query.Date.Value.DayOfWeek &&
                        query.StartTime >= t.StartTime &&
                        query.EndTime <= t.EndTime
                    )

                    &&

                    // ❌ ما يكون محجوز
                    !v.Bookings.Any(b =>
                        b.BookingDate.Date == query.Date.Value.Date &&
                        b.Status == BookingStatusEnum.Confirmed &&
                        (
                            query.StartTime < b.EndTime &&
                            query.EndTime > b.StartTime
                        )
                    )
                );
            }

            // 🔃 SORT
            result = query.SortBy?.ToLower() switch
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
                    : result.OrderBy(v => v.Name),
            };

            // 📄 PAGINATION
            var page = query.Page <= 0 ? 1 : query.Page;

            result = result
                .Skip((page - 1) * query.PageSize)
                .Take(query.PageSize);

            return result.Select(v => MapVenue(v, true)).ToList();
        }




    }


}