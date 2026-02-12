using DocLink.Core.Factories;
using DocLink.Core.Models;
using DocLink.Data;
using DocLink.Services.DTO_s;
using DocLink.Services.Interfaces;
using DocLink.Services.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Testcontainers.PostgreSql;
using Xunit;

namespace DocLink.IntegrationTests.Services;

public class AccountServiceIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder()
        .WithImage("postgres:15-alpine")
        .Build();

    private ApplicationContext _context;
    private UserManager<Account> _userManager;
    private AccountService _accountService;

    public async Task InitializeAsync()
    {
        // 1. Запускаем контейнер
        await _postgreSqlContainer.StartAsync();

        // 2. Подключаем контекст к БД в контейнере
        var options = new DbContextOptionsBuilder<ApplicationContext>()
            .UseNpgsql(_postgreSqlContainer.GetConnectionString())
            .Options;

        _context = new ApplicationContext(options);
        await _context.Database.EnsureCreatedAsync();

        // 3. !!! ВАЖНО !!! Добавляем роли вручную, так как база пустая
        // Без этого UserManager упадет с ошибкой "Role PATIENT does not exist"
        _context.Roles.AddRange(
            new IdentityRole<Guid> 
            { 
                Id = Guid.NewGuid(), 
                Name = "Patient", 
                NormalizedName = "PATIENT" // Identity ищет именно по NormalizedName (обычно UPPERCASE)
            },
            new IdentityRole<Guid> 
            { 
                Id = Guid.NewGuid(), 
                Name = "Specialist", 
                NormalizedName = "SPECIALIST" 
            }
        );
        await _context.SaveChangesAsync();

        // 4. Настраиваем UserStore с явным указанием типов
        // <ТипЮзера, ТипРоли, ТипКонтекста, ТипКлюча(Guid)>
        var userStore = new UserStore<Account, IdentityRole<Guid>, ApplicationContext, Guid>(_context);

        // 5. Опции Identity (упрощаем пароли для тестов)
        var optionsIdentity = Options.Create(new IdentityOptions
        {
            User = { RequireUniqueEmail = true },
            Password = { RequireDigit = false, RequiredLength = 3, RequireNonAlphanumeric = false, RequireUppercase = false }
        });

        // 6. Мокаем ServiceProvider (он нужен UserManager-у, но мы его не используем напрямую)
        var serviceProviderMock = new Mock<IServiceProvider>().Object;

        // 7. Собираем настоящий UserManager
        _userManager = new UserManager<Account>(
            userStore, 
            optionsIdentity, 
            new PasswordHasher<Account>(), 
            new List<IUserValidator<Account>> { new UserValidator<Account>() },
            new List<IPasswordValidator<Account>> { new PasswordValidator<Account>() },
            new UpperInvariantLookupNormalizer(), 
            new IdentityErrorDescriber(), 
            serviceProviderMock,
            new NullLogger<UserManager<Account>>()
        );

        // 8. Мокаем остальные зависимости сервиса
        var tokenServiceMock = new Mock<ITokenService>();
        var factoryProviderMock = new Mock<IAccountFactoryProvider>();

        // Настраиваем фабрику, чтобы она создавала объект Patient
        var patientFactoryMock = new Mock<AccountFactory>();
        patientFactoryMock.Setup(f => f.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns((string f, string l, string e) => new Patient { FirstName = f, LastName = l, Email = e, UserName = e });

        factoryProviderMock.Setup(f => f.GetFactory("Patient")).Returns(patientFactoryMock.Object);

        // 9. Инициализируем тестируемый сервис
        _accountService = new AccountService(
            _userManager,
            null!, // SignInManager не нужен для тестов регистрации
            factoryProviderMock.Object,
            tokenServiceMock.Object
        );
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _postgreSqlContainer.DisposeAsync();
    }

    [Fact]
    public async Task RegisterAsync_ShouldCreateUserInDatabase_WithHashedPassword()
    {
        // Arrange
        var request = new RegistrationRequestModel
        {
            Email = "newuser@test.com",
            Password = "SecurePassword123!",
            FirstName = "Ivan",
            LastName = "Ivanov",
            Role = "Patient" // Роль должна совпадать с той, что мы добавили в InitializeAsync
        };

        // Act
        var result = await _accountService.RegisterAsync(request);

        // Assert
        Assert.True(result.IsSuccessful);

        // Проверяем напрямую в базе данных
        _context.ChangeTracker.Clear(); // Сбрасываем кэш EF, чтобы сделать честный SELECT
        var userInDb = await _context.Set<Patient>().FirstOrDefaultAsync(u => u.Email == request.Email);

        Assert.NotNull(userInDb);
        Assert.Equal("Ivan", userInDb.FirstName);
        
        // Пароль в базе НЕ должен совпадать с исходным (он должен быть захеширован)
        Assert.NotEqual(request.Password, userInDb.PasswordHash);
        
        // Проверяем, что UserManager может валидировать этот хеш
        var checkPassword = await _userManager.CheckPasswordAsync(userInDb, request.Password);
        Assert.True(checkPassword);
    }
}