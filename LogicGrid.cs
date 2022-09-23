using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;

namespace V2;

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
    private List<List<Cell>> _grid = new();
    private Random _rng = new(); // todo: inject

    public event BlockSpawnedHandler BlockSpawned;
    public event BlockMovedHandler BlockMoved;
    public event BlockMergeHandler BlocksMerged;

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
        switch (moveDirection) {
            case MoveDirection.Right:
                foreach (var row in _grid) {
                    // this is pointer to first free cell (without block) in a row
                    // -1 value means there is no free cell
                    var firstFreeCol = -1;
                    for (var colIdx = _grid.Count - 1; colIdx >= 0; colIdx--) {
                        if (row[colIdx].IsEmpty()) {
                            if (firstFreeCol == -1) {
                                firstFreeCol = colIdx;
                            }
                        }
                        else {
                            var fromCell = row[colIdx];
                            // handle merge
                            // check if cell before one that is free to move to has matching number
                            // if yes merge happens
                            if (colIdx < _grid.Count - 1) {
                                // find nearest cell
                                Cell mergeCandidateCell = null;
                                for (var i = colIdx + 1; i < _grid.Count; i++) {
                                    if (!row[i].IsEmpty()) {
                                        mergeCandidateCell = row[i];
                                        break;
                                    }
                                }

                                if (mergeCandidateCell != null && mergeCandidateCell.Block.mergeWith(fromCell.Block)) {
                                    // no need to move firstFreeCol pointer since nothing was moved in this iteration
                                    fromCell.Block = null;
                                    BlocksMerged?.Invoke(fromCell.GridPosition, mergeCandidateCell.GridPosition,
                                        mergeCandidateCell.Block.Value);
                                }
                                else if (firstFreeCol > -1) {
                                    // here we handle ordinary move
                                    var toCell = row[firstFreeCol];
                                    fromCell.moveBlockTo(toCell);
                                    firstFreeCol--; // block was just moved from current column, so index of first free colum is one higher
                                    BlockMoved?.Invoke(fromCell.GridPosition, toCell.GridPosition);
                                }
                            }
                        }
                    }
                }

                break;
            case MoveDirection.Left:
                List<List<Cell>> newGrid = new();
                for (var rowIdx = 0; rowIdx < _grid.Count; rowIdx++) {
                    var row = _grid[rowIdx];
                    List<Cell> newRow = new();
                    for (var colIdx = 0; colIdx < _grid.Count; colIdx++) {
                        var cellToCheck = row[colIdx];
                        if (cellToCheck.IsEmpty()) {
                            continue;
                        }

                        // try to merge
                        if (newRow.Count > 0 && newRow.Last().Block.mergeWith(cellToCheck.Block)) {
                            BlocksMerged?.Invoke(
                                cellToCheck.GridPosition,
                                newRow.Last().GridPosition,
                                newRow.Last().Block.Value);
                        }
                        else {
                            var newCell = new Cell(new GridPosition {
                                Row = rowIdx,
                                Column = colIdx
                            });
                            cellToCheck.moveBlockTo(newCell);
                            newRow.Add(newCell);
                            BlockMoved?.Invoke(cellToCheck.GridPosition, newCell.GridPosition);
                        }
                    }

                    while (row.Count < 4) {
                        
                    }
                }

                _grid = newGrid;
                break;
            case MoveDirection.Up:
                break;
            case MoveDirection.Down:
                break;
        }

        SpawnNewCell();
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