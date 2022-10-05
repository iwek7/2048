using Logic;

namespace unit_tests;

public class LogicTests {
    [Test]
    public void ShouldMoveCellRight() {
        AssertGrid(
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
            },
            new HashSet<BlocksMergedChange>(),
            1
        );
    }

    [Test]
    public void ShouldMoveCellLeft() {
        AssertGrid(
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
            },
            new HashSet<BlocksMergedChange>(),
            1
        );
    }

    [Test]
    public void ShouldMoveCellDown() {
        AssertGrid(
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
            },
            new HashSet<BlocksMergedChange>(),
            1
        );
    }

    [Test]
    public void ShouldMoveCellUp() {
        AssertGrid(
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
            },
            new HashSet<BlocksMergedChange>(),
            1
        );
    }

    [Test]
    public void ShouldMoveMultipleBlocks() {
        AssertGrid(
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
            },
            new HashSet<BlocksMergedChange>(),
            1
        );
    }

    [Test]
    public void ShouldNotMoveAndSpawnBlocksWhenMovementIsNotPossible() {
        AssertGrid(
            GridBuilder.BuildGrid(new[] {
                2, 2, 4, 2,
                0, 0, 0, 0,
                0, 0, 0, 0,
                0, 0, 0, 0
            }),
            MoveDirection.Up,
            new HashSet<BlockMovedChange>(),
            new HashSet<BlocksMergedChange>(),
            0
        );
    }

    [Test]
    public void ShouldMergeTwoBlocks() {
        AssertGrid(
            GridBuilder.BuildGrid(new[] {
                2, 2, 0, 0,
                0, 0, 0, 0,
                0, 0, 0, 0,
                0, 0, 0, 0
            }),
            MoveDirection.Left,
            new HashSet<BlockMovedChange>(),
            new HashSet<BlocksMergedChange> {
                new() {
                    MergeReceiverTargetPosition = new GridPosition { Row = 0, Column = 0 },
                    MergeReceiverInitialPosition = new GridPosition { Row = 0, Column = 0 },
                    NewBlockValue = BlockValue.Four,
                    MergedBlockInitialPosition = new GridPosition { Row = 0, Column = 1 }
                }
            },
            1
        );
    }

    [Test]
    public void ShouldMergeOnlyTwoBlocksWhenThereAreThreeBlocksInARow() {
        AssertGrid(
            GridBuilder.BuildGrid(new[] {
                2, 2, 2, 0,
                0, 0, 0, 0,
                0, 0, 0, 0,
                0, 0, 0, 0
            }),
            MoveDirection.Left,
            new HashSet<BlockMovedChange> {
                new() {
                    InitialPosition = new GridPosition { Row = 0, Column = 2 },
                    TargetPosition = new GridPosition { Row = 0, Column = 1 }
                }
            },
            new HashSet<BlocksMergedChange> {
                new() {
                    MergeReceiverTargetPosition = new GridPosition { Row = 0, Column = 0 },
                    MergeReceiverInitialPosition = new GridPosition { Row = 0, Column = 0 },
                    NewBlockValue = BlockValue.Four,
                    MergedBlockInitialPosition = new GridPosition { Row = 0, Column = 1 }
                }
            },
            1
        );
    }

    [Test]
    public void ShouldMergeOnlyTwoBlocksWhenThereAreThreeBlocksInARowWithDifferentValues() {
        AssertGrid(
            GridBuilder.BuildGrid(new[] {
                2, 2, 4, 0,
                0, 0, 0, 0,
                0, 0, 0, 0,
                0, 0, 0, 0
            }),
            MoveDirection.Left,
            new HashSet<BlockMovedChange> {
                new() {
                    InitialPosition = new GridPosition { Row = 0, Column = 2 },
                    TargetPosition = new GridPosition { Row = 0, Column = 1 }
                }
            },
            new HashSet<BlocksMergedChange> {
                new() {
                    MergeReceiverTargetPosition = new GridPosition { Row = 0, Column = 0 },
                    MergeReceiverInitialPosition = new GridPosition { Row = 0, Column = 0 },
                    NewBlockValue = BlockValue.Four,
                    MergedBlockInitialPosition = new GridPosition { Row = 0, Column = 1 }
                }
            },
            1
        );
    }

    [Test]
    public void ShouldMergeOnlyTwoBlocksWhenThereAreThreeBlocksInARowWithDifferentValuesAndAllBlocksMove() {
        AssertGrid(
            GridBuilder.BuildGrid(new[] {
                0, 0, 0, 0,
                0, 2, 0, 0,
                0, 2, 0, 0,
                0, 4, 0, 0
            }),
            MoveDirection.Up,
            new HashSet<BlockMovedChange> {
                new() {
                    InitialPosition = new GridPosition { Row = 3, Column = 1 },
                    TargetPosition = new GridPosition { Row = 1, Column = 1 }
                }
            },
            new HashSet<BlocksMergedChange> {
                new() {
                    MergeReceiverTargetPosition = new GridPosition { Row = 0, Column = 1 },
                    MergeReceiverInitialPosition = new GridPosition { Row = 1, Column = 1 },
                    NewBlockValue = BlockValue.Four,
                    MergedBlockInitialPosition = new GridPosition { Row = 2, Column = 1 }
                }
            },
            1
        );
    }

    [Test]
    public void ShouldDoTwoMergesInOneLine() {
        AssertGrid(
            GridBuilder.BuildGrid(new[] {
                2, 0, 0, 0,
                2, 0, 0, 0,
                2, 0, 0, 0,
                2, 0, 0, 0
            }),
            MoveDirection.Up,
            new HashSet<BlockMovedChange>(),
            new HashSet<BlocksMergedChange> {
                new() {
                    MergeReceiverTargetPosition = new GridPosition { Row = 0, Column = 0 },
                    MergeReceiverInitialPosition = new GridPosition { Row = 0, Column = 0 },
                    NewBlockValue = BlockValue.Four,
                    MergedBlockInitialPosition = new GridPosition { Row = 1, Column = 0 }
                },
                new() {
                    MergeReceiverTargetPosition = new GridPosition { Row = 1, Column = 0 },
                    MergeReceiverInitialPosition = new GridPosition { Row = 2, Column = 0 },
                    NewBlockValue = BlockValue.Four,
                    MergedBlockInitialPosition = new GridPosition { Row = 3, Column = 0 }
                }
            },
            1
        );
    }


    [Test]
    public void ShouldNotMergeBlocksOfDifferentValues() {
        AssertGrid(
            GridBuilder.BuildGrid(new[] {
                0, 4, 2, 8,
                0, 8, 8, 16,
                4, 0, 4, 64,
                8, 2, 32, 128
            }),
            MoveDirection.Down,
            new HashSet<BlockMovedChange> {
                new() {
                    InitialPosition = new GridPosition { Row = 0, Column = 1 },
                    TargetPosition = new GridPosition { Row = 1, Column = 1 }
                },
                new() {
                    InitialPosition = new GridPosition { Row = 1, Column = 1 },
                    TargetPosition = new GridPosition { Row = 2, Column = 1 }
                }
            },
            new HashSet<BlocksMergedChange>(),
            1
        );
    }
    
    [Test]
    public void ShouldMakeSureDeveloperIsNotAnIdiotAndGridIsNotChangedInIfConditionPls() {
        // that's hack because it relies on knowing how blocks are spawned internally in logic
        // but I would have to rebuild random interface and mock it in more explicit way to avoid it
        // todo: take a look but probably not worth it
        var rng = new TestRandom {
            PresetRandomInt = 0
        };
        var grid = GridBuilder.BuildGrid(new[] {
            8, 0, 0, 0,
            32, 8, 0, 0,
            0, 8, 0, 0,
            0, 16, 0, 0
        }, rng);
        
        AssertGrid(
            grid,
            MoveDirection.Up,
            new HashSet<BlockMovedChange> {
                new() {
                    InitialPosition = new GridPosition { Row = 3, Column = 1 },
                    TargetPosition = new GridPosition { Row = 1, Column = 1 }
                }
            },
            new HashSet<BlocksMergedChange> {
                new() {
                    MergeReceiverTargetPosition = new GridPosition { Row = 0, Column = 1 },
                    MergeReceiverInitialPosition = new GridPosition { Row = 1, Column = 1 },
                    NewBlockValue = BlockValue.Sixteen,
                    MergedBlockInitialPosition = new GridPosition { Row = 2, Column = 1 }
                }
            },
            1
        );
        
        AssertGrid(
            grid,
            MoveDirection.Left,
            new HashSet<BlockMovedChange>(),
            new HashSet<BlocksMergedChange>(),
            0
        );
    }


    private static void AssertGrid(
        LogicGrid logicGrid,
        MoveDirection moveDirection,
        IReadOnlySet<BlockMovedChange> expectedBlockMoveChanges,
        IReadOnlySet<BlocksMergedChange> expectedMergeChanges,
        int expectedNumberOfSpawns
    ) {
        // when
        var changes = logicGrid.UpdateWithMove(moveDirection);

        // then
        Console.WriteLine("Following test changes were detected");
        foreach (var change in changes) {
            Console.WriteLine(change);
        }

        Console.WriteLine("OVER AND OUT");

        // todo common method for all types?
        var moveChanges = (from change in changes where change is BlockMovedChange select (BlockMovedChange)change)
            .ToHashSet();

        Assert.IsTrue(expectedBlockMoveChanges.SetEquals(moveChanges));

        var mergeChanges = (from change in changes where change is BlocksMergedChange select (BlocksMergedChange)change)
            .ToHashSet();
        Assert.IsTrue(expectedMergeChanges.SetEquals(mergeChanges));

        var spawnChanges = (from change in changes where change is BlockSpawnedChange select (BlockSpawnedChange)change)
            .ToList();
        Assert.AreEqual(expectedNumberOfSpawns, spawnChanges.Count);
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

    internal static LogicGrid BuildGrid(int[] schema) {
        return BuildGrid(schema, new Random());
    }

    internal static LogicGrid BuildGrid(int[] schema, Random rng) {
        Assert.AreEqual(16, schema.Length);

        List<List<LogicGrid.Cell>> grid = new();
        for (var i = 0; i < 4; i++) {
            List<LogicGrid.Cell> row = new();
            for (var j = 0; j < 4; j++) {
                var newCell = new LogicGrid.Cell(new GridPosition { Row = i, Column = j });
                var schemaValue = schema[i * 4 + j];
                if (schemaValue != 0) {
                    newCell.assignBlock(new LogicGrid.Block((BlockValue) schemaValue));
                }

                row.Add(newCell);
            }

            grid.Add(row);
        }

        return new LogicGrid(grid, rng);
    }
}