using DocLink.Core.Models;
using DocLink.Data;
using DocLink.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace DocLink.IntegrationTests.Repositories;

public class SpecialistRepositoryTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder()
        .WithImage("postgres:15-alpine") 
        .Build();

    private ApplicationContext _context;
    private SpecialistRepository _repository;

    public async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();

        var connectionString = _postgreSqlContainer.GetConnectionString();

        var options = new DbContextOptionsBuilder<ApplicationContext>()
            .UseNpgsql(connectionString)
            .Options;

        _context = new ApplicationContext(options);
        
        await _context.Database.EnsureCreatedAsync();

        _repository = new SpecialistRepository(_context);
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _postgreSqlContainer.DisposeAsync();
    }

    [Fact]
    public async Task GetSpecialistById_ShouldReturnData_FromRealPostgres()
    {
        var specialistId = Guid.NewGuid();
        var specialist = new Specialist
        {
            Id = specialistId,
            FirstName = "Gregory",
            LastName = "House",
            OffersSpecialists = new List<OfferSpecialist>
            {
                new OfferSpecialist 
                { 
                    Offer = new Offer { Title = "Diagnostics", Price = 500 } 
                }
            },
            SpecialistLocations = new List<SpecialistLocation>
            {
                new SpecialistLocation 
                { 
                    Location = new Location { City = "Princeton", Street = "Main St", Number = 221 } 
                }
            }
        };

        _context.Specialists.Add(specialist);
        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();

        var result = await _repository.GetSpecialistById(specialistId);

        Assert.NotNull(result);
        Assert.Equal("House", result.LastName);
        
        Assert.Single(result.OffersSpecialists);
        Assert.Equal("Diagnostics", result.OffersSpecialists.First().Offer.Title);
        
        Assert.Single(result.SpecialistLocations);
        Assert.Equal("Princeton", result.SpecialistLocations.First().Location.City);
    }
    [Fact]
    public async Task GetAllSpecialistsAsync_ShouldReturnAllSpecialists_WithIncludedOffers()
    {
        // Arrange
        var specialist1 = new Specialist
        {
            Id = Guid.NewGuid(),
            FirstName = "Alice",
            LastName = "Cooper",
            OffersSpecialists = new List<OfferSpecialist>
            {
                new OfferSpecialist { Offer = new Offer { Title = "Basic Checkup", Price = 100 } }
            }
        };

        var specialist2 = new Specialist
        {
            Id = Guid.NewGuid(),
            FirstName = "Bob",
            LastName = "Marley",
            OffersSpecialists = new List<OfferSpecialist>
            {
                new OfferSpecialist { Offer = new Offer { Title = "Advanced Surgery", Price = 1000 } }
            }
        };

        _context.Specialists.AddRange(specialist1, specialist2);
        await _context.SaveChangesAsync();
        
        _context.ChangeTracker.Clear();

        // Act
        var result = await _repository.GetAllSpecialistsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Count() >= 2); // >= because other tests might leave data if not cleaned (though Testcontainers is usually fresh)
        
        var alice = result.FirstOrDefault(s => s.Id == specialist1.Id);
        Assert.NotNull(alice);
        Assert.Single(alice.OffersSpecialists);
        Assert.Equal("Basic Checkup", alice.OffersSpecialists.First().Offer.Title);
    }

    [Fact]
    public async Task GetSpecialistById_ShouldReturnNull_WhenIdDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.GetSpecialistById(nonExistentId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetSpecialistById_ShouldIncludeReviews_WhenTheyExist()
    {
        // Arrange
        // 1. Сначала создаем Пациента, который будет автором отзывов
        var patient = new Patient
        {
            Id = Guid.NewGuid(),
            FirstName = "Reviewer",
            LastName = "Guy",
            Email = "reviewer@test.com",
            UserName = "reviewer"
        };

        // 2. Создаем Специалиста с отзывами, привязанными к этому Пациенту
        var specialistId = Guid.NewGuid();
        var specialist = new Specialist
        {
            Id = specialistId,
            FirstName = "John",
            LastName = "Wick",
            Reviews = new List<Review>
            {
                new Review 
                { 
                    Rating = 5, 
                    Comment = "Excellent doctor",
                    Patient = patient // <--- ВАЖНО: Привязываем отзыв к пациенту
                },
                new Review 
                { 
                    Rating = 1, 
                    Comment = "Waited too long",
                    Patient = patient // <--- ВАЖНО: Привязываем отзыв к пациенту
                }
            }
        };

        // Добавляем пациента и специалиста (отзывы добавятся автоматически через специалиста)
        _context.Add(patient); 
        _context.Specialists.Add(specialist);
        
        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();

        // Act
        var result = await _repository.GetSpecialistById(specialistId);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Reviews);
        Assert.Equal(2, result.Reviews.Count);
        Assert.Contains(result.Reviews, r => r.Comment == "Excellent doctor");
    }
}