using DocLink.Core.Models;

namespace DocLink.Core.Interfaces;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(Guid id);
    Task<Account?> GetByEmailAsync(string email);
    Task AddAsync(Account account);
    Task<Account?> UpdateAsync(Account account);
    Task<bool> DeleteAsync(Guid id);
}