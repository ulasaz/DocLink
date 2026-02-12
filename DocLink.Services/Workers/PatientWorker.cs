using DocLink.Services.Interfaces;
using System.Threading.Tasks;
using System;

namespace DocLink.Services.Workers
{
    public class PatientWorker : IPatientWorker
    {
        public Task<bool> CanBookAppointment(Guid patientId)
        {
            // Tutaj normalnie byłaby logika sprawdzająca długi, bany itp.
            // Zgodnie z diagramem zwracamy true, żeby proces przeszedł dalej.
            return Task.FromResult(true);
        }
    }
}