using DocLink.Core.Models;

namespace DocLink.Services.DTO_s;

public class SpecialistDetailsDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Specialization { get; set; }
    public string AboutMe { get; set; } 
    
    public List<SpecialistOfferDto> Offers { get; set; }
    public List<SpecialistLocationDto> SpecialistLocations { get; set; }
    public List<ReviewDto> Reviews { get; set; }
}