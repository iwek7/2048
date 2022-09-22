using System;
using System.Collections.Generic;
using System.Reflection.Metadata;

namespace V2;

public delegate void BlockSpawnedHandler(GridPosition gridPosition, int blockValue);

public delegate void BlockMovedHandler(GridPosition initialPosition, GridPosition newPosition);

public record GridPosition {
    // todo use uint here
    public int XPos { get; init; }
    public int YPos { get; init; }
}

public class LogicGrid {
    private List<List<Cell>> _grid = new();
    private Random _rng = new Random(); // todo: inject

    public event BlockSpawnedHandler BlockSpawned;
    public event BlockMovedHandler BlockMoved;

    public LogicGrid(int gridDimension) {
        for (var i = 0; i < gridDimension; i++) {
            List<Cell> row = new();
            for (var j = 0; j < gridDimension; j++) {
                row.Add(new Cell(
                        new GridPosition {
                            XPos = i,
                            YPos = j
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
                for (var rowIdx = 0; rowIdx < _grid.Count; rowIdx++) {
                    getFirstFreeCellOfRow(rowIdx);
                    // we don't move first column anyways
                    var firstFreeCol = -1;
                    for (var colIdx = 0; rowIdx < _grid.Count; rowIdx++) {
                        if (_grid[rowIdx][colIdx].IsEmpty()) {
                            firstFreeCol = colIdx;
                        }
                        else {
                            if (firstFreeCol > -1) {
                                var fromCell = _grid[rowIdx][colIdx];
                                var toCell = _grid[rowIdx][firstFreeCol];
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

    // returns null if no cell is free
    private Cell getFirstFreeCellOfRow(int rowIdx) {
        for (var colIdx = 0; colIdx < _grid[rowIdx].Count; colIdx++) {
            if (_grid[rowIdx][colIdx].IsEmpty()) {
                return _grid[rowIdx][colIdx];
            }
        }

        return null;
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
        List<Cell> emptyCells = new();
        foreach (var row in _grid) {
            for (var j = 0; j < _grid.Count; j++) {
                if (row[j].IsEmpty()) {
                    emptyCells.Add(row[j]);
                }
            }
        }

        return emptyCells;
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