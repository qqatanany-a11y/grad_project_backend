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
        private readonly IEmailService _emailService;
        private readonly IPasswordGenerator _passwordGenerator;

        public AdminService(
            IOwnerRequestRepo ownerRequestRepo,
            IUserRepo userRepo,
            ICompanyRepo companyRepo,
            IVenueRepo venueRepo,
            IEmailService emailService,
             IPasswordGenerator passwordGenerator
            )

        {
            _ownerRequestRepo = ownerRequestRepo;
            _userRepo = userRepo;
            _companyRepo = companyRepo;
            _venueRepo = venueRepo;
            _emailService = emailService;
            _passwordGenerator = passwordGenerator;

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

            var generatedPassword = _passwordGenerator.Generate();

            var ownerUser = new User(
                request.Email,
                BCrypt.Net.BCrypt.HashPassword(generatedPassword),
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

        

            await _emailService.SendEmailAsync(
                request.Email,
                "Your owner account has been approved",
                $"Hello {request.FirstName},<br><br>Your account has been approved.<br>Your temporary password is: <b>{generatedPassword}</b><br><br>Please login and change it."
            );

            request.Approve();
            await _ownerRequestRepo.UpdateAsync(request);
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