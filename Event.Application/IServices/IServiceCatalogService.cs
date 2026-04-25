using Event.Application.Dtos;

namespace Event.Application.IServices
{
    public interface IServiceCatalogService
    {
        Task<ServiceDto> CreateAsync(CreateServiceDto dto);
        Task<List<ServiceDto>> GetAllAsync();
    }
}