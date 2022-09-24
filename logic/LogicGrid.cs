using System;
using System.Collections.Generic;
using System.Linq;

namespace Logic;

using Grid = List<List<LogicGrid.Cell>>;

public delegate void BlockSpawnedHandler(GridPosition gridPosition, int blockValue);

public delegate void BlockMovedHandler(GridPosition initialPosition, GridPosition targetPosition);

public delegate void BlockMergeHandler(
    GridPosition mergedBlockInitialPosition,
    GridPosition mergeReceiverPosition,
    int newBlockValue
);

public record GridPosition {
    // todo use uint here
    public int Row { get; init; }
    public int Column { get; init; }
}

public class LogicGrid {
    private Grid _grid = new();
    private Random _rng = new(); // todo: inject

    public event BlockSpawnedHandler BlockSpawned;
    public event BlockMovedHandler BlockMoved;
    public event BlockMergeHandler BlocksMerged;

    public LogicGrid(int gridDimension) {
        for (var i = 0; i < gridDimension; i++) {
            List<Cell> row = new();
            for (var j = 0; j < gridDimension; j++) {
                row.Add(new Cell(new GridPosition { Row = i, Column = j }));
            }
            _grid.Add(row);
        }
    }

    public void UpdateWithMove(MoveDirection moveDirection) {
        switch (moveDirection) {
            case MoveDirection.Right:
                ApplyMove(
                    new CompositeGridTransformer(new List<IGridTransformer> { new RightSideGridTransformer(), new RightSideGridTransformer() }),
                    new CompositeGridTransformer(new List<IGridTransformer> { new LeftSideGridTransformer(), new LeftSideGridTransformer() })
                );
                break;
            case MoveDirection.Left:
                ApplyMove(new IdentityGridTransformer(), new IdentityGridTransformer());
                break;
            case MoveDirection.Up:
                ApplyMove(new LeftSideGridTransformer(), new RightSideGridTransformer());
                break;
            case MoveDirection.Down:
                ApplyMove(new RightSideGridTransformer (), new LeftSideGridTransformer());
                break;
        }

        SpawnNewCell();
    }

    // this transforms grid using provided transformer and applies movement to left, later it applies reverse transform to grid
    // todo: both this objects should be part of one aggregating object
    private void ApplyMove(IGridTransformer gridTransformer, IGridTransformer reverseTransformer) {
        var transformedGrid = gridTransformer.TransposeGrid(_grid);
        Grid newGrid = new();
        for (var rowIdx = 0; rowIdx < transformedGrid.Count; rowIdx++) {
            List<Cell> newRow = new();
            foreach (var cell in transformedGrid[rowIdx].Where(cell => !cell.IsEmpty())) {
                // try to merge
                if (newRow.Count > 0 && newRow.Last().Block.mergeWith(cell.Block)) {
                    // we need to invoke event using correct values of initial grid, so we need to reverse transposition of positions
                    BlocksMerged?.Invoke(
                        reverseTransformer.TransposeGridPosition(cell.GridPosition),
                        reverseTransformer.TransposeGridPosition(newRow.Last().GridPosition),
                        newRow.Last().Block.Value);
                } else {
                    var newCell = new Cell(new GridPosition {
                        Row = rowIdx,
                        Column = newRow.Count
                    });
                    cell.moveBlockTo(newCell);
                    newRow.Add(newCell);
                    // we need to invoke event using correct values of initial grid, so we need to reverse transposition of positions
                    BlockMoved?.Invoke(
                        reverseTransformer.TransposeGridPosition(cell.GridPosition),
                        reverseTransformer.TransposeGridPosition(newCell.GridPosition)
                    );
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

        _grid = reverseTransformer.TransposeGrid(newGrid);
    }

    private void SpawnNewCell() {
        var cells = GetEmptyCells();
        if (cells.Count > 0) {
            var newBlock = new Block(2);
            var cell = cells[_rng.Next(cells.Count)];
            cell.assignBlock(newBlock);
            BlockSpawned?.Invoke(cell.GridPosition, newBlock.Value);
        }
    }

    private List<Cell> GetEmptyCells() {
        return (from row in _grid from cell in row where cell.IsEmpty() select cell).ToList();
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

        // todo: replace this usage with assignBlock, mutability sux
        internal void moveBlockTo(Cell otherCell) {
            if (Block == null) {
                throw new ArgumentException($"Trying to move block from cell that has no block {GridPosition}");
            }

            otherCell.assignBlock(Block);
            Block = null;
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
