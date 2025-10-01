using DocLink.Services.DTO_s;
using DocLink.Services.Services;
using Microsoft.AspNetCore.Mvc;

namespace DocLink.WebAPI.Controllers;

[ApiController]
[Route("api/auth")]
public class AccountController : ControllerBase
{
    private AccountService _accountService;

    public AccountController(AccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpPost("/register")]
    public async Task<ActionResult<RegistrationRespondModel>> RegisterUserAsync([FromBody] RegistrationRequestModel requestModel)
    {
        var account = await _accountService.RegisterUserAsync(requestModel);
        var respondModel = new RegistrationRespondModel();
        if (account == null)
            respondModel.IsSuccessful = false;
        respondModel.IsSuccessful = true;

        return Ok(respondModel);
    }
    
    [HttpPost("/login")]
    public async Task<ActionResult<LoginRespondModel>> LoginUserAync([FromBody] LoginRequestModel requestModel)
    {
        var token = await _accountService.LoginAsync(requestModel);
        var respondModel = new LoginRespondModel()
        {
            IsSuccessful = true,
            Token = token
        };

        return Ok(respondModel);
    }
}