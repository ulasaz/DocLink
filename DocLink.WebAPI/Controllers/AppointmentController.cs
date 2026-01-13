using DocLink.Core.Models;
using DocLink.Services.DTO_s;
using DocLink.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DocLink.WebAPI.Controllers;

[ApiController]
[Route("api/appoinment")]
public class AppointmentController : ControllerBase
{
    private IAppointmentService _appointmentService;

    public AppointmentController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }
    
    [HttpPost("book")]
    public async Task<ActionResult<AppointmentResponseModel>> BookAppointment([FromBody] AppointmentRequestModel? requestModel)
    {
        if (requestModel == null)
            return BadRequest(); 
        
        var result = await _appointmentService.BookAppointment(requestModel);

        if (!result.IsSuccessful)
            return BadRequest(result.Errors);
        return Ok(result);
    }
    [HttpGet("patient/{patientId}")]
    public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetAppointmentsByPatientIdAsync(Guid patientId)
    {
        var result = await _appointmentService.GetAppointmentsByPatientIdAsync(patientId);
        return Ok(result);
    }
    
}