using Event.Application.Dtos;
using Event.Application.IServices;
using events.domain.Entities;
using events.domain.Repos;

namespace Event.Application.Services
{
    public class ServiceCatalogService : IServiceCatalogService
    {
        private readonly IServiceRepo _serviceRepo;

        public ServiceCatalogService(IServiceRepo serviceRepo)
        {
            _serviceRepo = serviceRepo;
        }

        public async Task<ServiceDto> CreateAsync(CreateServiceDto dto)
        {
            var service = new Service(dto.Name, dto.Description);

            await _serviceRepo.AddAsync(service);

            return new ServiceDto
            {
                Id = service.Id,
                Name = service.Name,
                Description = service.Description
            };
        }

        public async Task<List<ServiceDto>> GetAllAsync()
        {
            var services = await _serviceRepo.GetAllAsync();

            return services.Select(x => new ServiceDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description
            }).ToList();
        }
    }
}