using Logic;

namespace unit_tests;

public class Tests {
    [SetUp]
    public void Setup() { }

    [Test]
    public void GridHasCorrectSize() {
        // given
        var logicGrid = new LogicGrid(4, new TestRandom());
        
        // when
        var grid = logicGrid.Grid;
        
        // then
        Assert.AreEqual(4, grid.Capacity);
        foreach (var row in grid) {
            Assert.AreEqual(4, row.Capacity);
        }
    }
}

internal class TestRandom : Random {
    public override int Next() {
        return base.Next();
    }

    public override double NextDouble() {
        return base.NextDouble();
    }
}