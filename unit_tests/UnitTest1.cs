using Logic;

namespace unit_tests;

public class Tests {
    [SetUp]
    public void Setup() { }

    [Test]
    public void Test1() {
        // given
        var logicGrid = new LogicGrid(4);
        
        // when
        var grid = logicGrid.Grid;
        
        // then
        Assert.AreEqual(4, grid.Capacity);
        foreach (var row in grid) {
            Assert.AreEqual(4, row.Capacity);
        }
    }
}