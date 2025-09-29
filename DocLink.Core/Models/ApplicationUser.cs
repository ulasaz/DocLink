using Microsoft.AspNetCore.Identity;

namespace DocLink.Core.Models;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    
    public DoctorProfile DoctorProfile { get; set; }
    public PatientProfile PatientProfile { get; set; }
}