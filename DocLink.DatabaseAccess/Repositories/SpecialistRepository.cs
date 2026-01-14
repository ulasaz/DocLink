using DocLink.Core.Models;
using DocLink.Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DocLink.Data.Repositories;

public class SpecialistRepository : ISpecialistRepository
{
    private ApplicationContext _applicationContext;

    public SpecialistRepository(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }
    
    public async Task<IEnumerable<Specialist>> GetAllSpecialistsAsync()
    {
        return await _applicationContext.Set<Specialist>()
            .Include(o => o.OffersSpecialists)
            .ThenInclude(os => os.Offer)
            .AsNoTracking() 
            .ToListAsync();
    }

    public async Task<Specialist?> GetSpecialistById(Guid id)
    {
        return await _applicationContext.Specialists
            .Include(sh => sh.Schedules)
            .Include(r => r.Reviews)
            .Include(s => s.OffersSpecialists)
            .ThenInclude(os => os.Offer)
            .Include(s => s.SpecialistLocations)
            .ThenInclude(sl => sl.Location)
            .Include(s => s.Appointments)
            .Where(a => a.Id == id)
            .FirstOrDefaultAsync();
    }
}