using Event.Application.IServices;
using events.domain.Entites;
using events.domain.Entities;
using events.domain.Repos;

namespace Event.Application.Services
{
    public class AdminService : IAdminService
    {
        private readonly IOwnerRequestRepo _ownerRequestRepo;
        private readonly IUserRepo _userRepo;
        private readonly ICompanyRepo _companyRepo;
        private readonly IVenueRepo _venueRepo;

        public AdminService(
            IOwnerRequestRepo ownerRequestRepo,
            IUserRepo userRepo,
            ICompanyRepo companyRepo,
            IVenueRepo venueRepo)
        {
            _ownerRequestRepo = ownerRequestRepo;
            _userRepo = userRepo;
            _companyRepo = companyRepo;
            _venueRepo = venueRepo;
        }

        public async Task<List<OwnerRequest>> GetOwnerRequestsAsync()
            => await _ownerRequestRepo.GetAllAsync();

        public async Task ApproveOwnerAsync(int id)
        {
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
                BCrypt.Net.BCrypt.HashPassword("Owner1234"), // just for now 
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
        }
        public async Task RejectOwnerAsync(int id, string reason)
        {
            var request = await _ownerRequestRepo.GetByIdAsync(id);

            if (request == null)
                throw new Exception("Request not found");

            request.Reject(reason);
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