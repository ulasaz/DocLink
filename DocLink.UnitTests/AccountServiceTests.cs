using DocLink.Core.Factories;
using DocLink.Core.Models;
using DocLink.Services.DTO_s;
using DocLink.Services.Interfaces;
using DocLink.Services.Services;
using DocLink.UnitTests.Utils;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;

namespace DocLink.UnitTests.Services;

public class AccountServiceTests
{
    private readonly Mock<UserManager<Account>> _userManagerMock;
    private readonly Mock<IAccountFactoryProvider> _factoryProviderMock;
    private readonly Mock<ITokenService> _tokenServiceMock;

    private readonly AccountService _accountService;

    public AccountServiceTests()
    {
        _userManagerMock = UserManagerMock.Create<Account>();
        _factoryProviderMock = new Mock<IAccountFactoryProvider>();
        _tokenServiceMock = new Mock<ITokenService>();

        _accountService = new AccountService(
            _userManagerMock.Object,
            null!,
            _factoryProviderMock.Object,
            _tokenServiceMock.Object
        );
    }
    

    [Fact]
    public async Task LoginAsync_ShouldFail_WhenUserNotFound()
    {
        _userManagerMock
            .Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((Account)null!);

        var result = await _accountService.LoginAsync(new LoginRequestModel
        {
            Email = "test@test.com",
            Password = "123"
        });

        Assert.False(result.IsSuccessful);
        Assert.Contains("Invalid login attempt", result.Errors);
    }

    [Fact]
    public async Task LoginAsync_ShouldFail_WhenPasswordIsInvalid()
    {
        var user = new Account { Email = "test@test.com" };

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(user.Email))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(x => x.CheckPasswordAsync(user, It.IsAny<string>()))
            .ReturnsAsync(false);

        var result = await _accountService.LoginAsync(new LoginRequestModel
        {
            Email = user.Email,
            Password = "wrong"
        });

        Assert.False(result.IsSuccessful);
        Assert.Contains("Invalid login attempt", result.Errors);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnToken_WhenCredentialsAreValid()
    {
        var user = new Account { Email = "test@test.com" };

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(user.Email))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(x => x.CheckPasswordAsync(user, It.IsAny<string>()))
            .ReturnsAsync(true);

        _tokenServiceMock
            .Setup(x => x.GenerateJwtToken(user))
            .ReturnsAsync("jwt-token");

        var result = await _accountService.LoginAsync(new LoginRequestModel
        {
            Email = user.Email,
            Password = "Password123!"
        });

        Assert.True(result.IsSuccessful);
        Assert.Equal("jwt-token", result.Token);
    }
    

    [Fact]
    public async Task RegisterAsync_ShouldSucceed_WhenUserCreated()
    {
        var request = new RegistrationRequestModel
        {
            Email = "test@test.com",
            Password = "Password123!",
            FirstName = "John",
            LastName = "Doe",
            Role = "Patient"
        };

        var account = new Account { Email = request.Email };

        var factoryMock = new Mock<AccountFactory>();
        factoryMock
            .Setup(x => x.Create(
                request.FirstName,
                request.LastName,
                request.Email))
            .Returns(account);

        _factoryProviderMock
            .Setup(x => x.GetFactory(request.Role))
            .Returns(factoryMock.Object);

        _userManagerMock
            .Setup(x => x.CreateAsync(account, request.Password))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock
            .Setup(x => x.AddToRoleAsync(account, request.Role.ToLower()))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _accountService.RegisterAsync(request);

        Assert.True(result.IsSuccessful);
    }

[Fact]
public async Task RegisterAsync_ShouldThrowException_WhenRoleIsUnknown()
{
    var request = new RegistrationRequestModel
    {
        Email = "test@test.com",
        Password = "Password123!",
        FirstName = "John",
        LastName = "Doe",
        Role = "UnknownRole"
    };

    _factoryProviderMock
        .Setup(x => x.GetFactory(request.Role))
        .Throws(new ArgumentException("Unknown account type"));

    await Assert.ThrowsAsync<ArgumentException>(() =>
        _accountService.RegisterAsync(request));
}

