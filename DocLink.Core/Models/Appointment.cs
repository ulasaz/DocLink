namespace DocLink.Core.Models;

public class Appointment
{
    public Guid Id { get; set; }
    public DateTime Time { get; set; }
    public string? Status { get; set; }
    
    public Patient? Patient { get; set; }
    public Guid PatientId { get; set; }
    
    public Guid? OfferId { get; set; } 
    public Offer Offer { get; set; }
    
    public Specialist? Specialist { get; set; }
    public Guid SpecialistId { get; set; }
}