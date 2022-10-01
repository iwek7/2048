using Logic;

namespace unit_tests;

public class Tests {
    private TestRandom _testRng;

    [SetUp]
    public void Setup() {
        _testRng = new TestRandom();
    }
    
    [TestCase(0, 0, MoveDirection.Right, 0, 3)]
    [TestCase(0, 0, MoveDirection.Down, 3, 0)]
    [TestCase(0, 3, MoveDirection.Left, 0, 0)]
    [TestCase(3, 0, MoveDirection.Up, 0, 0)]
    public void ShouldMoveCell(int initialGridRow, int initialGridCol,
        MoveDirection moveDirection, int expectedGridRow, int expectedGridCol) {
        // given

        // todo: move this to some builder
        List<List<LogicGrid.Cell>> grid = new();
        for (var i = 0; i < 4; i++) {
            List<LogicGrid.Cell> row = new();
            for (var j = 0; j < 4; j++) {
                var newCell = new LogicGrid.Cell(new GridPosition { Row = i, Column = j });
                if (i == initialGridRow && j == initialGridCol) {
                    newCell.assignBlock(new LogicGrid.Block(2));
                }

                row.Add(newCell);
            }

            grid.Add(row);
        }

        var logicGrid = new LogicGrid(grid, _testRng);

        // when
        var changes = logicGrid.UpdateWithMove(moveDirection);

        // then
        Assert.AreEqual(2, changes.Count);
        // todo is it possible to return list of move changes here?, at least create helper method
        var moveChanges = (from change in changes where change is BlockMovedChange select change).ToList();
        Assert.AreEqual(1, moveChanges.Count);

        var moveChange = (BlockMovedChange)moveChanges[0];
        Assert.AreEqual(
            new GridPosition { Row = initialGridRow, Column = initialGridCol },
            moveChange.InitialPosition
        );
        Assert.AreEqual(
            new GridPosition { Row = expectedGridRow, Column = expectedGridCol },
            moveChange.TargetPosition
        );

        var spawnChanges = (from change in changes where change is BlockSpawnedChange select change).ToList();
        Assert.AreEqual(1, spawnChanges.Count);
    }
}

internal class TestRandom : Random {
    internal int? PresetRandomInt { get; set; } = null;
    internal double? PresetRandomDouble { get; set; } = null;

    // todo validations
    public override int Next(int range) {
        return PresetRandomInt ?? base.Next(range);
    }

    public override double NextDouble() {
        return PresetRandomDouble ?? base.NextDouble();
    }
}

// internal class GridBuilder {
//
//     static List<List<LogicGrid.Cell>> BuildGridWithBlock(LogicGrid.Block block) {
//         
//     }
//     
// }