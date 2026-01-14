namespace DocLink.Services.DTO_s;

public class AppointmentRequestModel
{
    public Guid PatientId { get; set; }
    public Guid SpecialistId { get; set; }
    public DateTime Time { get; set; }
    public string Status { get; set; }
    public Guid OfferId { get; set; }
}