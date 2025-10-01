using DocLink.Core.Interfaces;
using DocLink.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace DocLink.Data.Repositories;

public class AccountRepository : IAccountRepository
{
    private ApplicationContext _applicationContext;

    public AccountRepository(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }


    public async Task<Account?> GetByIdAsync(Guid id)
    {
        var account = await _applicationContext.Accounts.FirstOrDefaultAsync(a => a.Id == id);
        return account;
    }

    public async Task<Account?> GetByEmailAsync(string email)
    {
        var account = await _applicationContext.Accounts.FirstOrDefaultAsync(a => a.Email == email);
        return account;
    }

    public async Task AddAsync(Account account)
    {
        await _applicationContext.Accounts.AddAsync(account);
        await _applicationContext.SaveChangesAsync();
    }

    public async Task<Account?> UpdateAsync(Account account)
    {
        _applicationContext.Accounts.Update(account);
        
        await _applicationContext.SaveChangesAsync();
        
        return account;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var account = await _applicationContext.Accounts.FirstOrDefaultAsync(a => a.Id == id);

        if (account != null) _applicationContext.Accounts.Remove(account);

        await _applicationContext.SaveChangesAsync();
        
        return true;
    }
}