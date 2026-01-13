using DocLink.Core.Models;

namespace DocLink.Services.DTO_s;

public class AppointmentDto
{
    public Guid Id { get; set; }
    public DateTime Time { get; set; }
    public string? Status { get; set; }
    public string? SpecialistName { get; set; }
    public Guid SpecialistId { get; set; }
    public string? Street { get; set; } 
    public string? City { get; set; }
    public List<string> Offers { get; set; } 
}