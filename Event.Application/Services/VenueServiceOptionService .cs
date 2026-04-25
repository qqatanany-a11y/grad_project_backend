using Event.Application.Dtos;
using Event.Application.IServices;
using events.domain.Entities;
using events.domain.Repos;

namespace Event.Application.Services
{
    public class VenueServiceOptionService : IVenueServiceOptionService
    {
        private readonly IVenueRepo _venueRepo;
        private readonly IServiceRepo _serviceRepo;
        private readonly IVenueServiceOptionRepo _venueServiceOptionRepo;

        public VenueServiceOptionService(
            IVenueRepo venueRepo,
            IServiceRepo serviceRepo,
            IVenueServiceOptionRepo venueServiceOptionRepo)
        {
            _venueRepo = venueRepo;
            _serviceRepo = serviceRepo;
            _venueServiceOptionRepo = venueServiceOptionRepo;
        }

        public async Task<VenueServiceOptionDto> AddAsync(AddVenueServiceOptionDto dto)
        {
            var venue = await _venueRepo.GetByIdAsync(dto.VenueId);
            if (venue == null)
                throw new Exception("Venue not found");

            var service = await _serviceRepo.GetByIdAsync(dto.ServiceId);
            if (service == null)
                throw new Exception("Service not found");

            var option = new VenueServiceOption(dto.VenueId, dto.ServiceId, dto.Price);

            await _venueServiceOptionRepo.AddAsync(option);

            return new VenueServiceOptionDto
            {
                Id = option.Id,
                VenueId = option.VenueId,
                ServiceId = option.ServiceId,
                ServiceName = service.Name,
                Price = option.Price,
                IsActive = option.IsActive
            };
        }

        public async Task<List<VenueServiceOptionDto>> GetByVenueIdAsync(int venueId)
        {
            var options = await _venueServiceOptionRepo.GetByVenueIdAsync(venueId);

            return options.Select(x => new VenueServiceOptionDto
            {
                Id = x.Id,
                VenueId = x.VenueId,
                ServiceId = x.ServiceId,
                ServiceName = x.Service.Name,
                Price = x.Price,
                IsActive = x.IsActive
            }).ToList();
        }
    }
}