[Fact]
public async Task RegisterAsync_ShouldFail_WhenCreateAsyncReturnsFailure()
{
    var request = new RegistrationRequestModel
    {
        Email = "test@test.com",
        Password = "Password123!",
        FirstName = "John",
        LastName = "Doe",
        Role = "Patient"
    };

    var account = new Account { Email = request.Email };

    var factoryMock = new Mock<AccountFactory>();
    factoryMock
        .Setup(x => x.Create(
            request.FirstName,
            request.LastName,
            request.Email))
        .Returns(account);

    _factoryProviderMock
        .Setup(x => x.GetFactory(request.Role))
        .Returns(factoryMock.Object);

    _userManagerMock
        .Setup(x => x.CreateAsync(account, request.Password))
        .ReturnsAsync(IdentityResult.Failed(
            new IdentityError { Description = "Email already exists" }
        ));

    var result = await _accountService.RegisterAsync(request);

    Assert.False(result.IsSuccessful);
    Assert.Contains("Email already exists", result.Errors!);
}

[Fact]
public async Task RegisterAsync_ShouldReturnAllErrors_WhenMultipleErrorsOccur()
{
    var request = new RegistrationRequestModel
    {
        Email = "test@test.com",
        Password = "weak",
        FirstName = "John",
        LastName = "Doe",
        Role = "Patient"
    };

    var account = new Account { Email = request.Email };

    var factoryMock = new Mock<AccountFactory>();
    factoryMock
        .Setup(x => x.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
        .Returns(account);

    _factoryProviderMock
        .Setup(x => x.GetFactory(request.Role))
        .Returns(factoryMock.Object);

    _userManagerMock
        .Setup(x => x.CreateAsync(account, request.Password))
        .ReturnsAsync(IdentityResult.Failed(
            new IdentityError { Description = "Password too weak" },
            new IdentityError { Description = "Email invalid" }
        ));

    var result = await _accountService.RegisterAsync(request);

    Assert.False(result.IsSuccessful);
    Assert.Equal(2, result.Errors!.Count());
}

[Fact]
public async Task RegisterAsync_ShouldAddUserToRole_InLowerCase()
{
    var request = new RegistrationRequestModel
    {
        Email = "test@test.com",
        Password = "Password123!",
        FirstName = "John",
        LastName = "Doe",
        Role = "Patient"
    };

    var account = new Account { Email = request.Email };

    var factoryMock = new Mock<AccountFactory>();
    factoryMock
        .Setup(x => x.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
        .Returns(account);

    _factoryProviderMock
        .Setup(x => x.GetFactory(request.Role))
        .Returns(factoryMock.Object);

    _userManagerMock
        .Setup(x => x.CreateAsync(account, request.Password))
        .ReturnsAsync(IdentityResult.Success);

    _userManagerMock
        .Setup(x => x.AddToRoleAsync(account, "patient"))
        .ReturnsAsync(IdentityResult.Success);

    await _accountService.RegisterAsync(request);

    _userManagerMock.Verify(
        x => x.AddToRoleAsync(account, "patient"),
        Times.Once);
}

[Fact]
public async Task RegisterAsync_ShouldCallFactoryCreate_WithCorrectData()
{
    var request = new RegistrationRequestModel
    {
        Email = "test@test.com",
        Password = "Password123!",
        FirstName = "John",
        LastName = "Doe",
        Role = "Patient"
    };

    var factoryMock = new Mock<AccountFactory>();

    _factoryProviderMock
        .Setup(x => x.GetFactory(request.Role))
        .Returns(factoryMock.Object);

    _userManagerMock
        .Setup(x => x.CreateAsync(It.IsAny<Account>(), It.IsAny<string>()))
        .ReturnsAsync(IdentityResult.Success);

    _userManagerMock
        .Setup(x => x.AddToRoleAsync(It.IsAny<Account>(), It.IsAny<string>()))
        .ReturnsAsync(IdentityResult.Success);

    await _accountService.RegisterAsync(request);

    factoryMock.Verify(x =>
        x.Create(
            request.FirstName,
            request.LastName,
            request.Email),
        Times.Once);
    }
}
