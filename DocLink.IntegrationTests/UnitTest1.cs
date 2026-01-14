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
}