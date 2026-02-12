using DocLink.Core.Factories;
using DocLink.Services.Services;
using NUnit.Framework;
using System;
using System.ComponentModel;

[TestFixture]
[NUnit.Framework.Category("Factory")]
[NUnit.Framework.Description("Testy jednostkowe AccountFactoryProvider (Warstwa Usług/Logiki)")]
public class AccountFactoryProviderTests
{
    private AccountFactoryProvider _provider;

    [SetUp]
    public void SetUp()
    {
        _provider = new AccountFactoryProvider();
    }

    [TearDown]
    public void TearDown()
    {
        _provider = null;
    }

    [TestCase("Patient", typeof(PatientFactory))]
    [TestCase("Specialist", typeof(SpecialistFactory))]
    [Order(1)]
    [NUnit.Framework.Description("Pobranie odpowiedniej fabryki na podstawie roli")]
    public void GetFactory_ShouldReturnCorrectFactoryType(string role, Type expectedType)
    {
        // 1) Jeśli

        // 2) Gdy
        var factory = _provider.GetFactory(role);

        // 3) Wtedy
        Assert.That(factory, Is.Not.Null, "Fabryka nie powinna być nullem");
        Assert.That(factory, Is.InstanceOf(expectedType), $"Dla roli {role} powinna zostać zwrócona fabryka typu {expectedType.Name}");
    }

    [Test, Order(2)]
    [NUnit.Framework.Description("Pobranie fabryki dla nieznanej roli powinno rzucić wyjątek")]
    public void GetFactory_ShouldThrowException_WhenRoleIsUnknown()
    {
        // 1) Jeśli
        var unknownRole = "Admin";

        // 2) Gdy & 3) Wtedy
        // Weryfikacja rzucenia wyjątku
        var ex = Assert.Throws<ArgumentException>(() => _provider.GetFactory(unknownRole));
        
        // Sprawdzenie komunikatu wyjątku
        Assert.That(ex.Message, Is.EqualTo("Unknown account type"));
    }
}