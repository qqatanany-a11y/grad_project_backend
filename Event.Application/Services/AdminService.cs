using Event.Application.Dtos;
using Event.Application.IServices;
using events.domain.Entites;
using events.domain.Entities;
using events.domain.Repos;
using System.Text.Json;
namespace Event.Application.Services
{
    public class AdminService : IAdminService
    {
        private readonly IOwnerRequestRepo _ownerRequestRepo;
        private readonly IUserRepo _userRepo;
        private readonly ICompanyRepo _companyRepo;
        private readonly IVenueRepo _venueRepo;
        private readonly IPasswordGenerator _passwordGenerator;
        private readonly IEmailService _emailService;
        private readonly IEditRequestRepo _editRequestRepo;

        public AdminService(
            IOwnerRequestRepo ownerRequestRepo,
            IUserRepo userRepo,
            ICompanyRepo companyRepo,
            IVenueRepo venueRepo,
             IPasswordGenerator passwordGenerator,
    IEmailService emailService,
    IEditRequestRepo editRequestRepo
            )
            IVenueRepo venueRepo)
        {
            _ownerRequestRepo = ownerRequestRepo;
            _userRepo = userRepo;
            _companyRepo = companyRepo;
            _venueRepo = venueRepo;
            _passwordGenerator = passwordGenerator;
            _emailService = emailService;
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


            var generatedPassword = _passwordGenerator.Generate(10);

          
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


            request.Approve();
            await _ownerRequestRepo.UpdateAsync(request);

            await _emailService.SendEmailAsync(
    request.Email,
    "Owner Account Approved 🎉",
    $@"
    <h2>Welcome to Events Platform 🎉</h2>

    <p>Dear {request.FirstName},</p>

    <p>Your request to become an <strong>Owner</strong> has been approved.</p>

    <h3>Your Login Details:</h3>
    <ul>
        <li><strong>Email:</strong> {request.Email}</li>
        <li><strong>Password:</strong> {generatedPassword}</li>
    </ul>

    <p>Please login and change your password immediately.</p>

    <br/>

    <p>Best regards,<br/>Events Team</p>
    "
);
        }

        public async Task OwnerRequestAsync(RegisterOwnerDto dto)
        {
            if(dto.Email == null || dto.FirstName == null || dto.LastName == null || dto.PhoneNumber == null || dto.CompanyName == null || dto.BusinessAddress == null || dto.BusinessPhone == null)
                throw new Exception("All fields are required");

            var existingUser = await _userRepo.GetUserByEmailAsync(dto.Email);
            if (existingUser != null)
                throw new Exception("User with this email already exists");
            var request = new OwnerRequest(
                dto.Email,
                dto.PhoneNumber,
                dto.FirstName,
                dto.LastName,
                dto.CompanyName,
                dto.BusinessAddress,
                dto.BusinessPhone
            var venue = new Venue(
                request.VenueName,
                "Pending description",
                "Amman",
                request.BusinessAddress,
                100,
                company.Id,
                VenueCategory.WeddingHall,
                PricingType.FixedSlots,
                null
            );
            await _ownerRequestRepo.AddAsync(request);
                
            await _emailService.SendEmailAsync(
                    dto.Email,
                    "Owner Account Request Received",
                    $@"
                    <h2>Events Platform</h2>
                    <p>Dear {dto.FirstName},</p>
                    <p>Thank you for your interest in becoming an <strong>Owner</strong> on our platform. We have received your request and will review it shortly.</p>
                    <p>You will receive an email notification once we have processed your request.</p>
                    <br/>
                    <p>Best regards,<br/>Events Team</p>
                    "
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

            await _emailService.SendEmailAsync(
  request.Email,
  "Owner Account Request Update",
  $@"
    <h2>Events Platform</h2>

    <p>Dear {request.FirstName},</p>

    <p>We regret to inform you that your request to become an <strong>Owner</strong> has been <strong>rejected</strong>.</p>

    <p><strong>Reason:</strong> {reason}</p>

    <p>If you believe this was a mistake or need further clarification, please feel free to contact our support team.</p>

    <br/>

    <p>Best regards,<br/>Events Team</p>
    "
);

            await _ownerRequestRepo.UpdateAsync(request);
        }

        public async Task<List<Company>> GetCompaniesAsync()
            => await _companyRepo.GetAllAsync();

        public async Task<List<Venue>> GetVenuesAsync()
            => await _venueRepo.GetAllAsync();

        public async Task<List<User>> GetUsersAsync()
            => await _userRepo.GetAllUsersAsync();



        public async Task ApproveVenueUpdate(int requestId, int adminId)
        {
            var request = await _editRequestRepo.GetByIdAsync(requestId);

            if (request == null)
                throw new Exception("Request not found");

            if (request.Type != EditRequestTypeEnum.VenueUpdate)
                throw new Exception("Invalid request type");

            var venue = await _venueRepo.GetByIdAsync(request.TargetId!.Value);

            if (venue == null)
                throw new Exception("Venue not found");

            // 🔥 فك JSON
            var data = JsonSerializer.Deserialize<UpdateVenueDto>(request.RequestedDataJson);

            if (data == null)
                throw new Exception("Invalid request data");

            // 🔥 تطبيق التعديل
            venue.Update(
                data.Name,
                data.Description,
                data.City,
                data.Address,
                data.Capacity,
                data.IsActive,
                data.Type,
                data.PricingType,
                data.PricePerHour,
                data.DepositPercentage,
                data.FacebookUrl,
        data.InstagramUrl,
        data.WebsiteUrl
            );

            request.Approve(adminId);

            await _venueRepo.UpdateAsync(venue);
            await _editRequestRepo.UpdateAsync(request);

            // 📩 EMAIL
            await _emailService.SendEmailAsync(
                venue.Company.Email,
                "Venue Update Approved ✅",
                $@"
        <p>Dear {venue.Company.Name},</p>

        <p>Your venue update request has been <strong>approved</strong>.</p>

        <p>The changes are now live.</p>

        <p>Best regards,<br/>Events Team</p>
        "
            );
        }
        public async Task RejectVenueUpdate(int requestId, int adminId, string reason)
        {
            var request = await _editRequestRepo.GetByIdAsync(requestId);

            if (request == null)
                throw new Exception("Request not found");

            request.Reject(adminId, reason);

            await _editRequestRepo.UpdateAsync(request);

            var venue = await _venueRepo.GetByIdAsync(request.TargetId!.Value);

            // 📩 EMAIL
            await _emailService.SendEmailAsync(
                venue.Company.Email,
                "Venue Update Rejected ❌",
                $@"
        <p>Dear {venue.Company.Name},</p>

        <p>Your venue update request has been <strong>rejected</strong>.</p>

        <p><strong>Reason:</strong> {reason}</p>

        <p>Please update the data and try again.</p>

        <p>Best regards,<br/>Events Team</p>
        "
            );
        }
    } }

    }
}
