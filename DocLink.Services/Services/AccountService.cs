using DocLink.Core.Models;
using DocLink.Services.DTO_s;
using DocLink.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace DocLink.Services.Services;

public class AccountService : IAccountService
{
    private readonly UserManager<Account> _userManager;
    private readonly IAccountFactoryProvider _factoryProvider;
    private readonly ITokenService _tokenService;

    public AccountService(UserManager<Account> userManager, SignInManager<Account> signInManager, IAccountFactoryProvider factoryProvider, ITokenService tokenService)
    {
        _userManager = userManager;
        _factoryProvider = factoryProvider;
        _tokenService = tokenService;
    }

    public async Task<RegistrationResponseModel> RegisterAsync(RegistrationRequestModel requestModel)
    {
        var factory = _factoryProvider.GetFactory(requestModel.Role);
        var account = factory.Create(
            requestModel.FirstName,
            requestModel.LastName,
            requestModel.Email
        );

        var result = await _userManager.CreateAsync(account, requestModel.Password);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(account, requestModel.Role.ToLower());
            return new RegistrationResponseModel { IsSuccessful = true };
        }
        
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description);
            return new RegistrationResponseModel { Errors = errors, IsSuccessful = false};
        }

        return new RegistrationResponseModel {IsSuccessful = true};
    }

    public async Task<LoginResponseModel> LoginAsync(LoginRequestModel loginRequestModel)
    {
        var account =  await _userManager.FindByEmailAsync(loginRequestModel.Email);
        
        if(account is null)
            return new LoginResponseModel { Errors = ["Invalid login attempt"] };

        var isPasswordValid = await _userManager.CheckPasswordAsync(account, loginRequestModel.Password);
    
        if (!isPasswordValid)
            return new LoginResponseModel { Errors = ["Invalid login attempt"], IsSuccessful = false };
        var token = await _tokenService.GenerateJwtToken(account);
        return new LoginResponseModel { IsSuccessful = true, Token = token };
    }

    public async Task<Account> GetAccountByEmail(string email)
    {
        return await _userManager.FindByEmailAsync(email) ?? throw new InvalidOperationException();
    }

    public async Task<PatientProfileDto> GetPatientProfileAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) 
            throw new Exception("User not found");

        var roles = await _userManager.GetRolesAsync(user);
        
        if (!roles.Contains("Patient"))
        {
            throw new Exception("You are not a patient");
        }
        
        return new PatientProfileDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber ?? ""
        };
    }
}