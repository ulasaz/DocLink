namespace DocLink.Core.Models;

public class Review
{
    public Guid Id { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    
    public Guid PatientId { get; set; }
    public Patient? Patient { get; set; }
    
    public Guid SpecialistId { get; set; }
    public Specialist? Specialist { get; set; }
    
}