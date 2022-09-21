using System;
using System.Collections.Generic;

namespace V2;

public class GameGrid
{
    private List<List<Cell>> _grid = new();
    private Random r = new Random();
    
    GameGrid(int gridDimension)
    {
        for(var i= 0 ; i < gridDimension; i++)
        {
            for (var j = 0; j < gridDimension; j++)
            {
                _grid.Add(new List<Cell>());
            }
          
        }
    }

    void SpawnNewCell()
    {
        
    }

    private List<Cell> getEmptyCells()
    {
        for(var i= 0 ; i < _grid.Count; i++)
        {
            for (var j = 0; j < _grid.Count; j++)
            {
                _grid.Add(new List<Cell>());
            }
          
        }
    }
    
    class Cell
    {
        private int? _value = null;

        bool isEmpty()
        {
            return _value == null;
        }
    }
    
    
    
    
}