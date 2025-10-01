using DocLink.Core.Models;
using DocLink.Data.Repositories;
using DocLink.Services.DTO_s;
using DocLink.Services.Interfaces;

namespace DocLink.Services.Services;

public class AccountService : IAccountService
{
    private AccountRepository _accountRepository;

    public AccountService(AccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async Task<Account?> RegisterUserAsync(RegistrationRequestModel requestModel)
    {
        var account = new Account
        {
            FirstName = requestModel.FirstName,
            LastName = requestModel.LastName,
            PasswordHash = requestModel.Password,
            Email = requestModel.Email
        };
        await _accountRepository.AddAsync(account);
        
        return account;
    }
    
    public async Task<string> LoginAsync(LoginRequestModel requestModel)
    {
        await _accountRepository.GetByEmailAsync(requestModel.Email);
        string token = "FAKE_TOKEN_FOR_NOW";
        return token;
    }

    public async Task<Account?> GetUserByIdAsync(Guid id)
    {
       return await _accountRepository.GetByIdAsync(id);
    }

    public async Task<bool> DeleteUserAsync(Guid id)
    {
        return await _accountRepository.DeleteAsync(id);
    }

    public async Task<Account?> GetUserByEmailAsync(string email)
    {
        return await _accountRepository.GetByEmailAsync(email);
    }

    public async Task<Account?> UpdateAsync(Account account)
    {
        return await _accountRepository.UpdateAsync(account);
    }
}