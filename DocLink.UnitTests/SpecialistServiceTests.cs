using DocLink.Core.Models;
using DocLink.Data.Interfaces;
using DocLink.Services.DTO_s;
using DocLink.Services.Services;
using Moq;
using Xunit;

namespace DocLink.UnitTests.Services;

public class SpecialistServiceTests
{
    private readonly Mock<ISpecialistRepository> _specialistRepoMock;
    private readonly SpecialistService _specialistService;

    public SpecialistServiceTests()
    {
        _specialistRepoMock = new Mock<ISpecialistRepository>();
        
        _specialistService = new SpecialistService(
            _specialistRepoMock.Object
        );
    }
    

    [Fact]
    public async Task GetAllSpecialistsDtoAsync_ShouldReturnMappedDtos_WhenSpecialistsExist()
    {
        // Arrange
        var specialistId = Guid.NewGuid();
        var specialists = new List<Specialist>
        {
            new Specialist
            {
                Id = specialistId,
                FirstName = "John",
                LastName = "Doe",
                OffersSpecialists = new List<OfferSpecialist>
                {
                    new OfferSpecialist { Offer = new Offer { Title = "Cardiology" } },
                    new OfferSpecialist { Offer = new Offer { Title = "Surgery" } }
                }
            }
        };

        _specialistRepoMock.Setup(r => r.GetAllSpecialistsAsync())
            .ReturnsAsync(specialists);

        // Act
        var result = await _specialistService.GetAllSpecialistsDtoAsync();

        // Assert
        var dto = result.FirstOrDefault();
        Assert.NotNull(dto);
        Assert.Equal(specialistId, dto.Id);
        Assert.Equal("John", dto.FirstName);
        Assert.Equal("Doe", dto.LastName);
        
        Assert.Contains("Cardiology", dto.Specialization);
        Assert.Contains("Surgery", dto.Specialization);
        Assert.Contains(", ", dto.Specialization);
    }

    [Fact]
    public async Task GetAllSpecialistsDtoAsync_ShouldReturnEmptyList_WhenNoSpecialists()
    {
        // Arrange
        _specialistRepoMock.Setup(r => r.GetAllSpecialistsAsync())
            .ReturnsAsync(new List<Specialist>());

        // Act
        var result = await _specialistService.GetAllSpecialistsDtoAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllSpecialistsDtoAsync_ShouldHandleSpecialistsWithoutOffers()
    {
        // Arrange
        var specialist = new Specialist
        {
            Id = Guid.NewGuid(),
            FirstName = "Jane",
            LastName = "Doe",
            OffersSpecialists = new List<OfferSpecialist>() // Пустой список услуг
        };

        _specialistRepoMock.Setup(r => r.GetAllSpecialistsAsync())
            .ReturnsAsync(new List<Specialist> { specialist });

        // Act
        var result = await _specialistService.GetAllSpecialistsDtoAsync();

        // Assert
        var dto = result.First();
        Assert.Equal("", dto.Specialization);
    }


    [Fact]
    public async Task GetAllDataAboutSpecialist_ShouldReturnDetailsDto_WhenFound()
    {
        var specialistId = Guid.NewGuid();
        var specialist = new Specialist
        {
            Id = specialistId,
            FirstName = "Dr.",
            LastName = "House",
            OffersSpecialists = new List<OfferSpecialist>
            {
                new OfferSpecialist 
                { 
                    Offer = new Offer { Id = Guid.NewGuid(), Title = "Diagnosis", Price = 100 } 
                }
            },
            SpecialistLocations = new List<SpecialistLocation>
            {
                new SpecialistLocation 
                { 
                    Location = new Location { City = "Princeton", Street = "Main St", Number = 1 } 
                }
            },
            Reviews = new List<Review>
            {
                new Review { Rating = 5, Comment = "Great doctor" }
            }
        };

        _specialistRepoMock.Setup(r => r.GetSpecialistById(specialistId))
            .ReturnsAsync(specialist);

        // Act
        var result = await _specialistService.GetAllDataAboutSpecialist(specialistId);
        
        Assert.Equal(specialistId, result.Id);
        Assert.Equal("Dr.", result.FirstName);
        Assert.Equal("House", result.LastName);
        
        Assert.Single(result.Offers);
        Assert.Equal("Diagnosis", result.Offers.First().Title);
        Assert.Equal(100, result.Offers.First().Price);
        
        Assert.Single(result.SpecialistLocations);
        Assert.Equal("Princeton", result.SpecialistLocations.First().City);
        Assert.Equal("Main St 1", result.SpecialistLocations.First().Address);
        
        Assert.Single(result.Reviews);
        Assert.Equal(5, result.Reviews.First().Rate);
        Assert.Equal("Great doctor", result.Reviews.First().Content);
    }

    [Fact]
    public async Task GetAllDataAboutSpecialist_ShouldThrowException_WhenSpecialistNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _specialistRepoMock.Setup(r => r.GetSpecialistById(id))
            .ReturnsAsync((Specialist?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => 
            _specialistService.GetAllDataAboutSpecialist(id));
        
        Assert.Equal("Specialist not found", exception.Message);
    }

    [Fact]
    public async Task GetAllDataAboutSpecialist_ShouldHandleEmptyRelations()
    {
        // Arrange
        var specialistId = Guid.NewGuid();
        var specialist = new Specialist
        {
            Id = specialistId,
            FirstName = "New",
            LastName = "Doctor",
            OffersSpecialists = new List<OfferSpecialist>(),
            SpecialistLocations = new List<SpecialistLocation>(),
            Reviews = new List<Review>()
        };

        _specialistRepoMock.Setup(r => r.GetSpecialistById(specialistId))
            .ReturnsAsync(specialist);

        // Act
        var result = await _specialistService.GetAllDataAboutSpecialist(specialistId);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Offers);
        Assert.Empty(result.SpecialistLocations);
        Assert.Empty(result.Reviews);
    }
}