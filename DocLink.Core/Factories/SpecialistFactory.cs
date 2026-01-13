using DocLink.Core.Models;

namespace DocLink.Core.Factories;

public class SpecialistFactory : AccountFactory
{
    public override Account Create(string firstName, string lastName, string email)
        => new Specialist
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            UserName = email
        };
}