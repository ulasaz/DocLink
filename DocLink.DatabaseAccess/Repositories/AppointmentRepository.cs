using DocLink.Core.Models;
using DocLink.Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DocLink.Data.Repositories;

public class AppointmentRepository : IAppointmentRepository
{
    private ApplicationContext _applicationContext;

    public AppointmentRepository(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }

    public async Task<Appointment> AddAppointmentAsync(Appointment appointment)
    {
        ArgumentNullException.ThrowIfNull(appointment);
        
        await _applicationContext.Appointments.AddAsync(appointment);
        await _applicationContext.SaveChangesAsync();
        return appointment;
    }
    public async Task<IEnumerable<Appointment>> GetAppointmentsByPatientIdAsync(Guid patientId)
    {
        return await _applicationContext.Appointments
            .Where(a => a.PatientId == patientId)
            .Include(a => a.Specialist)
            .ThenInclude(s => s.OffersSpecialists) 
            .ThenInclude(os => os.Offer) 
            .Include(a => a.Offer)
            .Include(a => a.Specialist)
            .ThenInclude(s => s.SpecialistLocations)
            .ThenInclude(sl => sl.Location)
            .OrderByDescending(a => a.Time)
            .ToListAsync();
    }
}