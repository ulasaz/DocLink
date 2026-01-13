namespace DocLink.Services.DTO_s;

public class AppointmentResponseModel
{
    public string? SpecialistFirstName { get; set; }
    public string? SpecialistLastName { get; set; }
    public string? PatientFirstName { get; set; }
    public string? PatientLastName { get; set; }
    public DateTime Time { get; set; }
    public bool IsSuccessful { get; set; }
    public IEnumerable<string>? Errors { get; set; }
}