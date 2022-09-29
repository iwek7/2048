using System;
using System.Collections.Generic;
using System.Linq;

namespace Logic;

using Grid = List<List<LogicGrid.Cell>>;

public record GridPosition {
    // todo use uint here
    public int Row { get; init; }
    public int Column { get; init; }
}

public class LogicGrid {
    private Grid _grid = new();
    private Random _rng = new(); // todo: inject


    public LogicGrid(int gridDimension) {
        for (var i = 0; i < gridDimension; i++) {
            List<Cell> row = new();
            for (var j = 0; j < gridDimension; j++) {
                row.Add(new Cell(new GridPosition { Row = i, Column = j }));
            }

            _grid.Add(row);
        }
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
        var moveResult = ApplyMoveWithTransforms(moveDirection, _grid);

        _grid = moveResult.GridAfterMove;

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
        // if no moves occured then this condition was evaluated during previous move, hance checking current moveResult
        if (moveResult.GridChanges.Count > 0 && NoMovesLeft()) {
            moveResult.GridChanges.Add(new NoMovesLeftChange());
        }

        return moveResult.GridChanges;
    }


    private static MoveResult ApplyMoveWithTransforms(MoveDirection moveDirection, Grid grid) {
        return moveDirection switch {
            MoveDirection.Right => ApplyMoveToTheLeft(grid,
                new CompositeGridTransformer(new List<IGridTransformer> {
                    new RightSideGridTransformer(), new RightSideGridTransformer()
                }),
                new CompositeGridTransformer(new List<IGridTransformer> {
                    new LeftSideGridTransformer(), new LeftSideGridTransformer()
                })),
            MoveDirection.Left => ApplyMoveToTheLeft(grid, new IdentityGridTransformer(),
                new IdentityGridTransformer()),
            MoveDirection.Up => ApplyMoveToTheLeft(grid, new LeftSideGridTransformer(), new RightSideGridTransformer()),
            MoveDirection.Down => ApplyMoveToTheLeft(grid, new RightSideGridTransformer(),
                new LeftSideGridTransformer()),
            _ => throw new ArgumentOutOfRangeException(nameof(moveDirection), moveDirection, null)
        };
    }

    // this transforms grid using provided transformer and applies movement to left, later it applies reverse transform to grid
    // todo: both this objects should be part of one aggregating object
    private static MoveResult ApplyMoveToTheLeft(
        Grid grid,
        IGridTransformer gridTransformer,
        IGridTransformer reverseTransformer
    ) {
        List<IGridChange> changes = new();
        var transformedGrid = gridTransformer.TransposeGrid(grid);
        Grid newGrid = new();
        for (var rowIdx = 0; rowIdx < transformedGrid.Count; rowIdx++) {
            List<Cell> newRow = new();
            foreach (var cell in transformedGrid[rowIdx].Where(cell => !cell.IsEmpty())) {
                // try to merge
                if (newRow.Count > 0 && newRow.Last().Block.mergeWith(cell.Block)) {
                    // we need to invoke event using correct values of initial grid, so we need to reverse transposition of positions
                    changes.Add(
                        new BlocksMergedChange {
                            MergedBlockInitialPosition = reverseTransformer.TransposeGridPosition(cell.GridPosition),
                            MergeReceiverPosition =
                                reverseTransformer.TransposeGridPosition(newRow.Last().GridPosition),
                            NewBlockValue = newRow.Last().Block.Value
                        }
                    );
                } else {
                    var newCell = new Cell(new GridPosition {
                        Row = rowIdx,
                        Column = newRow.Count
                    });
                    newCell.assignBlock(cell.Block);
                    newRow.Add(newCell);
                    if (cell.GridPosition != newCell.GridPosition) {
                        // we need to invoke event using correct values of initial grid, so we need to reverse transposition of positions
                        changes.Add(new BlockMovedChange {
                                InitialPosition = reverseTransformer.TransposeGridPosition(cell.GridPosition),
                                TargetPosition = reverseTransformer.TransposeGridPosition(newCell.GridPosition)
                            }
                        );
                    }
                }
            }

            while (newRow.Count < transformedGrid[rowIdx].Count) {
                newRow.Add(new Cell(new GridPosition {
                    Row = rowIdx,
                    Column = newRow.Count
                }));
            }

            newGrid.Add(newRow);
        }

        return new MoveResult {
            GridChanges = changes,
            GridAfterMove = reverseTransformer.TransposeGrid(newGrid)
        };
    }

    private bool NoMovesLeft() {
        return GetEmptyCells().Count == 0 && // if there is space to move then move is obviously possible
               // try moving in every direction and see if any change happens
               ApplyMoveWithTransforms(MoveDirection.Left, _grid).GridChanges.Count == 0 &&
               ApplyMoveWithTransforms(MoveDirection.Right, _grid).GridChanges.Count == 0 &&
               ApplyMoveWithTransforms(MoveDirection.Up, _grid).GridChanges.Count == 0 &&
               ApplyMoveWithTransforms(MoveDirection.Down, _grid).GridChanges.Count == 0;
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
        return (from row in _grid from cell in row where cell.IsEmpty() select cell).ToList();
    }

    internal record MoveResult {
        internal Grid GridAfterMove { get; init; }
        internal List<IGridChange> GridChanges { get; init; }
    }

    internal class Cell {
        public GridPosition GridPosition { get; }
        public Block Block;

        internal Cell(GridPosition gridPosition) {
            GridPosition = gridPosition;
        }

        internal bool IsEmpty() {
            return Block == null;
        }

        // todo: research nullability
        internal void assignBlock(Block block) {
            if (Block != null) {
                throw new ArgumentException($"Trying to put block into not empty grid cell {GridPosition}");
            }

            Block = block;
        }
    }

    internal class Block {
        public int Value { get; private set; }

        // todo: make static constructor for set types of blocks instead
        internal Block(int value) {
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