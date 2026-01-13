using DocLink.Core.Models;
using DocLink.Services.DTO_s;

namespace DocLink.Services.Interfaces;

public interface IAppointmentService
{
    Task<AppointmentResponseModel> BookAppointment(AppointmentRequestModel appointmentRequestModel);
    Task<IEnumerable<AppointmentDto>> GetAppointmentsByPatientIdAsync(Guid patientId);
}