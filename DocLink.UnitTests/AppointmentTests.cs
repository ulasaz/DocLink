using System.ComponentModel;
using DocLink.Core.Models;
using NUnit.Framework;

[TestFixture]
[NUnit.Framework.Category("Entity")]
[TestFixture(Description = "Testy jednostkowe klasy Appointment (Warstwa Encji)")]
public class AppointmentTests
{
    private Appointment _appointment;

    // Przygotowanie danych
    [SetUp]
    public void SetUp()
    {
        _appointment = new Appointment();
    }

    // Sprzątanie po teście
    [TearDown]
    public void TearDown()
    {
        _appointment = null;
    }

    [Test, Order(1)]
    [DisplayName("Utworzenie wizyty - sprawdzenie właściwości")]
    public void CreateAppointment_ShouldSetPropertiesCorrectly()
    {
        // 1) Jeśli
        var id = Guid.NewGuid();
        var time = DateTime.Now;
        const string status = "Scheduled";

        // 2) Gdy
        _appointment.Id = id;
        _appointment.Time = time;
        _appointment.Status = status;

        // 3) Wtedy
        Assert.That(_appointment, Is.Not.Null, "Obiekt nie powinien być nullem");
        Assert.That(_appointment.Id, Is.EqualTo(id), "ID powinno być zgodne");
        Assert.That(_appointment.Status, Is.SameAs(status), "Referencja stringa statusu powinna być ta sama");
    }
    
    [TestCase("Confirmed")]
    [TestCase("Cancelled")]
    [TestCase("Pending")]
    [Order(2)]
    [DisplayName("Zmiana statusu wizyty")]
    public void SetStatus_ShouldUpdateStatus(string newStatus)
    {
        // 1) Jeśli
        _appointment.Status = "Initial";

        // 2) Gdy
        _appointment.Status = newStatus;

        // 3) Wtedy
        Assert.That(_appointment.Status, Is.EqualTo(newStatus));
    }
}