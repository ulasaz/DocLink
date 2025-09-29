using System.Runtime.CompilerServices;

namespace DocLink.Core.Models;

public class Patient : Account
{
    public Guid Id { get; set; }
    public DateTime BirthDate { get; set; }
    
    public List<Review> Reviews { get; set; }
    public List<Appointment> Appointments { get; set; }
    
}