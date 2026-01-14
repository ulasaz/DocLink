using DocLink.Core.Factories;

namespace DocLink.Services.Interfaces;

public interface IAccountFactoryProvider
{
    AccountFactory GetFactory(string role);
}
