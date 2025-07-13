using System;
using Abyse.ReUI;
using NUnit.Framework;

public class ServiceLocatorTests
{
    private ServiceLocator _locator;

    [SetUp]
    public void SetUp()
    {
        _locator = new ServiceLocator();
    }

    [Test]
    public void Register_Instance_ByType_ShouldBeResolvable()
    {
        var service = new MyService();
        _locator.Register(service);

        var resolved = _locator.Get<MyService>();

        Assert.AreSame(service, resolved);
    }

    [Test]
    public void Register_Instance_ById_ShouldBeResolvable()
    {
        var service = new MyService();
        _locator.Register(service, "custom-id");

        var resolved = _locator.Get<MyService>("custom-id");

        Assert.AreSame(service, resolved);
    }

    [Test]
    public void Register_Factory_ByType_ShouldReturnNewInstanceEachTime()
    {
        var callCount = 0;

        _locator.Register<MyService>(() =>
        {
            callCount++;
            return new MyService();
        });

        var a = _locator.Get<MyService>();
        var b = _locator.Get<MyService>();

        Assert.AreNotSame(a, b);
        Assert.AreEqual(2, callCount);
    }

    [Test]
    public void Register_Factory_ById_ShouldReturnInstance()
    {
        _locator.Register<MyService>(() => new MyService(), "factory-id");

        var resolved = _locator.Get<MyService>("factory-id");

        Assert.IsNotNull(resolved);
    }

    [Test]
    public void Get_UnregisteredType_ShouldThrow()
    {
        Assert.Throws<InvalidOperationException>(() => { _locator.Get<MyService>(); });
    }

    [Test]
    public void Get_UnregisteredId_ShouldThrow()
    {
        Assert.Throws<InvalidOperationException>(() => { _locator.Get<MyService>("nonexistent-id"); });
    }

    [Test]
    public void TryGet_Registered_ByType_ShouldReturnTrueAndValue()
    {
        var service = new MyService();
        _locator.Register(service);

        var success = _locator.TryGet<MyService>(out var resolved);

        Assert.IsTrue(success);
        Assert.AreSame(service, resolved);
    }

    [Test]
    public void TryGet_Registered_ById_ShouldReturnTrueAndValue()
    {
        var service = new MyService();
        _locator.Register(service, "my-id");

        var success = _locator.TryGet<MyService>(out var resolved, "my-id");

        Assert.IsTrue(success);
        Assert.AreSame(service, resolved);
    }

    [Test]
    public void TryGet_Unregistered_ShouldReturnFalse()
    {
        var success = _locator.TryGet<MyService>(out var resolved);

        Assert.IsFalse(success);
        Assert.IsNull(resolved);
    }

    [Test]
    public void Register_DuplicateType_ShouldThrow()
    {
        _locator.Register(new MyService());

        Assert.Throws<InvalidOperationException>(() => { _locator.Register(new MyService()); });
    }

    [Test]
    public void Register_DuplicateId_ShouldThrow()
    {
        _locator.Register(new MyService(), "shared-id");

        Assert.Throws<InvalidOperationException>(() => { _locator.Register(new MyService(), "shared-id"); });
    }

    private class MyService
    {
    }

    private class AnotherService : MyService
    {
    }
}