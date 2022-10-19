namespace Logic;

// todo: is it possible to make this alias global to use it in unit tests and other files?
// todo: Right now there is code duplication between this place and transformers file
using Grid = List<List<LogicGrid.ICell>>;

public record GridPosition {
    public int Row { get; init; }
    public int Column { get; init; }
}

public class LogicGrid {
    // todo: revert this to private
    private Grid Grid { get; set; } = new();
    private readonly Random _rng;

    public LogicGrid(int gridDimension, Random rng) {
        _rng = rng;
        for (var i = 0; i < gridDimension; i++) {
            List<ICell> row = new();
            for (var j = 0; j < gridDimension; j++) {
                row.Add(new EmptyCell { GridPosition = new GridPosition { Row = i, Column = j } });
            }

            Grid.Add(row);
        }
    }

    public LogicGrid(Grid grid, Random rng) {
        Grid = grid;
        _rng = rng;
    }

    // todo: somehow prevent calling initialize twice
    public BlockSpawnedChange Initialize() {
        var cellWithSpawn = SpawnNewBlock();
        return new BlockSpawnedChange {
            NewBlockPosition = cellWithSpawn.GridPosition,
            NewBlockValue = cellWithSpawn.BlockValue
        };
    }

    public List<IGridChange> UpdateWithMove(MoveDirection moveDirection) {
        var moveResult = ApplyMoveWithTransforms(moveDirection, Grid);
        Console.WriteLine("Grid before changes:");
        DebugPrintGrid();
        Grid = moveResult.GridAfterMove;

        // new cell is spawned only if move actually moves something
        // it also ensures that there is free space in grid
        if (moveResult.GridChanges.Count > 0) {
            var cellWithSpawn = SpawnNewBlock();
            moveResult.GridChanges.Add(new BlockSpawnedChange {
                NewBlockPosition = cellWithSpawn.GridPosition,
                // ReSharper disable once PossibleInvalidOperationException
                // We just spawned cell with value so that it will never be null.
                NewBlockValue = (BlockValue)cellWithSpawn.BlockValue
            });
        }

        // check lose condition
        // if no moves occured then this condition was evaluated during previous move, hence checking current moveResult
        if (moveResult.GridChanges.Count > 0 && NoMovesLeft()) {
            moveResult.GridChanges.Add(new NoMovesLeftChange());
        }

        Console.WriteLine("Grid after changes:");
        DebugPrintGrid();

        return moveResult.GridChanges;
    }

    // todo: move static stuff away from here
    private static readonly IGridTransformer RightSideTransformer = new RightSideGridTransformer();
    private static readonly IGridTransformer LeftSideGridTransformer = new LeftSideGridTransformer();

    private static MoveResult ApplyMoveWithTransforms(MoveDirection moveDirection, Grid grid) {
        switch (moveDirection) {
            case MoveDirection.Right:
                // todo: fix useless transformer allocation
                var transformer = new CompositeGridTransformer(new List<IGridTransformer> {
                    RightSideTransformer, RightSideTransformer
                });
                var reverseTransformer = new CompositeGridTransformer(new List<IGridTransformer> {
                    LeftSideGridTransformer, LeftSideGridTransformer
                });
                return ApplyMoveToTheLeft(transformer.TransposeGrid(grid)).Transpose(reverseTransformer);
            case MoveDirection.Left:
                return ApplyMoveToTheLeft(grid);
            case MoveDirection.Up:
                return ApplyMoveToTheLeft(LeftSideGridTransformer.TransposeGrid(grid)).Transpose(RightSideTransformer);
            case MoveDirection.Down:
                return ApplyMoveToTheLeft(RightSideTransformer.TransposeGrid(grid)).Transpose(LeftSideGridTransformer);
            default:
                throw new ArgumentOutOfRangeException(nameof(moveDirection), moveDirection, null);
        }
    }

