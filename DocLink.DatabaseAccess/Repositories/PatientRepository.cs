using DocLink.Data.Interfaces;

namespace DocLink.Data.Repositories;

public class PatientRepository : IPatientRepository
{
    private ApplicationContext _context;

    public PatientRepository(ApplicationContext context)
    {
        _context = context;
    }
    
}