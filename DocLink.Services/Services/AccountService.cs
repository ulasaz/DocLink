using DocLink.Core.Models;
using DocLink.Services.DTO_s;
using DocLink.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace DocLink.Services.Services;

public class AccountService : IAccountService
{
    private readonly UserManager<Account> _userManager;
    private readonly SignInManager<Account> _signInManager;

    public AccountService(UserManager<Account> userManager, SignInManager<Account> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<RegistrationResponseModel> RegisterAsync(RegistrationRequestModel requestModel)
    {
        var account = new Account
        {
            FirstName = requestModel.FirstName,
            LastName = requestModel.LastName,
            Email = requestModel.Email,
            PasswordHash = requestModel.PasswordHash
        };

        var result = await _userManager.CreateAsync(account, requestModel.PasswordHash);
        
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description);
            return new RegistrationResponseModel { Errors = errors };
        }

        return new RegistrationResponseModel {IsSuccessful = true};
    }

    public async Task<LoginResponseModel> LoginAsync(LoginRequestModel loginRequestModel)
    {
        var account =  await _userManager.FindByEmailAsync(loginRequestModel.Email);
        
        if(account is null)
            return new LoginResponseModel { Errors = ["Invalid login attempt"] };

        var result = await _signInManager.PasswordSignInAsync(
            account, 
            loginRequestModel.Password,
            isPersistent: false,   
            lockoutOnFailure: false 
        );
        
        if (!result.Succeeded)
            return new LoginResponseModel { Errors = ["Invalid login attempt"] };

        return new LoginResponseModel {IsSuccessful = true, Token = "TOKEN"};
    }
}