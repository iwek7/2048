using Godot;
using V2;

public partial class App : Control {
    private const int GridDimension = 4;
    private Button _startGameButton;
    private GridContainer _mainGrid;

    private LogicGrid _logicGrid;

    private PackedScene _blockScene;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        _startGameButton = FindChild("StartGameButton") as Button;
        _mainGrid = FindChild("MainGrid") as GridContainer;
        _startGameButton.Pressed += HandleStartButtonClicked;
        _logicGrid = new LogicGrid(GridDimension);
        _logicGrid.BlockSpawned += HandleBlockSpawned;

        _blockScene = (PackedScene)ResourceLoader.Load("res://Block.tscn");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) { }

    public override void _UnhandledInput(InputEvent inputEvent) {
        base._UnhandledInput(inputEvent);
        if (inputEvent is not InputEventKey { Pressed: true } keyboardEvent) return;

        switch (keyboardEvent.Keycode) {
            case Key.Left:
                GD.Print("Move left");
                _logicGrid.UpdateWithMove(MoveDirection.Left);
                break;
            case Key.Up:
                GD.Print("Move up");
                _logicGrid.UpdateWithMove(MoveDirection.Up);
                break;
            case Key.Right:
                GD.Print("Move right");
                _logicGrid.UpdateWithMove(MoveDirection.Right);
                break;
            case Key.Down:
                GD.Print("Move down");
                _logicGrid.UpdateWithMove(MoveDirection.Down);
                break;
        }
    }

    private void HandleStartButtonClicked() {
        GD.Print("Starting game!");
    }

    private void HandleBlockSpawned(int row, int col, int value) {
        GD.Print($"Block in row {row}, column {col} was spawned with value {value}");
        var blockNode = (BlockNode)_blockScene.Instantiate();
        // here order is important
        _mainGrid.GetChild(row + GridDimension * col).AddChild(blockNode);
        blockNode.Value = value;
    }
}