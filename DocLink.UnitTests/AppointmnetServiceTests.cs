using DocLink.Core.Models;
using DocLink.Data.Interfaces;
using DocLink.Services.DTO_s;
using DocLink.Services.Interfaces;
using DocLink.Services.Services;
using Microsoft.AspNetCore.Identity;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

[TestFixture]
[NUnit.Framework.Category("Service")]
public class AppointmentServiceTests
{
    private Mock<IAppointmentRepository> _repoMock;
    private Mock<IPatientWorker> _patientWorkerMock;
    private Mock<IScheduleWorker> _scheduleWorkerMock;
    private Mock<UserManager<Account>> _userManagerMock;
    private AppointmentService _service;

    [SetUp]
    public void SetUp()
    {
        // 1. Mockowanie Repozytorium
        _repoMock = new Mock<IAppointmentRepository>();

        // 2. Mockowanie Workerów
        _patientWorkerMock = new Mock<IPatientWorker>();
        _scheduleWorkerMock = new Mock<IScheduleWorker>();

        // 3. Mockowanie UserManager
        var store = new Mock<IUserStore<Account>>();
        _userManagerMock = new Mock<UserManager<Account>>(store.Object, null, null, null, null, null, null, null, null);
        
        _service = new AppointmentService(
            _repoMock.Object, 
            _patientWorkerMock.Object, 
            _scheduleWorkerMock.Object, 
            _userManagerMock.Object
        );
    }

    [Test, Order(1)]
    [DisplayName("BookAppointment - Sukces (Happy Path)")]
    public async Task BookAppointment_ShouldReturnSuccess_WhenValidationPasses()
    {
        // 1) ARRANG (JEŚLI)
        var patientId = Guid.NewGuid();
        var specialistId = Guid.NewGuid();
        var time = DateTime.Now;

        var request = new AppointmentRequestModel 
        { 
            PatientId = patientId, 
            SpecialistId = specialistId, 
            Time = time,
            OfferId = Guid.NewGuid(),
            Status = "Pending"
        };

        var patientAccount = new Account { Id = patientId, FirstName = "Jan", LastName = "Kowalski" };
        var specialistAccount = new Account { Id = specialistId, FirstName = "Dr", LastName = "House" };

        // Setup: Workery muszą zwracać TRUE, aby przejść walidację
        _patientWorkerMock.Setup(x => x.CanBookAppointment(patientId))
            .ReturnsAsync(true);
        
        _scheduleWorkerMock.Setup(x => x.IsSlotAvailable(specialistId, time))
            .ReturnsAsync(true);

        // Setup: UserManager dla mapowania nazwisk w odpowiedzi
        _userManagerMock.Setup(x => x.FindByIdAsync(patientId.ToString()))
            .ReturnsAsync(patientAccount);
        _userManagerMock.Setup(x => x.FindByIdAsync(specialistId.ToString()))
            .ReturnsAsync(specialistAccount);

        // Setup: Repozytorium
        _repoMock.Setup(x => x.AddAppointmentAsync(It.IsAny<Appointment>()))
            .ReturnsAsync((Appointment a) => a);

        // 2) ACT (GDY)
        var result = await _service.BookAppointment(request);

        // 3) ASSERT (WTEDY)
        Assert.That(result.IsSuccessful, Is.True, "Operacja powinna zakończyć się sukcesem");
        Assert.That(result.PatientFirstName, Is.EqualTo("Jan"));
        
        _patientWorkerMock.Verify(x => x.CanBookAppointment(patientId), Times.Once);
        _scheduleWorkerMock.Verify(x => x.IsSlotAvailable(specialistId, time), Times.Once);
        _repoMock.Verify(x => x.AddAppointmentAsync(It.IsAny<Appointment>()), Times.Once);
        _scheduleWorkerMock.Verify(x => x.ReserveSlot(specialistId, time), Times.Once);
    }
    
    [TestCase(false, true, "Patient cannot book appointment")]  // Pacjent zablokowany
    [TestCase(true, false, "The selected slot is not available")] // Slot zajęty
    [Order(2)]
    public async Task BookAppointment_ShouldFail_WhenValidationFails(
        bool canBook, 
        bool isSlotFree, 
        string expectedErrorFragment)
    {
        // 1) JEŚLI
        var request = new AppointmentRequestModel 
        { 
            PatientId = Guid.NewGuid(), 
            SpecialistId = Guid.NewGuid(), 
            Time = DateTime.Now 
        };
        
        _patientWorkerMock.Setup(x => x.CanBookAppointment(It.IsAny<Guid>()))
            .ReturnsAsync(canBook);

        _scheduleWorkerMock.Setup(x => x.IsSlotAvailable(It.IsAny<Guid>(), It.IsAny<DateTime>()))
            .ReturnsAsync(isSlotFree);

        // 2) GDY
        var result = await _service.BookAppointment(request);

        // 3) WTEDY
        Assert.That(result.IsSuccessful, Is.False, "Powinien zwrócić błąd walidacji");
        Assert.That(result.Errors, Is.Not.Null);
        Assert.That(result.Errors.First(), Does.Contain(expectedErrorFragment));
        
        _repoMock.Verify(x => x.AddAppointmentAsync(It.IsAny<Appointment>()), Times.Never);
        _scheduleWorkerMock.Verify(x => x.ReserveSlot(It.IsAny<Guid>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Test, Order(3)]
    [DisplayName("BookAppointment - Obsługa wyjątku z bazy danych")]
    public async Task BookAppointment_ShouldHandleDatabaseException()
    {
        // 1) JEŚLI
        var request = new AppointmentRequestModel { PatientId = Guid.NewGuid(), SpecialistId = Guid.NewGuid(), Time = DateTime.Now };

        // Workery muszą pozwolić na przejście do bloku try/catch
        _patientWorkerMock.Setup(x => x.CanBookAppointment(It.IsAny<Guid>())).ReturnsAsync(true);
        _scheduleWorkerMock.Setup(x => x.IsSlotAvailable(It.IsAny<Guid>(), It.IsAny<DateTime>())).ReturnsAsync(true);

        // Symulacja błędu bazy danych
        _repoMock.Setup(x => x.AddAppointmentAsync(It.IsAny<Appointment>()))
            .ThrowsAsync(new Exception("Critical DB Error"));

        // 2) GDY
        var result = await _service.BookAppointment(request);

        // 3) WTEDY
        Assert.That(result.IsSuccessful, Is.False);
        Assert.That(result.Errors.First(), Does.Contain("System error"));

        // Upewniamy się, że próba zapisu wystąpiła
        _repoMock.Verify(x => x.AddAppointmentAsync(It.IsAny<Appointment>()), Times.Once);

        _scheduleWorkerMock.Verify(x => x.ReserveSlot(It.IsAny<Guid>(), It.IsAny<DateTime>()), Times.Never);
    }
}