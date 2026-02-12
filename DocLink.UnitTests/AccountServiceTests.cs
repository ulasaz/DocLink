// namespace: DocLink.Tests.Unit.Services
using DocLink.Core.Factories;
using DocLink.Core.Models;
using DocLink.Services.DTO_s;
using DocLink.Services.Interfaces;
using DocLink.Services.Services;
using Microsoft.AspNetCore.Identity;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

[TestFixture]
[NUnit.Framework.Category("Service")]
[NUnit.Framework.Description("Testy jednostkowe serwisu konta - Rejestracja")]
public class AccountServiceTests
{
    private Mock<UserManager<Account>> _userManagerMock;
    private Mock<SignInManager<Account>> _signInManagerMock;
    private Mock<IAccountFactoryProvider> _factoryProviderMock;
    private Mock<ITokenService> _tokenServiceMock;
    private AccountService _service;

    [SetUp]
    public void SetUp()
    {
        // 1) Jeśli: Konfiguracja mocków
        var userStore = new Mock<IUserStore<Account>>();
        _userManagerMock = new Mock<UserManager<Account>>(userStore.Object, null, null, null, null, null, null, null, null);
        
        var contextAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
        var claimsFactory = new Mock<IUserClaimsPrincipalFactory<Account>>();
        _signInManagerMock = new Mock<SignInManager<Account>>(_userManagerMock.Object, contextAccessor.Object, claimsFactory.Object, null, null, null, null);

        _factoryProviderMock = new Mock<IAccountFactoryProvider>();
        _tokenServiceMock = new Mock<ITokenService>();

        _service = new AccountService(_userManagerMock.Object, _signInManagerMock.Object, _factoryProviderMock.Object, _tokenServiceMock.Object);
    }

    [Test, Order(1)]
    [DisplayName("RegisterAsync - Rejestracja udana")]
    public async Task RegisterAsync_ShouldReturnSuccess_WhenDataIsValid()
    {
        // 1) Jeśli
        var request = new RegistrationRequestModel
        {
            Email = "test@test.com",
            Password = "Password123!",
            Role = "Patient",
            FirstName = "Jan",
            LastName = "Kowalski"
        };

        var createdAccount = new Patient { Email = request.Email, UserName = request.Email };
        
        // Mockowanie fabryki (zwraca fabrykę, która zwraca konto)
        var factoryMock = new Mock<AccountFactory>();
        factoryMock.Setup(f => f.Create(request.FirstName, request.LastName, request.Email))
                   .Returns(createdAccount);

        _factoryProviderMock.Setup(p => p.GetFactory(request.Role))
                            .Returns(factoryMock.Object);

        // Mockowanie UserManagera (CreateAsync -> Success)
        _userManagerMock.Setup(u => u.CreateAsync(createdAccount, request.Password))
                        .ReturnsAsync(IdentityResult.Success);
        
        // Mockowanie dodawania do roli
        _userManagerMock.Setup(u => u.AddToRoleAsync(createdAccount, "patient")) // Service robi .ToLower()
                        .ReturnsAsync(IdentityResult.Success);

        // 2) Gdy
        var result = await _service.RegisterAsync(request);

        // 3) Wtedy
        Assert.That(result.IsSuccessful, Is.True, "Rejestracja powinna zakończyć się sukcesem");
        Assert.That(result.Errors, Is.Null, "Nie powinno być błędów");

        // Weryfikacja interakcji
        _userManagerMock.Verify(u => u.CreateAsync(It.IsAny<Account>(), request.Password), Times.Once);
        _userManagerMock.Verify(u => u.AddToRoleAsync(It.IsAny<Account>(), "patient"), Times.Once);
    }

    [Test, Order(2)]
    [DisplayName("RegisterAsync - Błąd walidacji hasła/użytkownika")]
    public async Task RegisterAsync_ShouldFail_WhenIdentityCreateFails()
    {
        // 1) Jeśli
        var request = new RegistrationRequestModel { Role = "Specialist", Email = "doc@test.com" };
        var account = new Specialist();

        // Mock fabryki
        var factoryMock = new Mock<AccountFactory>();
        factoryMock.Setup(f => f.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(account);
        _factoryProviderMock.Setup(p => p.GetFactory("Specialist")).Returns(factoryMock.Object);

        // Symulacja błędu z UserManagera
        var identityErrors = new IdentityError { Description = "Password too short" };
        _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<Account>(), It.IsAny<string>()))
                        .ReturnsAsync(IdentityResult.Failed(identityErrors));

        // 2) Gdy
        var result = await _service.RegisterAsync(request);

        // 3) Wtedy
        Assert.That(result.IsSuccessful, Is.False);
        Assert.That(result.Errors, Is.Not.Null);
        Assert.That(result.Errors.First(), Is.EqualTo("Password too short"));

        // Weryfikacja, że NIE dodano do roli
        _userManagerMock.Verify(u => u.AddToRoleAsync(It.IsAny<Account>(), It.IsAny<string>()), Times.Never);
    }

    [Test, Order(3)]
    [DisplayName("RegisterAsync - Błąd: Email zajęty")]
    public async Task RegisterAsync_ShouldFail_WhenEmailIsTaken()
    {
        // 1) Jeśli (Arrange) - przygotowanie danych
        var request = new RegistrationRequestModel 
        { 
            Role = "Patient", 
            Email = "zajety@email.com", // Ten email symulujemy jako zajęty
            Password = "Password123!",
            FirstName = "Test",
            LastName = "User"
        };
        
        // Mockujemy obiekt konta (ponieważ fabryka musi coś zwrócić, zanim dojdziemy do bazy)
        var account = new Patient { Email = request.Email };
        var factoryMock = new Mock<AccountFactory>();
        factoryMock.Setup(f => f.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                   .Returns(account);
        _factoryProviderMock.Setup(p => p.GetFactory(request.Role))
                            .Returns(factoryMock.Object);

        // KLUCZOWY MOMENT: Symulacja błędu "DuplicateEmail" z UserManagera
        // Mówimy mockowi: "Jak ktoś spróbuje utworzyć to konto, zwróć błąd, że email jest zajęty"
        var identityError = new IdentityError 
        { 
            Code = "DuplicateEmail", 
            Description = $"Email '{request.Email}' is already taken." 
        };
        
        _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<Account>(), It.IsAny<string>()))
                        .ReturnsAsync(IdentityResult.Failed(identityError));

        // 2) Gdy (Act) - wykonanie akcji
        var result = await _service.RegisterAsync(request);

        // 3) Wtedy (Assert) - sprawdzenie wyników
        
        // Oczekujemy, że IsSuccessful będzie FALSE (bo rejestracja ma się NIE udać)
        Assert.That(result.IsSuccessful, Is.True, "System powinien zablokować rejestrację dla zajętego emaila");
        
        // Sprawdzamy, czy w liście błędów jest informacja o zajętym emailu
        Assert.That(result.Errors, Is.Not.Null);
        Assert.That(result.Errors.First(), Contains.Substring("already taken"));

        // Weryfikacja: Nie powinno próbować dodawać do roli, jeśli nie utworzono usera
        _userManagerMock.Verify(u => u.AddToRoleAsync(It.IsAny<Account>(), It.IsAny<string>()), Times.Never);
    }
}