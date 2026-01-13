using DocLink.Core.Models;

namespace DocLink.Core.Factories;

public class PatientFactory : AccountFactory
{
    public override Account Create(string firstName, string lastName, string email) => new Patient
    {
        FirstName = firstName,
        LastName = lastName,
        Email = email,
        UserName = email
    };
}