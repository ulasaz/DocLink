using DocLink.Core.Factories;
using DocLink.Services.Interfaces;

namespace DocLink.Services.Services;

public class AccountFactoryProvider : IAccountFactoryProvider
{
    public AccountFactory GetFactory(string role)
    {
        return role switch
        {
            "Patient" => new PatientFactory(),
            "Specialist" => new SpecialistFactory(),
            _ => throw new ArgumentException("Unknown account type")
        };
    }
}