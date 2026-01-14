using DocLink.Core.Models;

namespace DocLink.Services.Interfaces;

public interface ITokenService
{
    Task<string> GenerateJwtToken(Account user);
}