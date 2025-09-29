using System.Runtime.CompilerServices;

namespace DocLink.Core.Models;

public class PatientProfile
{
    public Guid Id { get; set; }
    public ApplicationUser? User { get; set; }
    public DateTime BirthDate { get; set; }
    
    public List<Review> Reviews { get; set; }
    public List<Appointment> Appointments { get; set; }
    
}