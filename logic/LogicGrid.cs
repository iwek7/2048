namespace Logic;

using Grid = List<List<LogicGrid.Cell>>;

public record GridPosition {
    // todo use uint here
    public int Row { get; init; }
    public int Column { get; init; }
}

public class LogicGrid {
    // todo: revert this to private
    public List<List<Cell>> Grid { get; private set; } = new();
    private readonly Random _rng;


    public LogicGrid(int gridDimension, Random rng) {
        _rng = rng;
        for (var i = 0; i < gridDimension; i++) {
            List<Cell> row = new();
            for (var j = 0; j < gridDimension; j++) {
                row.Add(new Cell(new GridPosition { Row = i, Column = j }));
            }

            Grid.Add(row);
        }
    }

    public LogicGrid(List<List<Cell>> grid, Random rng) {
        Grid = grid;
        _rng = rng;
    }

    // todo: somehow prevent calling initialize twice
    public BlockSpawnedChange Initialize() {
        var cellWithSpawn = SpawnNewBlock();
        return new BlockSpawnedChange {
            NewBlockPosition = cellWithSpawn.GridPosition,
            NewBlockValue = cellWithSpawn.Block.Value
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
                NewBlockValue = cellWithSpawn.Block.Value
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
            List<Cell> newRow = new();
            foreach (var cell in grid[rowIdx].Where(cell => !cell.IsEmpty())) {
                // try to merge
                // last condition is to account for situation like 2 2 4, without this check first this will merge into 8 because 2 merges will happen
                // this check does not allow merging one cell twice in one move
                if (newRow.Count > 0 &&
                    newRow.Last().Block.mergeWith(cell.Block)
                    && (from change in changes
                        where change is BlocksMergedChange mergedChange &&
                              mergedChange.MergeReceiverTargetPosition == newRow.Last().GridPosition
                        select change).ToList().Count == 0) {
                    // we need to invoke event using correct values of initial grid, so we need to reverse transposition of positions;

                    // now we need to check if merge receiver was moved, if yes then remove this move from list and instead
                    // use initial position of move as merge receiver initial position
                    // todo: rethink this ugly solution
                    var mergeReceiverMove = (BlockMovedChange)changes.FirstOrDefault(change =>
                        change is BlockMovedChange moveChange &&
                        moveChange.TargetPosition == newRow.Last().GridPosition, null);

                    foreach (var change in changes) {
                        Console.WriteLine(change);
                    }

                    GridPosition mergeReceiverInitialPosition;
                    if (mergeReceiverMove != null) {
                        changes.Remove(mergeReceiverMove);
                        mergeReceiverInitialPosition = mergeReceiverMove.InitialPosition;
                    }
                    else {
                        mergeReceiverInitialPosition = newRow.Last().GridPosition;
                    }

                    changes.Add(
                        new BlocksMergedChange {
                            MergeReceiverInitialPosition = mergeReceiverInitialPosition,
                            MergeReceiverTargetPosition = newRow.Last().GridPosition,
                            MergedBlockInitialPosition = cell.GridPosition,
                            NewBlockValue = newRow.Last().Block.Value
                        }
                    );
                }
                else {
                    var newCell = new Cell(new GridPosition {
                        Row = rowIdx,
                        Column = newRow.Count
                    });
                    newCell.assignBlock(cell.Block);
                    newRow.Add(newCell);
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
            while (newRow.Count < grid[rowIdx].Count) {
                newRow.Add(new Cell(new GridPosition {
                    Row = rowIdx,
                    Column = newRow.Count
                }));
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
    private Cell SpawnNewBlock() {
        var cells = GetEmptyCells();
        if (cells.Count == 0) {
            // todo read some guide which exception to throw
            throw new ArgumentException("Trying spawn new block but there are no empty cells");
        }

        var newBlock = new Block(_rng.NextDouble() <= 0.9 ? 2 : 4);
        var cell = cells[_rng.Next(cells.Count)];
        cell.assignBlock(newBlock);
        return cell;
    }

    private List<Cell> GetEmptyCells() {
        return (from row in Grid from cell in row where cell.IsEmpty() select cell).ToList();
    }

    private void DebugPrintGrid() {
        foreach (var row in Grid) {
            foreach (var col in row) {
                Console.Write($"{col.Block?.Value ?? 0} ");
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

    public class Cell {
        public GridPosition GridPosition { get; }
        public Block Block;

        public Cell(GridPosition gridPosition) {
            GridPosition = gridPosition;
        }

        internal bool IsEmpty() {
            return Block == null;
        }

        // todo: research nullability
        public void assignBlock(Block block) {
            if (Block != null) {
                throw new ArgumentException($"Trying to put block into not empty grid cell {GridPosition}");
            }

            Block = block;
        }
    }

    public class Block {
        public int Value { get; private set; }

        // todo: make static constructor for set types of blocks instead
        public Block(int value) {
            Value = value;
        }

        // true if merge happened
        // false if not
        internal bool mergeWith(Block other) {
            if (other.Value != Value) return false;
            Value += other.Value;
            return true;
        }
    }
}