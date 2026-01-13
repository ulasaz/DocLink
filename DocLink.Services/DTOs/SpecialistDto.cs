using DocLink.Core.Models;

namespace DocLink.Services.DTO_s;

public class SpecialistDto
{
    public Guid Id { get; set; }
    public string? FirstName { get; set; }
    public string LastName { get; set; }
    public string Specialization { get; set; }
}