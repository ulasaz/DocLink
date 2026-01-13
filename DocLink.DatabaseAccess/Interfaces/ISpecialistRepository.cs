using DocLink.Core.Models;

namespace DocLink.Data.Interfaces;

public interface ISpecialistRepository
{
    Task<IEnumerable<Specialist>> GetAllSpecialistsAsync();
    Task<Specialist?> GetSpecialistById(Guid id);
}