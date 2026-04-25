using Event.Application.Dtos;
using Event.Application.IServices;
using events.domain.Entites;
using events.domain.Entities;
using events.domain.Repos;
using Microsoft.Extensions.Logging;

namespace Event.Application.Services
{
    public class AdminService : IAdminService
    {
        private readonly IOwnerRequestRepo _ownerRequestRepo;
        private readonly IUserRepo _userRepo;
        private readonly ICompanyRepo _companyRepo;
        private readonly IVenueRepo _venueRepo;
        private readonly ILogger<AdminService> _logger;

        public AdminService(
            IOwnerRequestRepo ownerRequestRepo,
            IUserRepo userRepo,
            ICompanyRepo companyRepo,
            IVenueRepo venueRepo,
            ILogger<AdminService> logger)
        {
            _ownerRequestRepo = ownerRequestRepo;
            _userRepo = userRepo;
            _companyRepo = companyRepo;
            _venueRepo = venueRepo;
            _logger = logger;
        }

        public async Task<List<OwnerRequestDto>> GetOwnerRequestsAsync()
        {
            var requests = await _ownerRequestRepo.GetAllAsync();

            return requests.Select(request => new OwnerRequestDto
            {
                Id = request.Id,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                FirstName = request.FirstName,
                LastName = request.LastName,
                CompanyName = request.CompanyName,
                BusinessAddress = request.BusinessAddress,
                BusinessPhone = request.BusinessPhone,
                VenueName = request.VenueName,
                Status = request.Status,
                CreatedAt = request.CreatedAt
            }).ToList();
        }

        public async Task<ApproveOwnerResponseDto> ApproveOwnerAsync(int id)
        {
            const string ownerPassword = "Owner1234";
            var request = await _ownerRequestRepo.GetByIdAsync(id);

            if (request == null)
                throw new Exception("Request not found");

            if (request.Status != "Pending")
                throw new Exception("Already processed");

            var existingUser = await _userRepo.GetUserByEmailAsync(request.Email);
            if (existingUser != null)
                throw new Exception("User already exists");

            var ownerUser = new User(
                request.Email,
                BCrypt.Net.BCrypt.HashPassword(ownerPassword),
                request.PhoneNumber,
                request.FirstName,
                request.LastName,
                3
            );

            await _userRepo.AddUserAsync(ownerUser);

            var company = new Company(
                request.CompanyName,
                request.BusinessAddress,
                request.BusinessPhone,
                request.Email,
                ownerUser.Id
            );

            await _companyRepo.AddAsync(company);

            var venue = new Venue(
                request.VenueName,
                "Pending description",
                "Amman",
                request.BusinessAddress,
                100,
                0,
                company.Id
            );

            await _venueRepo.AddAsync(venue);

            request.Approve();
            await _ownerRequestRepo.UpdateAsync(request);

            Console.WriteLine("==================================================");
            Console.WriteLine("OWNER ACCOUNT CREATED");
            Console.WriteLine($"Request ID: {request.Id}");
            Console.WriteLine($"Company: {request.CompanyName}");
            Console.WriteLine($"Email: {ownerUser.Email}");
            Console.WriteLine($"Password: {ownerPassword}");
            Console.WriteLine("==================================================");

            _logger.LogInformation(
                "Owner account created after approving request {RequestId}. Email: {Email}, Password: {Password}",
                request.Id,
                ownerUser.Email,
                ownerPassword);

            return new ApproveOwnerResponseDto
            {
                Message = "Owner account created successfully",
                RequestId = request.Id,
                CompanyName = request.CompanyName,
                Email = ownerUser.Email,
                Password = ownerPassword
            };
        }

        public async Task RejectOwnerAsync(int id)
        {
            var request = await _ownerRequestRepo.GetByIdAsync(id);

            if (request == null)
                throw new Exception("Request not found");

            request.Reject();
            await _ownerRequestRepo.UpdateAsync(request);
        }

        public async Task<List<Company>> GetCompaniesAsync()
            => await _companyRepo.GetAllAsync();

        public async Task<List<Venue>> GetVenuesAsync()
            => await _venueRepo.GetAllAsync();

        public async Task<List<User>> GetUsersAsync()
            => await _userRepo.GetAllUsersAsync();
    }
}
