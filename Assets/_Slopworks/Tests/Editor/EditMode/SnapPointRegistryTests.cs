using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class SnapPointRegistryTests
{
    private SnapPointRegistry _registry;

    [SetUp]
    public void SetUp()
    {
        _registry = new SnapPointRegistry();
    }

    private SnapPoint CreateSnap(Vector2Int cell, int level, Vector2Int edgeDir,
        SnapPointType type = SnapPointType.FoundationEdge, BuildingData owner = null)
    {
        owner ??= new BuildingData("test", cell, Vector2Int.one, 0, level);
        return new SnapPoint(cell, level, edgeDir, type, owner);
    }

    // -- Register / Count --

    [Test]
    public void NewRegistry_CountIsZero()
    {
        Assert.AreEqual(0, _registry.Count);
    }

    [Test]
    public void Register_IncrementsCount()
    {
        _registry.Register(CreateSnap(Vector2Int.zero, 0, Vector2Int.up));
        Assert.AreEqual(1, _registry.Count);
    }

    // -- GetAt --

    [Test]
    public void GetAt_ReturnsRegisteredSnap()
    {
        var snap = CreateSnap(new Vector2Int(5, 5), 0, Vector2Int.up);
        _registry.Register(snap);

        var found = _registry.GetAt(new Vector2Int(5, 5), 0, Vector2Int.up);
        Assert.AreSame(snap, found);
    }

    [Test]
    public void GetAt_WrongDirection_ReturnsNull()
    {
        _registry.Register(CreateSnap(new Vector2Int(5, 5), 0, Vector2Int.up));

        Assert.IsNull(_registry.GetAt(new Vector2Int(5, 5), 0, Vector2Int.right));
    }

    [Test]
    public void GetAt_WrongLevel_ReturnsNull()
    {
        _registry.Register(CreateSnap(new Vector2Int(5, 5), 0, Vector2Int.up));

        Assert.IsNull(_registry.GetAt(new Vector2Int(5, 5), 1, Vector2Int.up));
    }

    [Test]
    public void GetAt_EmptyCell_ReturnsNull()
    {
        Assert.IsNull(_registry.GetAt(new Vector2Int(5, 5), 0, Vector2Int.up));
    }

    // -- Unregister --

    [Test]
    public void Unregister_DecrementsCount()
    {
        var snap = CreateSnap(Vector2Int.zero, 0, Vector2Int.up);
        _registry.Register(snap);
        _registry.Unregister(snap);
        Assert.AreEqual(0, _registry.Count);
    }

    [Test]
    public void Unregister_RemovesFromLookup()
    {
        var snap = CreateSnap(new Vector2Int(5, 5), 0, Vector2Int.up);
        _registry.Register(snap);
        _registry.Unregister(snap);

        Assert.IsNull(_registry.GetAt(new Vector2Int(5, 5), 0, Vector2Int.up));
    }

    // -- GetPointsForOwner --

    [Test]
    public void GetPointsForOwner_ReturnsManagedList()
    {
        var owner = new BuildingData("test", new Vector2Int(5, 5), Vector2Int.one);
        var snap1 = CreateSnap(new Vector2Int(5, 5), 0, Vector2Int.up, owner: owner);
        var snap2 = CreateSnap(new Vector2Int(5, 5), 0, Vector2Int.right, owner: owner);
        _registry.Register(snap1);
        _registry.Register(snap2);

        var result = _registry.GetPointsForOwner(owner);
        Assert.AreEqual(2, result.Count);
    }

    [Test]
    public void GetPointsForOwner_NoMatch_ReturnsEmpty()
    {
        var owner = new BuildingData("test", Vector2Int.zero, Vector2Int.one);
        var result = _registry.GetPointsForOwner(owner);
        Assert.AreEqual(0, result.Count);
    }

    // -- GetAvailableAt --

    [Test]
    public void GetAvailableAt_ReturnsUnoccupiedOnly()
    {
        var owner = new BuildingData("test", new Vector2Int(5, 5), Vector2Int.one);
        var snap1 = CreateSnap(new Vector2Int(5, 5), 0, Vector2Int.up, owner: owner);
        var snap2 = CreateSnap(new Vector2Int(5, 5), 0, Vector2Int.right, owner: owner);
        snap2.IsOccupied = true;
        _registry.Register(snap1);
        _registry.Register(snap2);

        var available = _registry.GetAvailableAt(new Vector2Int(5, 5), 0);
        Assert.AreEqual(1, available.Count);
        Assert.AreSame(snap1, available[0]);
    }
}
