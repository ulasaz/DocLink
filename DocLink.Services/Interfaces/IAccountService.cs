using DocLink.Core.Models;
using DocLink.Services.DTO_s;

namespace DocLink.Services.Interfaces;

public interface IAccountService
{
    Task<RegistrationResponseModel> RegisterAsync(RegistrationRequestModel requestModel);
    Task<LoginResponseModel> LoginAsync(LoginRequestModel loginRequestModel);
}