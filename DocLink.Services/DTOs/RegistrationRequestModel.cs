namespace DocLink.Services.DTO_s;

public class RegistrationRequestModel
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Role { get; set; }
    
    public string? Specialization { get; set; } 
    public string? LicenseId { get; set; }
}