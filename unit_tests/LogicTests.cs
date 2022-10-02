using Logic;

namespace unit_tests;

public class LogicTests {
    private TestRandom _testRng;

    [SetUp]
    public void Setup() {
        _testRng = new TestRandom();
    }
    
    [Test, TestCaseSource(nameof(MovementTestCases))]
    public void ShouldMoveCell(
        GridPosition initialGridPosition,
        MoveDirection moveDirection,
        GridPosition expectedGridPosition) {
        // given

        // todo: move this to some builder
        List<List<LogicGrid.Cell>> grid = new();
        for (var i = 0; i < 4; i++) {
            List<LogicGrid.Cell> row = new();
            for (var j = 0; j < 4; j++) {
                var newCell = new LogicGrid.Cell(new GridPosition { Row = i, Column = j });
                if (newCell.GridPosition == initialGridPosition) {
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
            initialGridPosition,
            moveChange.InitialPosition
        );
        Assert.AreEqual(
            expectedGridPosition,
            moveChange.TargetPosition
        );

        var spawnChanges = (from change in changes where change is BlockSpawnedChange select change).ToList();
        Assert.AreEqual(1, spawnChanges.Count);
    }

    public static IEnumerable<TestCaseData> MovementTestCases() {
        yield return new TestCaseData(new GridPosition { Row = 0, Column = 0 }, MoveDirection.Right,
            new GridPosition { Row = 0, Column = 3 });
        yield return new TestCaseData(new GridPosition { Row = 0, Column = 0 }, MoveDirection.Down,
            new GridPosition { Row = 3, Column = 0 });
        yield return new TestCaseData(new GridPosition { Row = 0, Column = 3 }, MoveDirection.Left,
            new GridPosition { Row = 0, Column = 0 });
        yield return new TestCaseData(new GridPosition { Row = 3, Column = 0 }, MoveDirection.Up,
            new GridPosition { Row = 0, Column = 0 });
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