using Godot;
using System;
using V2;

public partial class App : Control {
    private Button _startGameButton;
    private GameGrid _gameGrid;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        _startGameButton = FindChild("StartGameButton") as Button;
        _startGameButton.Pressed += HandleStartButtonClicked;
        _gameGrid = new GameGrid(4);
        _gameGrid.BlockSpawned += HandleBlockSpawned;
    }
    
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) { }

    public override void _UnhandledInput(InputEvent inputEvent) {
        base._UnhandledInput(inputEvent);
        if (inputEvent is not InputEventKey { Pressed: true } keyboardEvent) return;

        switch (keyboardEvent.Keycode) {
            case Key.Left:
                GD.Print("Move left");
                _gameGrid.UpdateWithMove(MoveDirection.Left);
                break;
            case Key.Up:
                GD.Print("Move up");
                _gameGrid.UpdateWithMove(MoveDirection.Up);
                break;
            case Key.Right:
                GD.Print("Move right");
                _gameGrid.UpdateWithMove(MoveDirection.Right);
                break;
            case Key.Down:
                GD.Print("Move down");
                _gameGrid.UpdateWithMove(MoveDirection.Down);
                break;
        }
    }

    private void HandleStartButtonClicked() {
        GD.Print("Starting game!");
    }

    private void HandleBlockSpawned(int row, int col, int value) {
        GD.Print($"Block in row {row}, column {col} was spawned with value {value}");
    }
}