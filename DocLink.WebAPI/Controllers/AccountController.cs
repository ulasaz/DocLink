using System.Security.Claims;
using DocLink.Core.Models;
using DocLink.Services;
using DocLink.Services.DTO_s;
using DocLink.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace DocLink.WebAPI.Controllers;

[ApiController]
[Route("api/auth")]
public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly TokenService _tokenService;

    public AccountController(IAccountService accountService, TokenService tokenService)
    {
        _accountService = accountService;
        _tokenService = tokenService;
    }
    
      [HttpPost("register")]
    public async Task<ActionResult<RegistrationResponseModel>> RegisterUserAsync([FromBody] RegistrationRequestModel requestModel)
    {
        if (requestModel == null)
            return BadRequest(); 
        
        var result = await _accountService.RegisterAsync(requestModel);

       if (!result.IsSuccessful)
           return BadRequest(result.Errors);
       return Ok(result.IsSuccessful);
    }
    
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseModel>> LoginUserAsync([FromBody] LoginRequestModel requestModel)
    {
        if (requestModel == null)
            return BadRequest();

        var result = await _accountService.LoginAsync(requestModel);

        if (!result.IsSuccessful)
        {
            return Unauthorized(result);
        }

        return Ok(result);
    }
    
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Patient")]
    [HttpGet("patient-profile")]
    public async Task<ActionResult<PatientProfileDto>> GetMyPatientProfile()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    
        if (userId == null) return Unauthorized();

        try 
        {
            var profile = await _accountService.GetPatientProfileAsync(userId);
            return Ok(profile);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}