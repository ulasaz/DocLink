namespace DocLink.Core.Models;

public class Review
{
    public Guid Id { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    
    public Guid PatientId { get; set; }
    public PatientProfile? PatientProfile { get; set; }
    
    public Guid DoctorId { get; set; }
    public DoctorProfile? DoctorProfile { get; set; }
    
}