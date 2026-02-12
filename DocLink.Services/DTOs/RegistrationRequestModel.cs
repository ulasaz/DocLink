namespace DocLink.Services.DTO_s;
using System.ComponentModel.DataAnnotations;

public class RegistrationRequestModel
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; }

    [Required(ErrorMessage = "First Name is required")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "Last Name is required")]
    public string LastName { get; set; }

    [Required(ErrorMessage = "Role is required")]
    public string Role { get; set; }
    
    public string? Specialization { get; set; } 
    public string? LicenseId { get; set; }
}