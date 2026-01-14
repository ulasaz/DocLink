using DocLink.Core.Models;
using DocLink.Data.Interfaces;
using DocLink.Services.DTO_s;
using DocLink.Services.Interfaces;

namespace DocLink.Services.Services;

public class SpecialistService : ISpecialistService
{
    private readonly ISpecialistRepository _specialistRepository;

    public SpecialistService(ISpecialistRepository specialistRepository)
    {
        _specialistRepository = specialistRepository;
    }

    public async Task<IEnumerable<SpecialistDto>> GetAllSpecialistsDtoAsync()
    {
       var specialists = await _specialistRepository.GetAllSpecialistsAsync();

       return specialists.Select(s => new SpecialistDto
       {
           Id = s.Id,
           FirstName = s.FirstName,
           LastName = s.LastName,
           
           Specialization = string.Join(", ", s.OffersSpecialists
               .Select(os => os.Offer.Title)
               .Where(t => !string.IsNullOrEmpty(t)))
       }).ToList();
    }


    public async Task<SpecialistDetailsDto> GetAllDataAboutSpecialist(Guid id)
    {
        var specialist = await _specialistRepository.GetSpecialistById(id);
        if (specialist == null) throw new Exception("Specialist not found");
        
        return new SpecialistDetailsDto
        {
            Id = specialist.Id,
            FirstName = specialist.FirstName,
            LastName = specialist.LastName,
            // AboutMe = specialist.AboutMe,
            
            Offers = specialist.OffersSpecialists.Select(os => new SpecialistOfferDto
            {
                Id = os.Offer.Id,
                Title = os.Offer.Title,
                Price = os.Offer.Price
            }).ToList(),
            
            SpecialistLocations = specialist.SpecialistLocations.Select(sl => new SpecialistLocationDto
            {
                City = sl.Location.City,
                Address = $"{sl.Location.Street} {sl.Location.Number}"
            }).ToList(),
            
            Reviews = specialist.Reviews.Select(r => new ReviewDto
            {
                AuthorName = "Patient",
                Rate = r.Rating,
                Content = r.Comment
            }).ToList()
        };
    }
}