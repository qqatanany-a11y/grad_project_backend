using Event.Application.Dtos;
using Event.Application.IServices;
using events.domain.Entities;
using events.domain.Repos;
using System.Xml;

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
        public async Task<List<VenueDto>> GetByOwnerIdAsync(int OwnerId)
        {
            var user= await _userRepo.GetUserByIdAsync(OwnerId);
            if (user == null)
                 throw new Exception("المستخدم غير موجود");

            if(user.Role.Name!="Owner")
                throw new Exception("المستخدم ليس مالك");

            var venues = await _venueRepo.GetByOwnerId(OwnerId);
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
        public async Task<VenueDto> AddAsync(AddVenueDto dto)
        {
            var venue = new Venue(
                dto.Name,
                dto.Description,
                dto.City,
                dto.Address,
                dto.Capacity,
                dto.MinimalPrice,
                dto.CompanyId
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

        public async Task<VenueDto> UpdateAsync(int venueId, UpdateVenueDto dto)
        {
            var venue = await _venueRepo.GetByIdAsync(venueId);

            if (venue == null)
                throw new Exception("venue not exist");

  

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
        public async Task<VenueDto> GetByIdAsync(int venueId)
        {
            var venue = await _venueRepo.GetByIdAsync(venueId);
            if (venue == null)
                throw new Exception(" venue not exist");
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

        public async Task DeleteAsync(int venueId)
        {
            var venue = await _venueRepo.GetByIdAsync(venueId);

            if (venue == null)
                throw new Exception("venue not exist");

           
            await _venueRepo.DeleteAsync(venue);
        }

        public async Task<List<VenueDto>> GetAllAsync()
        {
            var venues = await _venueRepo.GetAllAsync();
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

        public async Task<List<VenueDto>> GetVenuesForGuestAsync()
        {
            var venues = await _venueRepo.GetAllActiveAsync();

            // convert data to Dto
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
                CompanyName = v.Company?.Name ?? "N/A",
                CompanyId = v.CompanyId
                
            }).ToList();
        }
    }
}