using DocLink.Core.Models;
using DocLink.Data.Interfaces;
using DocLink.Services.DTO_s;
using DocLink.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DocLink.Services.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IPatientWorker _patientWorker;
    private readonly IScheduleWorker _scheduleWorker;
    private readonly UserManager<Account> _userManager;

    public AppointmentService(
        IAppointmentRepository appointmentRepository,
        IPatientWorker patientWorker,
        IScheduleWorker scheduleWorker,
        UserManager<Account> userManager)
    {
        _appointmentRepository = appointmentRepository;
        _patientWorker = patientWorker;
        _scheduleWorker = scheduleWorker;
        _userManager = userManager;
    }

    // BookAppointment
    public async Task<AppointmentResponseModel> BookAppointment(AppointmentRequestModel requestModel)
    {
        // 1.1: Validate (Logika walidacji)

        // 1.1.1: CanBookAppointment
        bool canBook = await _patientWorker.CanBookAppointment(requestModel.PatientId);
        if (!canBook)
        {
            return new AppointmentResponseModel 
            { 
                IsSuccessful = false, 
                Errors = new List<string> { "Patient cannot book appointment (e.g. limit reached or debt)." } 
            };
        }

        // 1.1.3: IsSlotAvailable
        bool isSlotFree = await _scheduleWorker.IsSlotAvailable(requestModel.SpecialistId, requestModel.Time);
        if (!isSlotFree)
        {
            return new AppointmentResponseModel 
            { 
                IsSuccessful = false, 
                Errors = new List<string> { "The selected slot is not available." } 
            };
        }
        
        try
        {
            // <<create>> Appointment
            var appointment = new Appointment
            {
                Id = Guid.NewGuid(),
                PatientId = requestModel.PatientId,
                SpecialistId = requestModel.SpecialistId,
                Status = requestModel.Status,
                Time = requestModel.Time.ToUniversalTime(),
                OfferId = requestModel.OfferId
            };

            // addAppointmentAsync
            await _appointmentRepository.AddAppointmentAsync(appointment);

            // ReserveSlot (Kluczowy brakujący element w poprzednim kodzie)
            await _scheduleWorker.ReserveSlot(requestModel.SpecialistId, requestModel.Time);

            // Pobranie danych do odpowiedzi
            var patient = await _userManager.FindByIdAsync(requestModel.PatientId.ToString());
            var specialist = await _userManager.FindByIdAsync(requestModel.SpecialistId.ToString());
            
            return new AppointmentResponseModel
            {
                IsSuccessful = true,
                Time = appointment.Time,
                PatientFirstName = patient?.FirstName ?? "Unknown",
                PatientLastName = patient?.LastName ?? "Unknown",
                SpecialistFirstName = specialist?.FirstName ?? "Unknown",
                SpecialistLastName = specialist?.LastName ?? "Unknown"
            };
        }
        catch (Exception ex)
        {
            return new AppointmentResponseModel
            {
                IsSuccessful = false,
                Errors = new List<string> { "System error: " + ex.Message }
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