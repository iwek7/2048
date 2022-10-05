namespace Logic; 
using Grid = List<List<LogicGrid.Cell>>;

public interface IGridTransformer {
    Grid TransposeGrid(Grid grid);
    GridPosition TransposeGridPosition(GridPosition gridPosition);
}

internal class LeftSideGridTransformer : IGridTransformer {
    public Grid TransposeGrid(Grid grid) {
        Grid newGrid = new();
        for (var colIdx = grid.Count - 1; colIdx >= 0; colIdx--) {
            List<LogicGrid.Cell> newRow = new();
            for (var rowIdx = 0; rowIdx < grid[colIdx].Count; rowIdx++) {
                var newCellGridPosition = new GridPosition {
                    Row = grid.Count - (colIdx + 1),
                    Column = rowIdx
                };
                var newCell = new LogicGrid.Cell(newCellGridPosition);
                if (!grid[rowIdx][colIdx].IsEmpty()) {
                    newCell.assignBlock(grid[rowIdx][colIdx].Block);
                }

                newRow.Add(newCell);
            }

            newGrid.Add(newRow);
        }

        return newGrid;
    }

    // todo somehow unhardcode 4? Make it global variable?
    public GridPosition TransposeGridPosition(GridPosition gridPosition) {
        return new GridPosition { Row = 4 - (gridPosition.Column + 1), Column = gridPosition.Row };
    }
}

internal class RightSideGridTransformer : IGridTransformer {
    public Grid TransposeGrid(Grid grid) {
        Grid newGrid = new();
        for (var colIdx = 0; colIdx < grid.Count; colIdx++) {
            List<LogicGrid.Cell> newRow = new();
            for (var rowIdx = grid[colIdx].Count - 1; rowIdx >= 0; rowIdx--) {
                var newCellGridPosition = new GridPosition {
                    Row = colIdx,
                    Column = grid.Count - (rowIdx + 1)
                };
                var newCell = new LogicGrid.Cell(newCellGridPosition);
                if (!grid[rowIdx][colIdx].IsEmpty()) {
                    newCell.assignBlock(grid[rowIdx][colIdx].Block);
                }

                newRow.Add(newCell);
            }

            newGrid.Add(newRow);
        }

        return newGrid;
    }

    // todo somehow unhardcode 4? Make it global variable?
    public GridPosition TransposeGridPosition(GridPosition gridPosition) {
        return new GridPosition { Row = gridPosition.Column, Column = 4 - (gridPosition.Row + 1) };
    }
}

// applies list of provided transformers in provided order
internal class CompositeGridTransformer : IGridTransformer {
    private readonly List<IGridTransformer> _transformers;

    public CompositeGridTransformer(List<IGridTransformer> transformers) {
        _transformers = transformers;
    }

    public Grid TransposeGrid(Grid grid) {
        return _transformers.Aggregate(grid, (current, transformer) => transformer.TransposeGrid(current));
    }

    public GridPosition TransposeGridPosition(GridPosition gridPosition) {
        return _transformers.Aggregate(gridPosition,
            (current, transformer) => transformer.TransposeGridPosition(current)
        );
    }
}