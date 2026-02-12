using System;
using System.Threading.Tasks;

namespace DocLink.Services.Interfaces
{
    public interface IPatientWorker
    {
        Task<bool> CanBookAppointment(Guid patientId);
    }
}