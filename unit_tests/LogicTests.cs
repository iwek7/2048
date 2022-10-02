using Logic;

namespace unit_tests;

public class LogicTests {
    private TestRandom _testRng;

    [SetUp]
    public void Setup() {
        _testRng = new TestRandom();
    }

    [Test]
    public void ShouldMoveCellRight() {
        assertGrid(
            GridBuilder.BuildGrid(new[] {
                2, 0, 0, 0,
                0, 0, 0, 0,
                0, 0, 0, 0,
                0, 0, 0, 0
            }),
            MoveDirection.Right,
            new HashSet<BlockMovedChange> {
                new() {
                    InitialPosition = new GridPosition { Row = 0, Column = 0 },
                    TargetPosition = new GridPosition { Row = 0, Column = 3 }
                }
            }
        );
    }

    [Test]
    public void ShouldMoveCellLeft() {
        assertGrid(
            GridBuilder.BuildGrid(new[] {
                0, 0, 0, 2,
                0, 0, 0, 0,
                0, 0, 0, 0,
                0, 0, 0, 0
            }),
            MoveDirection.Left,
            new HashSet<BlockMovedChange> {
                new() {
                    InitialPosition = new GridPosition { Row = 0, Column = 3 },
                    TargetPosition = new GridPosition { Row = 0, Column = 0 }
                }
            }
        );
    }

    [Test]
    public void ShouldMoveCellDown() {
        assertGrid(
            GridBuilder.BuildGrid(new[] {
                2, 0, 0, 0,
                0, 0, 0, 0,
                0, 0, 0, 0,
                0, 0, 0, 0
            }),
            MoveDirection.Down,
            new HashSet<BlockMovedChange> {
                new() {
                    InitialPosition = new GridPosition { Row = 0, Column = 0 },
                    TargetPosition = new GridPosition { Row = 3, Column = 0 }
                }
            }
        );
    }

    [Test]
    public void ShouldMoveCellUp() {
        assertGrid(
            GridBuilder.BuildGrid(new[] {
                0, 0, 0, 0,
                0, 0, 0, 0,
                0, 0, 0, 0,
                2, 0, 0, 0
            }),
            MoveDirection.Up,
            new HashSet<BlockMovedChange> {
                new() {
                    InitialPosition = new GridPosition { Row = 3, Column = 0 },
                    TargetPosition = new GridPosition { Row = 0, Column = 0 }
                }
            }
        );
    }

    [Test]
    public void ShouldMoveMultipleBlocks() {
        assertGrid(
            GridBuilder.BuildGrid(new[] {
                0, 0, 0, 0,
                0, 0, 0, 0,
                0, 0, 0, 0,
                2, 2, 0, 0
            }),
            MoveDirection.Up,
            new HashSet<BlockMovedChange> {
                new() {
                    InitialPosition = new GridPosition { Row = 3, Column = 0 },
                    TargetPosition = new GridPosition { Row = 0, Column = 0 }
                },
                new() {
                    InitialPosition = new GridPosition { Row = 3, Column = 1 },
                    TargetPosition = new GridPosition { Row = 0, Column = 1 }
                }
            }
        );
    }

    private void assertGrid(
        List<List<LogicGrid.Cell>> grid,
        MoveDirection moveDirection,
        IReadOnlySet<BlockMovedChange> expectedBlockMoveChanges) {
        // given
        var logicGrid = new LogicGrid(grid, _testRng);

        // when
        var changes = logicGrid.UpdateWithMove(moveDirection);

        // then
        // todo common method for all types?
        var moveChanges = (from change in changes where change is BlockMovedChange select (BlockMovedChange)change)
            .ToHashSet();
        Assert.IsTrue(expectedBlockMoveChanges.SetEquals(moveChanges));

        var spawnChanges = (from change in changes where change is BlockSpawnedChange select (BlockSpawnedChange)change)
            .ToList();
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

internal static class GridBuilder {
    internal static List<List<LogicGrid.Cell>> BuildGrid(int[] schema) {
        Assert.AreEqual(16, schema.Length);

        List<List<LogicGrid.Cell>> grid = new();
        for (var i = 0; i < 4; i++) {
            List<LogicGrid.Cell> row = new();
            for (var j = 0; j < 4; j++) {
                var newCell = new LogicGrid.Cell(new GridPosition { Row = i, Column = j });
                var schemaValue = schema[i * 4 + j];
                if (schemaValue != 0) {
                    newCell.assignBlock(new LogicGrid.Block(schemaValue));
                }

                row.Add(newCell);
            }

            grid.Add(row);
        }

        return grid;
    }
}