using DocLink.Core.Models;

namespace DocLink.Core.Factories;

public abstract class AccountFactory
{
    public abstract Account Create(string firstName, string lastName, string email);
}