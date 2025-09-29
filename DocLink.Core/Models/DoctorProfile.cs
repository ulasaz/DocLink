namespace DocLink.Core.Models;

public class DoctorProfile
{
    public Guid Id { get; set; }
    
    public ApplicationUser User { get; set; }
    public string? AboutMe{ get; set; }
    
    public List<Offer> Offers { get; set; }
}