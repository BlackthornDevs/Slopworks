using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class FactoryGridMultiLevelTests
{
    private FactoryGrid _grid;

    [SetUp]
    public void SetUp()
    {
        _grid = new FactoryGrid();
    }

    // -- CellToWorld with levels --

    [Test]
    public void CellToWorld_Level0_YIsZero()
    {
        var world = _grid.CellToWorld(new Vector2Int(5, 5), 0);
        Assert.AreEqual(0f, world.y, 0.001f);
    }

    [Test]
    public void CellToWorld_Level1_YIsLevelHeight()
    {
        var world = _grid.CellToWorld(new Vector2Int(5, 5), 1);
        Assert.AreEqual(FactoryGrid.LevelHeight, world.y, 0.001f);
    }

    [Test]
    public void CellToWorld_Level2_YIsTwoTimesLevelHeight()
    {
        var world = _grid.CellToWorld(new Vector2Int(5, 5), 2);
        Assert.AreEqual(FactoryGrid.LevelHeight * 2f, world.y, 0.001f);
    }

    [Test]
    public void CellToWorld_LevelDoesNotAffectXZ()
    {
        var level0 = _grid.CellToWorld(new Vector2Int(3, 7), 0);
        var level1 = _grid.CellToWorld(new Vector2Int(3, 7), 1);
        Assert.AreEqual(level0.x, level1.x, 0.001f);
        Assert.AreEqual(level0.z, level1.z, 0.001f);
    }

    // -- Independent level placement --

    [Test]
    public void Place_DifferentLevels_Independent()
    {
        var data0 = new BuildingData("ground", new Vector2Int(5, 5), Vector2Int.one, 0, 0);
        var data1 = new BuildingData("upper", new Vector2Int(5, 5), Vector2Int.one, 0, 1);

        _grid.Place(new Vector2Int(5, 5), Vector2Int.one, 0, data0);
        _grid.Place(new Vector2Int(5, 5), Vector2Int.one, 1, data1);

        Assert.AreSame(data0, _grid.GetAt(new Vector2Int(5, 5), 0));
        Assert.AreSame(data1, _grid.GetAt(new Vector2Int(5, 5), 1));
    }

    [Test]
    public void SameCell_DifferentLevels_NoCollision()
    {
        var data0 = new BuildingData("ground", new Vector2Int(5, 5), Vector2Int.one);
        _grid.Place(new Vector2Int(5, 5), Vector2Int.one, 0, data0);

        Assert.IsTrue(_grid.CanPlace(new Vector2Int(5, 5), Vector2Int.one, 1));
    }

    [Test]
    public void SameCell_SameLevel_Collision()
    {
        var data = new BuildingData("ground", new Vector2Int(5, 5), Vector2Int.one);
        _grid.Place(new Vector2Int(5, 5), Vector2Int.one, 0, data);

        Assert.IsFalse(_grid.CanPlace(new Vector2Int(5, 5), Vector2Int.one, 0));
    }

    // -- MaxLevels --

    [Test]
    public void CanPlace_ExceedsMaxLevels_ReturnsFalse()
    {
        Assert.IsFalse(_grid.CanPlace(new Vector2Int(5, 5), Vector2Int.one, FactoryGrid.MaxLevels));
    }

    [Test]
    public void CanPlace_NegativeLevel_ReturnsFalse()
    {
        Assert.IsFalse(_grid.CanPlace(new Vector2Int(5, 5), Vector2Int.one, -1));
    }

    [Test]
    public void CanPlace_TopLevel_ReturnsTrue()
    {
        Assert.IsTrue(_grid.CanPlace(new Vector2Int(5, 5), Vector2Int.one, FactoryGrid.MaxLevels - 1));
    }

    // -- Remove at level --

    [Test]
    public void Remove_AtLevel_DoesNotAffectOtherLevels()
    {
        var data0 = new BuildingData("ground", new Vector2Int(5, 5), Vector2Int.one, 0, 0);
        var data1 = new BuildingData("upper", new Vector2Int(5, 5), Vector2Int.one, 0, 1);

        _grid.Place(new Vector2Int(5, 5), Vector2Int.one, 0, data0);
        _grid.Place(new Vector2Int(5, 5), Vector2Int.one, 1, data1);

        _grid.Remove(new Vector2Int(5, 5), Vector2Int.one, 0);

        Assert.IsNull(_grid.GetAt(new Vector2Int(5, 5), 0));
        Assert.AreSame(data1, _grid.GetAt(new Vector2Int(5, 5), 1));
    }

    [Test]
    public void Remove_AtLevel_AllowsReplacement()
    {
        var data = new BuildingData("ground", new Vector2Int(5, 5), Vector2Int.one, 0, 1);
        _grid.Place(new Vector2Int(5, 5), Vector2Int.one, 1, data);
        _grid.Remove(new Vector2Int(5, 5), Vector2Int.one, 1);

        Assert.IsTrue(_grid.CanPlace(new Vector2Int(5, 5), Vector2Int.one, 1));
    }

    // -- BuildingData level defaults --

    [Test]
    public void BuildingData_DefaultLevel_IsZero()
    {
        var data = new BuildingData("test", Vector2Int.zero, Vector2Int.one);
        Assert.AreEqual(0, data.Level);
    }

    [Test]
    public void BuildingData_ExplicitLevel_IsPreserved()
    {
        var data = new BuildingData("test", Vector2Int.zero, Vector2Int.one, 0, 2);
        Assert.AreEqual(2, data.Level);
    }

    // -- Backward compatibility (2D wrappers use level 0) --

    [Test]
    public void LegacyCanPlace_UsesLevel0()
    {
        var data = new BuildingData("ground", new Vector2Int(5, 5), Vector2Int.one);
        _grid.Place(new Vector2Int(5, 5), Vector2Int.one, data);

        // Legacy CanPlace should see the level-0 building
        Assert.IsFalse(_grid.CanPlace(new Vector2Int(5, 5), Vector2Int.one));
    }

    [Test]
    public void LegacyPlace_And_LevelAwareGet_AreConsistent()
    {
        var data = new BuildingData("ground", new Vector2Int(5, 5), Vector2Int.one);
        _grid.Place(new Vector2Int(5, 5), Vector2Int.one, data);

        Assert.AreSame(data, _grid.GetAt(new Vector2Int(5, 5), 0));
        Assert.IsNull(_grid.GetAt(new Vector2Int(5, 5), 1));
    }

    [Test]
    public void LegacyGetAt_ReturnsLevel0Data()
    {
        var data = new BuildingData("ground", new Vector2Int(5, 5), Vector2Int.one);
        _grid.Place(new Vector2Int(5, 5), Vector2Int.one, 0, data);

        Assert.AreSame(data, _grid.GetAt(new Vector2Int(5, 5)));
    }

    [Test]
    public void LegacyRemove_ClearsLevel0()
    {
        var data = new BuildingData("ground", new Vector2Int(5, 5), Vector2Int.one);
        _grid.Place(new Vector2Int(5, 5), Vector2Int.one, data);
        _grid.Remove(new Vector2Int(5, 5), Vector2Int.one);

        Assert.IsNull(_grid.GetAt(new Vector2Int(5, 5), 0));
    }

    // -- Multi-cell placement at levels --

    [Test]
    public void Place_MultiCell_AtLevel1_AllCellsOccupied()
    {
        var data = new BuildingData("smelter", new Vector2Int(10, 10), new Vector2Int(3, 2), 0, 1);
        _grid.Place(new Vector2Int(10, 10), new Vector2Int(3, 2), 1, data);

        for (int x = 10; x < 13; x++)
        {
            for (int z = 10; z < 12; z++)
            {
                Assert.AreSame(data, _grid.GetAt(new Vector2Int(x, z), 1),
                    $"Cell ({x},{z}) at level 1 should be occupied");
            }
        }

        // Level 0 should remain empty
        Assert.IsNull(_grid.GetAt(new Vector2Int(10, 10), 0));
    }
}
