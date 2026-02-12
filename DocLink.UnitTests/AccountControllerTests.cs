using DocLink.Services.DTO_s;
using DocLink.Services.Interfaces;
using DocLink.WebAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;
using DocLink.Services;

[TestFixture]
[Category("Controller")]
[Description("Testy kontrolera konta")]
public class AccountControllerTests
{
    private Mock<IAccountService> _accountServiceMock;
    
    private AccountController _controller;

    [SetUp]
    public void SetUp()
    {
        _accountServiceMock = new Mock<IAccountService>();
        _controller = new AccountController(_accountServiceMock.Object); 
    }

    [Test]
    public async Task RegisterUserAsync_ReturnsOk_WhenRegistrationSucceeds()
    {
        // 1) Jeśli
        var request = new RegistrationRequestModel { Email = "new@user.com" };
        var response = new RegistrationResponseModel { IsSuccessful = true };

        _accountServiceMock.Setup(s => s.RegisterAsync(request))
                           .ReturnsAsync(response);

        // 2) Gdy
        var actionResult = await _controller.RegisterUserAsync(request);

        // 3) Wtedy
        var okResult = actionResult.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.StatusCode, Is.EqualTo(200));
        Assert.That(okResult.Value, Is.True); // Kontroler zwraca Ok(result.IsSuccessful), czyli bool
    }

    [Test]
    public async Task RegisterUserAsync_ReturnsBadRequest_WhenRegistrationFails()
    {
        // 1) Jeśli
        var request = new RegistrationRequestModel { Email = "fail@user.com" };
        var response = new RegistrationResponseModel 
        { 
            IsSuccessful = false, 
            Errors = new[] { "Email already taken" } 
        };

        _accountServiceMock.Setup(s => s.RegisterAsync(request))
                           .ReturnsAsync(response);

        // 2) Gdy
        var actionResult = await _controller.RegisterUserAsync(request);

        // 3) Wtedy
        var badRequest = actionResult.Result as BadRequestObjectResult;
        Assert.That(badRequest, Is.Not.Null);
        Assert.That(badRequest.StatusCode, Is.EqualTo(400));
        Assert.That(badRequest.Value, Is.EqualTo(response.Errors)); // Kontroler zwraca błędy
    }
    
    [Test]
    public async Task RegisterUserAsync_ReturnsBadRequest_WhenRequestIsNull()
    {
        // 1) Jeśli
        RegistrationRequestModel request = null;

        // 2) Gdy
        var actionResult = await _controller.RegisterUserAsync(request);

        // 3) Wtedy
        Assert.That(actionResult.Result, Is.InstanceOf<BadRequestResult>());
        _accountServiceMock.Verify(s => s.RegisterAsync(It.IsAny<RegistrationRequestModel>()), Times.Never);
    }
}