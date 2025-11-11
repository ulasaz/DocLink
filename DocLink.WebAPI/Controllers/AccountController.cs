using DocLink.Core.Models;
using DocLink.Services.DTO_s;
using DocLink.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DocLink.WebAPI.Controllers;

[ApiController]
[Route("api/auth")]
public class AccountController : ControllerBase
{
    private IAccountService _accountService;

    public AccountController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpPost("/register")]
    public async Task<ActionResult<RegistrationResponseModel>> RegisterUserAsync([FromBody] RegistrationRequestModel requestModel)
    {
        if (requestModel == null)
            return BadRequest(); 
        
        var result = await _accountService.RegisterAsync(requestModel);

       if (!result.IsSuccessful)
           return BadRequest(result.Errors);
       return Ok(result.IsSuccessful);
    }
    
    [HttpPost("/login")]
    public async Task<ActionResult<LoginResponseModel>> LoginUserAsync([FromBody] LoginRequestModel requestModel)
    {
        if (requestModel == null)
            return BadRequest();
        var result = await _accountService.LoginAsync(requestModel);
        
        if (!result.IsSuccessful)
        {
            return Unauthorized(new LoginResponseModel { Errors = ["Invalid login attempt"] });
        }
        return Ok(new LoginResponseModel {IsSuccessful = true, Token = "TOKEN"});
        
    }
}