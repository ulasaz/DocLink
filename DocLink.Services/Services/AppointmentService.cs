using DocLink.Core.Models;
using DocLink.Data.Interfaces;
using DocLink.Services.DTO_s;
using DocLink.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace DocLink.Services.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly UserManager<Account> _userManager;
    
    public AppointmentService(IAppointmentRepository appointmentRepository, UserManager<Account> userManager)
    {
        _appointmentRepository = appointmentRepository;
        _userManager = userManager;
    }
    public async Task<AppointmentResponseModel> BookAppointment(AppointmentRequestModel requestModel)
    {
        var patient = await _userManager.FindByIdAsync(requestModel.PatientId.ToString());
        var specialist = await _userManager.FindByIdAsync(requestModel.SpecialistId.ToString());
        
        if (patient == null)
            return new AppointmentResponseModel { IsSuccessful = false, Errors = new List<string> { "Patient not found" } };
        
        if (specialist == null)
            return new AppointmentResponseModel { IsSuccessful = false, Errors = new List<string> { "Specialist not found" } };

        try 
        {
            var appointment = new Appointment
            {
                Id = Guid.NewGuid(),
                PatientId = requestModel.PatientId,
                SpecialistId = requestModel.SpecialistId,
                Status = requestModel.Status,
                Time = requestModel.Time.ToUniversalTime(),
                OfferId = requestModel.OfferId
            };
            
            await _appointmentRepository.AddAppointmentAsync(appointment);
            
            return new AppointmentResponseModel
            {
                PatientFirstName = patient.FirstName,
                PatientLastName = patient.LastName,
                SpecialistFirstName = specialist.FirstName,
                SpecialistLastName = specialist.LastName,
                IsSuccessful = true,
                Time = appointment.Time
            };
        }
        catch (Exception ex)
        {
            return new AppointmentResponseModel
            {
                IsSuccessful = false,
                Errors = new List<string> { "Database error: " + ex.Message }
            };
        }
    }

    public async Task<IEnumerable<AppointmentDto>> GetAppointmentsByPatientIdAsync(Guid patientId)
    {
        var appointments = await _appointmentRepository.GetAppointmentsByPatientIdAsync(patientId);
        return appointments.Select(a => 
        {
            var firstLocation = a.Specialist?.SpecialistLocations?.FirstOrDefault();
        
            var bookedOffers = new List<string>();

            if (a.Offer != null)
            {
                bookedOffers.Add(a.Offer.Title);
            }
            
            return new AppointmentDto
            {
                
                Id = a.Id,
                Time = a.Time,
                Status = a.Status,
                SpecialistId = a.SpecialistId,
                
                SpecialistName = a.Specialist != null 
                    ? $"{a.Specialist.FirstName} {a.Specialist.LastName}" 
                    : "Unknown Doctor",

                Offers = bookedOffers,
                City = firstLocation?.Location.City ?? "Online", 
                Street = firstLocation?.Location.Street ?? "-"
            };
        }).ToList();
    }
}