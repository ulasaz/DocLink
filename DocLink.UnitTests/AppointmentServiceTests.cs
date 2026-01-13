using DocLink.Core.Models;
using DocLink.Data.Interfaces;
using DocLink.Services.DTO_s;
using DocLink.Services.Services;
using DocLink.Services.Interfaces;
using DocLink.UnitTests.Utils;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;

namespace DocLink.UnitTests.Services;

public class AppointmentServiceTests
{
    private readonly Mock<IAppointmentRepository> _appointmentRepoMock;
    private readonly Mock<UserManager<Account>> _userManagerMock;
    private readonly AppointmentService _appointmentService;

    public AppointmentServiceTests()
    {
        _appointmentRepoMock = new Mock<IAppointmentRepository>();
        _userManagerMock = UserManagerMock.Create<Account>();

        _appointmentService = new AppointmentService(
            _appointmentRepoMock.Object,
            _userManagerMock.Object
        );
    }

    [Fact]
    public async Task BookAppointment_ShouldReturnSuccess_WhenPatientAndSpecialistExist()
    {
        var patientId = Guid.NewGuid();
        var specialistId = Guid.NewGuid();

        var patient = new Account { Id = patientId, FirstName = "John", LastName = "Doe" };
        var specialist = new Account { Id = specialistId, FirstName = "Dr.", LastName = "Smith" };

        _userManagerMock.Setup(u => u.FindByIdAsync(patientId.ToString())).ReturnsAsync(patient);
        _userManagerMock.Setup(u => u.FindByIdAsync(specialistId.ToString())).ReturnsAsync(specialist);

        _appointmentRepoMock.Setup(r => r.AddAppointmentAsync(It.IsAny<Appointment>()))
            .ReturnsAsync((Appointment a) => a);

        var request = new AppointmentRequestModel
        {
            PatientId = patientId,
            SpecialistId = specialistId,
            Status = "Booked",
            Time = DateTime.UtcNow.AddDays(1)
        };

        var result = await _appointmentService.BookAppointment(request);

        Assert.True(result.IsSuccessful);
        Assert.Equal("John", result.PatientFirstName);
        Assert.Equal("Dr.", result.SpecialistFirstName);
    }

    [Fact]
    public async Task BookAppointment_ShouldFail_WhenPatientNotFound()
    {
        var request = new AppointmentRequestModel
        {
            PatientId = Guid.NewGuid(),
            SpecialistId = Guid.NewGuid(),
            Status = "Booked",
            Time = DateTime.UtcNow
        };

        _userManagerMock.Setup(u => u.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((Account)null!);

        var result = await _appointmentService.BookAppointment(request);

        Assert.False(result.IsSuccessful);
        Assert.Contains("Patient not found", result.Errors);
    }

    [Fact]
    public async Task BookAppointment_ShouldFail_WhenSpecialistNotFound()
    {
        var patient = new Account { Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe" };
        var specialistId = Guid.NewGuid();

        _userManagerMock.Setup(u => u.FindByIdAsync(patient.Id.ToString())).ReturnsAsync(patient);
        _userManagerMock.Setup(u => u.FindByIdAsync(specialistId.ToString())).ReturnsAsync((Account)null!);

        var request = new AppointmentRequestModel
        {
            PatientId = patient.Id,
            SpecialistId = specialistId,
            Status = "Booked",
            Time = DateTime.UtcNow
        };

        var result = await _appointmentService.BookAppointment(request);

        Assert.False(result.IsSuccessful);
        Assert.Contains("Specialist not found", result.Errors);
    }

    [Fact]
    public async Task BookAppointment_ShouldFail_WhenRepositoryThrows()
    {
        var patient = new Account { Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe" };
        var specialist = new Account { Id = Guid.NewGuid(), FirstName = "Dr.", LastName = "Smith" };

        _userManagerMock.Setup(u => u.FindByIdAsync(patient.Id.ToString())).ReturnsAsync(patient);
        _userManagerMock.Setup(u => u.FindByIdAsync(specialist.Id.ToString())).ReturnsAsync(specialist);

        _appointmentRepoMock.Setup(r => r.AddAppointmentAsync(It.IsAny<Appointment>()))
            .ThrowsAsync(new Exception("DB failure"));

        var request = new AppointmentRequestModel
        {
            PatientId = patient.Id,
            SpecialistId = specialist.Id,
            Status = "Booked",
            Time = DateTime.UtcNow
        };

        var result = await _appointmentService.BookAppointment(request);

        Assert.False(result.IsSuccessful);
        Assert.Contains("Database error: DB failure", result.Errors);
    }
    

    [Fact]
    public async Task GetAppointmentsByPatientId_ShouldReturnMappedDto()
    {
        var patientId = Guid.NewGuid();
        var specialist = new Specialist
        {
            Id = Guid.NewGuid(),
            FirstName = "Dr.",
            LastName = "Smith",
            SpecialistLocations = new List<SpecialistLocation>
            {
                new SpecialistLocation { Location = new Location { City = "NYC", Street = "Main St" } }
            }
        };

        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            PatientId = patientId,
            SpecialistId = specialist.Id,
            Specialist = specialist,
            Time = DateTime.UtcNow,
            Status = "Booked"
        };

        _appointmentRepoMock.Setup(r => r.GetAppointmentsByPatientIdAsync(patientId))
            .ReturnsAsync(new List<Appointment> { appointment });

        var result = await _appointmentService.GetAppointmentsByPatientIdAsync(patientId);

        var dto = result.First();
        Assert.Equal("Dr. Smith", dto.SpecialistName);
        Assert.Equal("NYC", dto.City);
        Assert.Equal("Main St", dto.Street);
    }

    [Fact]
    public async Task GetAppointmentsByPatientId_ShouldReturnEmptyList_WhenNoAppointments()
    {
        var patientId = Guid.NewGuid();

        _appointmentRepoMock.Setup(r => r.GetAppointmentsByPatientIdAsync(patientId))
            .ReturnsAsync(new List<Appointment>());

        var result = await _appointmentService.GetAppointmentsByPatientIdAsync(patientId);

        Assert.Empty(result);
    }
}
