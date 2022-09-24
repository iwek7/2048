using Godot;
using Logic;

public partial class App : Control {
    private const int GridDimension = 4;
    private const string BlockNodeName = "BLOCK";
    private Button _startGameButton;
    private GridContainer _mainGrid;

    private LogicGrid _logicGrid;

    private PackedScene _blockScene;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        _startGameButton = (Button)FindChild("StartGameButton");
        _mainGrid = (GridContainer)FindChild("MainGrid");
        _startGameButton.Pressed += HandleStartButtonClicked;

        _logicGrid = new LogicGrid(GridDimension);
        _logicGrid.BlockSpawned += HandleBlockSpawned;
        _logicGrid.BlockMoved += HandleBlockMoved;
        _logicGrid.BlocksMerged += HandleBlocksMerged;

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

    private void HandleBlockSpawned(GridPosition gridPosition, int value) {
        GD.Print($"Block in row {gridPosition.Row}, column {gridPosition.Column} was spawned with value {value}");

        var blockNode = createBlockNode();
        var gridCellNode = getGridNode(gridPosition);
        // Here order is important, we need to first add block to the tree so that _ready is called
        // and children nodes are assigned to block node variables.
        gridCellNode.AddChild(blockNode);
        blockNode.Resize(((ColorRect)gridCellNode.FindChild("ColorRect")).Size);
        blockNode.Value = value;
    }

    private void HandleBlockMoved(GridPosition initialPosition, GridPosition targetPosition) {
        GD.Print($"Moving block from {initialPosition} to {targetPosition}");
        // remove node from one place
        var initialGridNode = getGridNode(initialPosition);
        var blockToMove = initialGridNode.GetNode<BlockNode>(BlockNodeName);
        initialGridNode.RemoveChild(blockToMove);

        // and move it to the other
        var targetGridNode = getGridNode(targetPosition);
        targetGridNode.AddChild(blockToMove);
    }

    private void HandleBlocksMerged(
        GridPosition mergedBlockInitialPosition,
        GridPosition mergeReceiverPosition,
        int newBlockValue
    ) {
        // remove one node
        var initialGridNode = getGridNode(mergedBlockInitialPosition);
        var blockToMove = initialGridNode.GetNode<BlockNode>(BlockNodeName);
        initialGridNode.RemoveChild(blockToMove);
        blockToMove.QueueFree();

        // and update the other
        var blockToMergeTo = getGridNode(mergeReceiverPosition).GetNode<BlockNode>(BlockNodeName);
        blockToMergeTo.Value = newBlockValue;
    }

    private Control getGridNode(GridPosition gridPosition) {
        return (Control)_mainGrid.GetChild(gridPosition.Row * GridDimension + gridPosition.Column);
    }

    private BlockNode createBlockNode() {
        var blockNode = (BlockNode)_blockScene.Instantiate();
        blockNode.Name = BlockNodeName;
        return blockNode;
    }
}