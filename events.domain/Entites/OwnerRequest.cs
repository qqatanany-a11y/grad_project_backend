using events.domain.Entites;

public class OwnerRequest : BaseEntity
{
    public string Email { get; private set; }
    public string PhoneNumber { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }

    public string CompanyName { get; private set; }
    public string BusinessAddress { get; private set; }
    public string BusinessPhone { get; private set; }

    public string VenueName { get; private set; }

    public string Status { get; private set; } = "Pending";

    private OwnerRequest() { }

    public OwnerRequest(string email, string phone, string firstName, string lastName,
        string companyName, string address, string businessPhone, string venueName)
    {
        Email = email;
        PhoneNumber = phone;
        FirstName = firstName;
        LastName = lastName;
        CompanyName = companyName;
        BusinessAddress = address;
        BusinessPhone = businessPhone;
        VenueName = venueName;
    }

    public void Approve() => Status = "Approved";
    public void Reject() => Status = "Rejected";
}