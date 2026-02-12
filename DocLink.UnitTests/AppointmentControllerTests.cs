using DocLink.Services.DTO_s;
using DocLink.Services.Interfaces;
using DocLink.WebAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

[TestFixture]
[Category("Controller")]
public class AppointmentControllerTests
{
    private Mock<IAppointmentService> _serviceMock;
    private AppointmentController _controller;

    [SetUp]
    public void SetUp()
    {
        _serviceMock = new Mock<IAppointmentService>();
        _controller = new AppointmentController(_serviceMock.Object);
    }

    [Test]
    public async Task BookAppointment_ReturnsOk_WhenServiceSucceeds()
    {
        // 1) Jeśli (Given)
        var request = new AppointmentRequestModel();
        var expectedResponse = new AppointmentResponseModel { IsSuccessful = true };

        _serviceMock.Setup(s => s.BookAppointment(request))
            .ReturnsAsync(expectedResponse);

        // 2) Gdy (When)
        var actionResult = await _controller.BookAppointment(request);

        // 3) Wtedy (Then)
        // Weryfikacja typu wyniku (ActionResult<T>)
        var okResult = actionResult.Result as OkObjectResult;
        
        // Poprawione asercje (NUnit 4):
        Assert.That(okResult, Is.Not.Null, "Wynik nie powinien być nullem");
        Assert.That(okResult.StatusCode, Is.EqualTo(200), "Status HTTP powinien być 200 OK");
        Assert.That(okResult.Value, Is.EqualTo(expectedResponse), "Zwrócony obiekt powinien być zgodny z oczekiwanym");
    }

    [Test]
    public async Task BookAppointment_ReturnsBadRequest_WhenRequestIsNull()
    {
        // 1) Jeśli
        AppointmentRequestModel request = null;

        // 2) Gdy
        var actionResult = await _controller.BookAppointment(request);

        // 3) Wtedy
        // Sprawdzenie typu (InstanceOf) w nowej składni
        Assert.That(actionResult.Result, Is.InstanceOf<BadRequestResult>(), "Powinien zostać zwrócony BadRequest (400)");
        
        // Weryfikacja, że serwis NIE został wywołany
        _serviceMock.Verify(s => s.BookAppointment(It.IsAny<AppointmentRequestModel>()), Times.Never);
    }

    [Test]
    public async Task BookAppointment_ReturnsBadRequest_WhenServiceFails()
    {
        // 1) Jeśli
        var request = new AppointmentRequestModel();
        var failureResponse = new AppointmentResponseModel 
        { 
            IsSuccessful = false, 
            Errors = new[] { "Error 1" } 
        };

        _serviceMock.Setup(s => s.BookAppointment(request))
            .ReturnsAsync(failureResponse);

        // 2) Gdy
        var actionResult = await _controller.BookAppointment(request);

        // 3) Wtedy
        var badRequest = actionResult.Result as BadRequestObjectResult;
        
        // Poprawione asercje:
        Assert.That(badRequest, Is.Not.Null, "Wynik nie powinien być nullem");
        Assert.That(badRequest.StatusCode, Is.EqualTo(400), "Status HTTP powinien być 400 BadRequest");
        Assert.That(badRequest.Value, Is.EqualTo(failureResponse.Errors), "Powinna zostać zwrócona lista błędów");
    }
}