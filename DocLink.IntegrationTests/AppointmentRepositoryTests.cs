using DocLink.Core.Models;
using DocLink.Data;
using DocLink.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace DocLink.IntegrationTests.Repositories;

public class AppointmentRepositoryTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder()
        .WithImage("postgres:15-alpine")
        .Build();

    private ApplicationContext _context;
    private AppointmentRepository _repository;

    public async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();

        var connectionString = _postgreSqlContainer.GetConnectionString();

        var options = new DbContextOptionsBuilder<ApplicationContext>()
            .UseNpgsql(connectionString)
            .Options;

        _context = new ApplicationContext(options);
        await _context.Database.EnsureCreatedAsync();

        _repository = new AppointmentRepository(_context);
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _postgreSqlContainer.DisposeAsync();
    }

    [Fact]
    public async Task AddAppointmentAsync_ShouldSaveToDatabase_WhenEntitiesExist()
    {
        // ИСПРАВЛЕНИЕ: Используем Patient вместо Account
        var patient = new Patient 
        { 
            Id = Guid.NewGuid(), 
            FirstName = "John", 
            LastName = "Doe", 
            Email = "patient@test.com", 
            UserName = "patient" 
        };
        
        var specialist = new Specialist 
        { 
            Id = Guid.NewGuid(), 
            FirstName = "Dr.", 
            LastName = "House",
            Email = "house@test.com",
            UserName = "house"
        };


        _context.Add(patient);
        _context.Add(specialist);
        await _context.SaveChangesAsync();

        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            PatientId = patient.Id,
            SpecialistId = specialist.Id,
            Time = DateTime.UtcNow.AddDays(1),
            Status = "Booked"
        };

        var result = await _repository.AddAppointmentAsync(appointment);

        _context.ChangeTracker.Clear();

        var savedAppointment = await _context.Set<Appointment>().FindAsync(appointment.Id);

        Assert.NotNull(savedAppointment);
        Assert.Equal(appointment.Id, result.Id);
        Assert.Equal("Booked", savedAppointment.Status);
        Assert.Equal(patient.Id, savedAppointment.PatientId);
    }

    [Fact]
    public async Task GetAppointmentsByPatientIdAsync_ShouldReturnAppointments_WithDeepIncludes()
    {
        // ИСПРАВЛЕНИЕ: Используем Patient вместо Account
        var patient = new Patient 
        { 
            Id = Guid.NewGuid(), 
            FirstName = "Jane", 
            LastName = "Doe", 
            Email = "jane@test.com", 
            UserName = "jane" 
        };
        
        var location = new Location { Id = Guid.NewGuid(), City = "Chicago", Street = "Michigan Ave", Number = 100 };
        
        var specialist = new Specialist 
        { 
            Id = Guid.NewGuid(), 
            FirstName = "Gregory", 
            LastName = "House",
            Email = "greg@test.com",
            UserName = "greg",
            SpecialistLocations = new List<SpecialistLocation>
            {
                new SpecialistLocation { Location = location }
            }
        };

        _context.Add(patient);
        _context.Add(specialist);
        await _context.SaveChangesAsync();

        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            PatientId = patient.Id, // Теперь этот ID ссылается на реального Пациента
            SpecialistId = specialist.Id,
            Time = DateTime.UtcNow,
            Status = "Confirmed"
        };

        _context.Add(appointment);
        await _context.SaveChangesAsync();
        
        _context.ChangeTracker.Clear();

        var result = await _repository.GetAppointmentsByPatientIdAsync(patient.Id);

        Assert.NotEmpty(result);
        var appt = result.First();

        Assert.NotNull(appt.Specialist);
        Assert.Equal("House", appt.Specialist.LastName);

        Assert.NotEmpty(appt.Specialist.SpecialistLocations);
        Assert.Equal("Chicago", appt.Specialist.SpecialistLocations.First().Location.City);
    }

    [Fact]
    public async Task GetAppointmentsByPatientIdAsync_ShouldReturnEmpty_WhenNoAppointmentsExist()
    {
        // ИСПРАВЛЕНИЕ: Используем Patient вместо Account
        var patient = new Patient 
        { 
            Id = Guid.NewGuid(), 
            FirstName = "Empty", 
            LastName = "User", 
            Email = "empty@test.com", 
            UserName = "empty" 
        };
        _context.Add(patient);
        await _context.SaveChangesAsync();

        var result = await _repository.GetAppointmentsByPatientIdAsync(patient.Id);

        Assert.Empty(result);

        var house = new HouseBuilder()
            .WithFloor("floor1")
            .WithBath("bath2")
            .WithDoor("door1")
            .WithWalls("walllls")
            .Build();
    }
}

public class House
{
    public string Door { get; set; }
    public string Wall { get; set; }
    public string Floor { get; set; }
    public string Bath { get; set; }
}

public class HouseBuilder()
{
    private House _house = new House();
    
    public HouseBuilder WithDoor(string door)
    {
        _house.Door = door;
        
        return this;
    }
    
    public HouseBuilder WithFloor(string door)
    {
        _house.Floor = door;
        
        return this;
    }
    
    public HouseBuilder WithWalls(string door)
    {
        _house.Wall = door;
        
        return this;
    }
    
    public HouseBuilder WithBath(string door)
    {
        _house.Bath = door;
        
        return this;
    }

    public House Build()
    {
        return _house;
    }
}