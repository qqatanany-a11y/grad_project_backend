using Event.Application.Dtos;
using events.domain.Entities;
using events.domain.Repos;

namespace Event.Application.Services
{
    public class VenueService : IVenueService
    {
        private readonly IVenueRepo _venueRepo;

        public VenueService(IVenueRepo venueRepo)
        {
            _venueRepo = venueRepo;
        }

        public async Task<List<VenueDto>> GetByCompanyIdAsync(int companyId)
        {
            var venues = await _venueRepo.GetByCompanyIdAsync(companyId);
            return venues.Select(v => new VenueDto
            {
                Id = v.Id,
                Name = v.Name,
                Description = v.Description,
                City = v.City,
                Address = v.Address,
                Capacity = v.Capacity,
                MinimalPrice = v.MinimalPrice,
                IsActive = v.IsActive,
                CompanyName = v.Company?.Name
            }).ToList();
        }

        public async Task<VenueDto> AddAsync(AddVenueDto dto, int companyId)
        {
            var venue = new Venue(
                dto.Name,
                dto.Description,
                dto.City,
                dto.Address,
                dto.Capacity,
                dto.MinimalPrice,
                companyId
            );

            await _venueRepo.AddAsync(venue);

            return new VenueDto
            {
                Id = venue.Id,
                Name = venue.Name,
                Description = venue.Description,
                City = venue.City,
                Address = venue.Address,
                Capacity = venue.Capacity,
                MinimalPrice = venue.MinimalPrice,
                IsActive = venue.IsActive
            };
        }

        public async Task<VenueDto> UpdateAsync(int venueId, UpdateVenueDto dto, int companyId)
        {
            var venue = await _venueRepo.GetByIdAsync(venueId);

            if (venue == null)
                throw new Exception("القاعة غير موجودة");

            if (venue.CompanyId != companyId)
                throw new Exception("غير مصرح لك بتعديل هذه القاعة");

            venue.Update(dto.Name, dto.Description, dto.City, dto.Address, dto.Capacity, dto.MinimalPrice, dto.IsActive);

            await _venueRepo.UpdateAsync(venue);

            return new VenueDto
            {
                Id = venue.Id,
                Name = venue.Name,
                Description = venue.Description,
                City = venue.City,
                Address = venue.Address,
                Capacity = venue.Capacity,
                MinimalPrice = venue.MinimalPrice,
                IsActive = venue.IsActive
            };
        }

        public async Task DeleteAsync(int venueId, int companyId)
        {
            var venue = await _venueRepo.GetByIdAsync(venueId);

            if (venue == null)
                throw new Exception("القاعة غير موجودة");

            if (venue.CompanyId != companyId)
                throw new Exception("غير مصرح لك بحذف هذه القاعة");

            await _venueRepo.DeleteAsync(venue);
        }
    }
}