    // this transforms grid using provided transformer and applies movement to left, later it applies reverse transform to grid
    // all inputs to this method should be already transformed since it only applies move to the left
    // likewise returned input is left site oriented
    // todo: both this objects should be part of one aggregating object
    private static MoveResult ApplyMoveToTheLeft(Grid grid) {
        List<IGridChange> changes = new();
        Grid newGrid = new();
        for (var rowIdx = 0; rowIdx < grid.Count; rowIdx++) {
            List<CellWithBlock> newFilledRow = new();
            foreach (var cell in from cell in grid[rowIdx] where cell is CellWithBlock select (CellWithBlock)cell) {
                // try to merge
                // last condition is to account for situation like 2 2 4, without this check first this will merge into 8 because 2 merges will happen
                // this check does not allow merging one cell twice in one move
                if (newFilledRow.Count > 0 &&
                    newFilledRow.Last().BlockValue == cell.BlockValue
                    && (from change in changes
                        where change is BlocksMergedChange mergedChange &&
                              mergedChange.MergeReceiverTargetPosition == newFilledRow.Last().GridPosition
                        select change).ToList().Count == 0) {
                    // replace last added block with merged one
                    var mergedCell = newFilledRow.Last() with { BlockValue = (BlockValue)((int)cell.BlockValue * 2) };
                    newFilledRow.RemoveAt(newFilledRow.Count - 1);
                    newFilledRow.Add(mergedCell);

                    // now we need to check if merge receiver was moved, if yes then remove this move from list and instead
                    // use initial position of move as merge receiver initial position
                    // todo: rethink this ugly solution
                    var mergeReceiverMove = (BlockMovedChange)changes.FirstOrDefault(change =>
                        change is BlockMovedChange moveChange &&
                        moveChange.TargetPosition == newFilledRow.Last().GridPosition, null);

                    foreach (var change in changes) {
                        Console.WriteLine(change);
                    }

                    GridPosition mergeReceiverInitialPosition;
                    if (mergeReceiverMove != null) {
                        changes.Remove(mergeReceiverMove);
                        mergeReceiverInitialPosition = mergeReceiverMove.InitialPosition;
                    } else {
                        mergeReceiverInitialPosition = newFilledRow.Last().GridPosition;
                    }

                    changes.Add(
                        new BlocksMergedChange {
                            MergeReceiverInitialPosition = mergeReceiverInitialPosition,
                            MergeReceiverTargetPosition = newFilledRow.Last().GridPosition,
                            MergedBlockInitialPosition = cell.GridPosition,
                            // ReSharper disable once PossibleInvalidOperationException
                            // Block value here is never null because new row contains only cells with not null block values.
                            // This possible to ensure this is never using type system but probably not worth the complication.
                            NewBlockValue = newFilledRow.Last().BlockValue
                        }
                    );
                } else {
                    var newCell = cell with {
                        GridPosition = new GridPosition {
                            Row = rowIdx,
                            Column = newFilledRow.Count
                        }
                    };
                    newFilledRow.Add(newCell);
                    if (cell.GridPosition != newCell.GridPosition) {
                        // we need to invoke event using correct values of initial grid, so we need to reverse transposition of positions
                        changes.Add(new BlockMovedChange {
                                InitialPosition = cell.GridPosition,
                                TargetPosition = newCell.GridPosition
                            }
                        );
                    }
                }
            }

            // fill new row with empty cells
            var newRow = newFilledRow.Cast<ICell>().ToList();
            while (newRow.Count < grid[rowIdx].Count) {
                newRow.Add(new EmptyCell {
                    GridPosition = new GridPosition {
                        Row = rowIdx,
                        Column = newRow.Count
                    }
                });
            }

            newGrid.Add(newRow);
        }

        return new MoveResult {
            GridChanges = changes,
            GridAfterMove = newGrid
        };
    }

    private bool NoMovesLeft() {
        return GetEmptyCells().Count == 0 && // if there is space to move then move is obviously possible
               // try moving in every direction and see if any change happens
               ApplyMoveWithTransforms(MoveDirection.Left, Grid).GridChanges.Count == 0 &&
               ApplyMoveWithTransforms(MoveDirection.Right, Grid).GridChanges.Count == 0 &&
               ApplyMoveWithTransforms(MoveDirection.Up, Grid).GridChanges.Count == 0 &&
               ApplyMoveWithTransforms(MoveDirection.Down, Grid).GridChanges.Count == 0;
    }


    // it returns cell in which block was spawned
    private CellWithBlock SpawnNewBlock() {
        var emptyCells = GetEmptyCells();
        if (emptyCells.Count == 0) {
            // todo read some guide which exception to throw
            throw new ArgumentException("Trying spawn new block but there are no empty cells");
        }

        var newBlockValue = _rng.NextDouble() <= 0.9 ? BlockValue.Two : BlockValue.Four;
        var cellToSpawnBlockIn = emptyCells[_rng.Next(emptyCells.Count)];
        var cellWithBlock = cellToSpawnBlockIn.withBlockValue(newBlockValue);
        Grid[cellToSpawnBlockIn.GridPosition.Row][cellToSpawnBlockIn.GridPosition.Column] = cellWithBlock;
        return cellWithBlock;
    }

    private List<EmptyCell> GetEmptyCells() {
        return (from row in Grid from cell in row where cell is EmptyCell select (EmptyCell)cell).ToList();
    }

    private void DebugPrintGrid() {
        foreach (var row in Grid) {
            foreach (var cell in row) {
                var blockValue = cell is CellWithBlock cellWithBlock ? (int)cellWithBlock.BlockValue : 0;
                Console.Write($"{blockValue} ");
            }

            Console.Write("\n");
        }
    }

    private record MoveResult {
        internal Grid GridAfterMove { get; init; }
        internal List<IGridChange> GridChanges { get; init; }

        internal MoveResult Transpose(IGridTransformer gridTransformer) {
            return new MoveResult {
                GridAfterMove = gridTransformer.TransposeGrid(GridAfterMove),
                GridChanges = (from change in GridChanges select change.Transpose(gridTransformer)).ToList()
            };
        }
    }

    public interface ICell {
        public GridPosition GridPosition { get; }
        public ICell WithPosition(GridPosition position);
    }

    public record EmptyCell : ICell {
        public GridPosition GridPosition { get; init; }

        public CellWithBlock withBlockValue(BlockValue blockValue) {
            return new CellWithBlock { GridPosition = GridPosition, BlockValue = blockValue };
        }

        public ICell WithPosition(GridPosition position) {
            return new EmptyCell { GridPosition = position };
        }
    }

    public record CellWithBlock : ICell {
        public GridPosition GridPosition { get; init; }

        public BlockValue BlockValue { get; init; }

        public ICell WithPosition(GridPosition position) {
            return this with { GridPosition = position };
        }
    }
}