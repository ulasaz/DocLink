namespace DocLink.Core.Models;

public class Appointment
{
    public Guid Id { get; set; }
    public DateTime Time { get; set; }
    
    public PatientProfile? PatientProfile { get; set; }
    public DoctorProfile? DoctorProfile { get; set; }
}