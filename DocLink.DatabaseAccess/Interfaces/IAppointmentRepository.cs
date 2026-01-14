using DocLink.Core.Models;

namespace DocLink.Data.Interfaces;

public interface IAppointmentRepository
{
    Task<Appointment> AddAppointmentAsync(Appointment appointment);
    Task<IEnumerable<Appointment>> GetAppointmentsByPatientIdAsync(Guid patientId);
}