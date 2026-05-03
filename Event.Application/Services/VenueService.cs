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
            IEmailService emailService;
            _editRequestRepo = _editRequestRepo;
        }

        public async Task<List<VenueDto>> GetByCompanyIdAsync(int companyId)
        {
            var venues = await _venueRepo.GetByCompanyIdAsync(companyId);

            return venues.Select(v => MapToDto(v)).ToList();
            return venues.Select(venue => MapVenue(venue)).ToList();
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

            return venues.Select(venue => MapVenue(venue)).ToList();
        }

        public async Task<VenueDto> AddAsync(int companyId, AddVenueDto dto)
        {
            ValidateVenue(dto.PricingType, dto.PricePerHour, dto.DepositPercentage);

            if (dto.ImageUrls == null || dto.ImageUrls.Count < 10)
                throw new Exception("You must upload at least 10 images.");

            if (dto.PricingType != PricingType.Hourly)
                dto.PricePerHour = null;

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
                dto.Type,
                dto.Category,
                dto.PricingType,
                dto.PricePerHour,
                dto.DepositPercentage,
                dto.FacebookUrl,
    dto.InstagramUrl,
    dto.WebsiteUrl
            );

            venue.AddImages(dto.ImageUrls);

            if (dto.TimeSlots != null)
            {
                VenueSlotSupport.SyncSlots(venue, dto.TimeSlots);
            }

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
            return MapVenue(venue);
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
                throw new Exception("القاعة غير موجودة");

            VenueSlotSupport.ValidateVenuePricing(dto.PricingType, dto.PricePerHour, dto.TimeSlots);

            if (dto.PricingType != PricingType.Hourly && (dto.TimeSlots == null || dto.TimeSlots.Count == 0))
            {
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
                throw new Exception("venue not exist");

            return MapToDto(venue);
                throw new Exception("القاعة غير موجودة");
            return MapVenue(venue);
        }

        public async Task DeleteAsync(int venueId)
        {
            var venue = await _venueRepo.GetByIdAsync(venueId);

            if (venue == null)
                throw new Exception("venue is not exist");
                throw new Exception("القاعة غير موجودة");

            await _venueRepo.DeleteAsync(venue);
        }

        public async Task<List<VenueDto>> GetAllAsync()
        {
            var venues = await _venueRepo.GetAllAsync();

            return venues.Select(v => MapToDto(v)).ToList();
            return venues.Select(venue => MapVenue(venue)).ToList();
        }

        public async Task<List<VenueDto>> GetVenuesForGuestAsync()
        {
            var venues = await _venueRepo.GetAllActiveAsync();

            return venues.Select(v => MapToDto(v)).ToList();
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
                DepositPercentage = venue.DepositPercentage
            };
            return venues.Select(venue => MapVenue(venue, true)).ToList();
        }
    }
}
