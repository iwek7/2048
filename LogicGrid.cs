using System;
using System.Collections.Generic;
using System.Reflection.Metadata;

namespace V2;

public delegate void BlockSpawnedHandler(int gridXPosition, int gridYPosition, int blockValue);

public class LogicGrid {
    private List<List<Cell>> _grid = new();
    private Random _rng = new Random(); // todo: inject

    public event BlockSpawnedHandler BlockSpawned;

    public LogicGrid(int gridDimension) {
        for (var i = 0; i < gridDimension; i++) {
            List<Cell> row = new();
            for (var j = 0; j < gridDimension; j++) {
                row.Add(new Cell(i, j));
            }

            _grid.Add(row);
        }
    }

    public void UpdateWithMove(MoveDirection moveDirection) {
        SpawnNewCell();
    }

    private void SpawnNewCell() {
        var cells = GetEmptyCells();
        if (cells.Count > 0) {
            var newBlock = new Block(2);
            var cell = cells[_rng.Next(cells.Count)];
            cell.assignBlock(newBlock);
            BlockSpawned?.Invoke(cell.XPos, cell.YPos, newBlock.Value);
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
        public int XPos { get; }
        public int YPos { get; }
        public Block Block;

        internal Cell(int xPos, int yPos) {
            XPos = xPos;
            YPos = yPos;
        }

        internal bool IsEmpty() {
            return Block == null;
        }

        // todo: research nullability
        internal void assignBlock(Block block) {
            Block = block;
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