using DocLink.Core.Models;
using DocLink.Services.DTO_s;
using DocLink.Services.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
namespace DocLink.Tests;

public class AccountServiceTests
{
    private readonly Mock<UserManager<Account>> _userManagerMock;
    private readonly Mock<IUserStore<Account>> _userStoreMock;
    private readonly Mock<SignInManager<Account>> _signInManagerMock;
    
    public AccountServiceTests()
    {
        _userStoreMock = new Mock<IUserStore<Account>>();
        _userManagerMock = new Mock<UserManager<Account>>(_userStoreMock.Object, null, null, null, null, null, null, null, null);
        var contextAccessor = new Mock<IHttpContextAccessor>();
        var claimsFactory = new Mock<IUserClaimsPrincipalFactory<Account>>();
        var options = new Mock<IOptions<IdentityOptions>>();
        var logger = new Mock<ILogger<SignInManager<Account>>>();
        var schemes = new Mock<IAuthenticationSchemeProvider>();
        _signInManagerMock = new Mock<SignInManager<Account>>(
            _userManagerMock.Object,
            contextAccessor.Object,
            claimsFactory.Object,
            options.Object,
            logger.Object,
            schemes.Object);
    }

    private AccountService CreateSut()
    {
        return new AccountService(_userManagerMock.Object, _signInManagerMock.Object);
    }

    [Fact]
    public async Task RegisterUserAsync_ShouldReturnSuccessfulResponse_WhenUserCreated()
    {
        _userManagerMock.Setup(x => x.CreateAsync(
            It.IsAny<Account>(),
            It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);

        var accountService = CreateSut();

        var requestModel = new RegistrationRequestModel
        {
            Email = "test@gmail.com",
            FirstName = "Test",
            LastName = "Test",
            Password = "test123"
        };

        var result = await accountService.RegisterAsync(requestModel);

        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public async Task SignInAsync_ShouldReturnSuccessfulResponse_WhenUserExists()
    {
        var requestModel = new LoginRequestModel
        {
            Email = "test@gmail.com",
            Password = "password-secret"
        };

        var account = new Account
        {
            Email = "test@gmail.com"
        };

        _userManagerMock.Setup(x => x.FindByEmailAsync(requestModel.Email)).ReturnsAsync(account);
        
        _signInManagerMock.Setup(x => x.PasswordSignInAsync(
            account,
            requestModel.Password,
            false,
            false)).ReturnsAsync(SignInResult.Success);

        var result  = await CreateSut().LoginAsync(requestModel);
        
        Assert.True(result.IsSuccessful);
    }

}