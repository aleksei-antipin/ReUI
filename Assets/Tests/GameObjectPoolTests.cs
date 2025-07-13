using System;
using System.Collections.Generic;
using Abyse.Pooling;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

public class GameObjectPoolTests
{
    private GameObjectPool _pool;
    private GameObject _stash;

    [SetUp]
    public void SetUp()
    {
        _stash = new GameObject("Stash");
        _pool = new GameObjectPool(_stash);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_stash);
    }

    [Test]
    public void RegisterPrefab_ByType_AndGet_ShouldReturnInstantiatedObject()
    {
        var prefab = new GameObject("MyPrefab");

        _pool.Register<GameObject>(prefab);

        var obj = _pool.Get<GameObject>();

        Assert.IsNotNull(obj);
        Assert.AreNotSame(prefab, obj);
        Assert.AreEqual(_stash.transform, obj.transform.parent);

        Object.DestroyImmediate(prefab);
    }

    [Test]
    public void RegisterPrefab_ById_AndTryGet_ShouldSucceed()
    {
        var prefab = new GameObject("ByIdPrefab");

        _pool.Register<GameObject>(prefab, "custom-id");

        var success = _pool.TryGet<GameObject>(out var obj, "custom-id");

        Assert.IsTrue(success);
        Assert.IsNotNull(obj);

        Object.DestroyImmediate(prefab);
    }

    [Test]
    public void RegisterFactory_ById_ShouldReturnFactoryInstance()
    {
        var name = "FactoryPrefab";
        _pool.Register<GameObject>(() => new GameObject(name), "factory-id");

        var obj = _pool.Get<GameObject>("factory-id");

        Assert.IsNotNull(obj);
        Assert.That(obj.name.StartsWith(name));
    }

    [Test]
    public void TryReturn_And_TryGet_ShouldReuseObject()
    {
        var prefab = new GameObject("Reusable");
        _pool.Register<GameObject>(prefab);

        var obj1 = _pool.Get<GameObject>();
        var returned = _pool.TryReturn(obj1);

        Assert.IsTrue(returned);

        var success = _pool.TryGet<GameObject>(out var obj2);

        Assert.IsTrue(success);
        Assert.AreSame(obj1, obj2);
        Assert.AreEqual(_stash.transform, obj2.transform.parent);

        Object.DestroyImmediate(prefab);
    }

    [Test]
    public void Get_InvalidId_ShouldThrow()
    {
        var ex = Assert.Throws<KeyNotFoundException>(() => _pool.Get<GameObject>("unknown"));
        Assert.That(ex.Message, Does.Contain("No prefab or factory"));
    }

    [Test]
    public void Register_SameTypeTwice_ShouldThrow()
    {
        var prefab1 = new GameObject("A");
        var prefab2 = new GameObject("B");

        _pool.Register<GameObject>(prefab1);

        Assert.Throws<InvalidOperationException>(() => _pool.Register<GameObject>(prefab2));

        Object.DestroyImmediate(prefab1);
        Object.DestroyImmediate(prefab2);
    }

    [Test]
    public void Register_SameIdTwice_ShouldThrow()
    {
        var prefab1 = new GameObject("X");
        var prefab2 = new GameObject("Y");

        _pool.Register<GameObject>(prefab1, "same-id");

        Assert.Throws<InvalidOperationException>(() => _pool.Register<GameObject>(prefab2, "same-id"));

        Object.DestroyImmediate(prefab1);
        Object.DestroyImmediate(prefab2);
    }

    [Test]
    public void TryReturn_UntrackedObject_ShouldReturnFalse()
    {
        var obj = new GameObject("NotFromPool");

        var result = _pool.TryReturn(obj);

        Assert.IsFalse(result);

        Object.DestroyImmediate(obj);
    }
}