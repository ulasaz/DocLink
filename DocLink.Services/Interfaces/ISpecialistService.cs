using DocLink.Services.DTO_s;

namespace DocLink.Services.Interfaces;

public interface ISpecialistService
{
    Task<IEnumerable<SpecialistDto>> GetAllSpecialistsDtoAsync();
    Task<SpecialistDetailsDto> GetAllDataAboutSpecialist(Guid id);
}