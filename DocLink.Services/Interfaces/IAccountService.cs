using DocLink.Core.Models;
using DocLink.Services.DTO_s;

namespace DocLink.Services.Interfaces;

public interface IAccountService
{
    Task<Account?> RegisterUserAsync(RegistrationRequestModel requestModel);
    Task<string> LoginAsync(LoginRequestModel requestModel);
    Task<Account?> GetUserByIdAsync(Guid id);
    Task<bool> DeleteUserAsync(Guid id);
    Task<Account?> GetUserByEmailAsync(string email);
    Task<Account?> UpdateAsync(Account account);
}