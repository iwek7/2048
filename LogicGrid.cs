using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;

namespace V2;

public delegate void BlockSpawnedHandler(GridPosition gridPosition, int blockValue);

public delegate void BlockMovedHandler(GridPosition initialPosition, GridPosition targetPosition);

public record GridPosition {
    // todo use uint here
    public int Row { get; init; }
    public int Column { get; init; }
}

public class LogicGrid {
    private List<List<Cell>> _grid = new();
    private Random _rng = new(); // todo: inject

    public event BlockSpawnedHandler BlockSpawned;
    public event BlockMovedHandler BlockMoved;

    public LogicGrid(int gridDimension) {
        for (var i = 0; i < gridDimension; i++) {
            List<Cell> row = new();
            for (var j = 0; j < gridDimension; j++) {
                row.Add(new Cell(
                        new GridPosition {
                            Row = i,
                            Column = j
                        }
                    )
                );
            }

            _grid.Add(row);
        }
    }

    public void UpdateWithMove(MoveDirection moveDirection) {
        SpawnNewCell();

        switch (moveDirection) {
            case MoveDirection.Right:
                break;
            case MoveDirection.Left:
                foreach (var row in _grid) {
                    // we don't move first column anyways
                    var firstFreeCol = -1;
                    for (var colIdx = 0; colIdx < _grid.Count; colIdx++) {
                        if (row[colIdx].IsEmpty()) {
                            firstFreeCol = colIdx;
                        }
                        else {
                            if (firstFreeCol > -1) {
                                var fromCell = row[colIdx];
                                var toCell = row[firstFreeCol];
                                fromCell.moveBlockTo(toCell);
                                firstFreeCol = colIdx; // block was just moved from current column
                                BlockMoved?.Invoke(fromCell.GridPosition, toCell.GridPosition);
                            }
                        }
                    }
                }

                break;
            case MoveDirection.Up:
                break;
            case MoveDirection.Down:
                break;
        }
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

    class Cell {
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

        internal void moveBlockTo(Cell otherCell) {
            if (Block == null) {
                throw new ArgumentException($"Trying to move block from cell that has no block {GridPosition}");
            }

            otherCell.assignBlock(Block);
            Block = null;
        }
    }

    class Block {
        public int Value { get; }

        // todo: make static constructor for set types of blocks instead
        internal Block(int value) {
            this.Value = value;
        }
    }
}