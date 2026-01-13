namespace DocLink.Core.Models;

public class SpecialistLocation
{
    public Guid Id { get; set; }

    public Guid LocationId { get; set; }
    public Location Location { get; set; }

    public Guid SpecialistId { get; set; }
    public Specialist Specialist { get; set; }
}