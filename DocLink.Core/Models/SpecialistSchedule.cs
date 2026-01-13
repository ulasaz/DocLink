namespace DocLink.Core.Models;

public class SpecialistSchedule
{
    public Guid Id { get; set; }

    public Guid SpecialistId { get; set; }
    public Specialist? Specialist { get; set; }

    public TimeOnly AvailableFrom { get; set; }
    public TimeOnly AvailableTo { get; set; }
}