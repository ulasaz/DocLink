namespace DocLink.Core.Models;

public class Specialist : Account
{
    public string? AboutMe{ get; set; }
    
    public List<Appointment> Appointments { get; set; }
    public List<Review> Reviews { get; set; }
    public List<SpecialistSchedule> Schedules { get; set; }
    public ICollection<OfferSpecialist> OffersSpecialists { get; set; } = new List<OfferSpecialist>();
    public ICollection<SpecialistLocation> SpecialistLocations { get; set; } = new List<SpecialistLocation>();
}