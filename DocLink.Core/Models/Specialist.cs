namespace DocLink.Core.Models;

public class Specialist : Account
{
    public string? AboutMe{ get; set; }
    
    public List<Appointment> Appointments { get; set; }
    public List<Review> Reviews { get; set; }
    public List<Offer> Offers { get; set; }
}