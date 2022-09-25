using System;
using System.Collections.Generic;
using Godot;
using Logic;

public partial class App : Control {
    private const int GridDimension = 4;
    private const string BlockNodeName = "BLOCK";
    private Button _startGameButton;
    private GridContainer _mainGrid;

    private LogicGrid _logicGrid;

    private PackedScene _blockScene;

    private List<BlockNode> _tweenedNodes = new();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        _startGameButton = (Button)FindChild("StartGameButton");
        _mainGrid = (GridContainer)FindChild("MainGrid");
        _startGameButton.Pressed += HandleStartButtonClicked;
        _blockScene = (PackedScene)ResourceLoader.Load("res://Block.tscn");

        _logicGrid = new LogicGrid(GridDimension);
        HandleBlockSpawned(_logicGrid.Initialize());
        HandleBlockSpawned(_logicGrid.Initialize());
    }

    public override void _UnhandledInput(InputEvent inputEvent) {
        base._UnhandledInput(inputEvent);
        if (inputEvent is not InputEventKey { Pressed: true } keyboardEvent) return;

        switch (keyboardEvent.Keycode) {
            case Key.Left:
                GD.Print("Move left");
                HandleGridChanges(_logicGrid.UpdateWithMove(MoveDirection.Left));
                break;
            case Key.Up:
                GD.Print("Move up");
                HandleGridChanges(_logicGrid.UpdateWithMove(MoveDirection.Up));
                break;
            case Key.Right:
                GD.Print("Move right");
                HandleGridChanges(_logicGrid.UpdateWithMove(MoveDirection.Right));
                break;
            case Key.Down:
                GD.Print("Move down");
                HandleGridChanges(_logicGrid.UpdateWithMove(MoveDirection.Down));
                break;
        }
    }

    private void HandleGridChanges(List<IGridChange> gridChanges) {
        foreach (var gridChange in gridChanges) {
            switch (gridChange) {
                case BlockMovedChange moveChange:
                    HandleBlockMoved(moveChange);
                    break;
                case BlocksMergedChange mergeChange:
                    HandleBlocksMerged(mergeChange);
                    break;
                case BlockSpawnedChange spawnChange:
                    HandleBlockSpawned(spawnChange);
                    break;
            }
        }
    }

    private void HandleBlockSpawned(BlockSpawnedChange blockSpawnedChange) {
        GD.Print(
            $"Block in row {blockSpawnedChange.NewBlockPosition.Row}, column {blockSpawnedChange.NewBlockPosition.Column} was spawned with value {blockSpawnedChange.NewBlockValue}");

        var blockNode = createBlockNode();
        var gridCellNode = getGridNode(blockSpawnedChange.NewBlockPosition);
        // Here order is important, we need to first add block to the tree so that _ready is called
        // and children nodes are assigned to block node variables.
        gridCellNode.AddChild(blockNode);
        blockNode.Resize(((ColorRect)gridCellNode.FindChild("ColorRect")).Size);
        blockNode.Value = blockSpawnedChange.NewBlockValue;
    }

    private void HandleBlockMoved(BlockMovedChange blockMovedChange) {
        GD.Print($"Moving block from {blockMovedChange.InitialPosition} to {blockMovedChange.TargetPosition}");
        // remove node from one place
        var initialGridNode = getGridNode(blockMovedChange.InitialPosition);
        var blockToMove = initialGridNode.GetNode<BlockNode>(BlockNodeName);
        initialGridNode.RemoveChild(blockToMove);
        
        var targetGridNode = getGridNode(blockMovedChange.TargetPosition);
        
        // to show movement one big hack is used
        // it could be so much nicer without this grid construct (maybe) and using regular nodes drawn on canvas
        // here we remove block from on grid, add it to app root to tween it to new position so that nice movement is shown
        blockToMove.Position = initialGridNode.GlobalPosition;
        AddChild(blockToMove);
        var tween = CreateTween();
        tween.TweenProperty(blockToMove, "position", targetGridNode.GlobalPosition, 0.1f);
        
        tween.Finished += () => {
            RemoveChild(blockToMove);
            blockToMove.Position = Vector2.Zero;
            // when adding blocks to temporary common parent (for tweening) they will get renamed
            // so update their name once more here when they got assigned to grids
            blockToMove.Name = BlockNodeName;
            targetGridNode.AddChild(blockToMove);
        };
    }

    private void HandleBlocksMerged(BlocksMergedChange blocksMergedChange) {
        // remove one node
        var initialGridNode = getGridNode(blocksMergedChange.MergedBlockInitialPosition);
        var blockToMove = initialGridNode.GetNode<BlockNode>(BlockNodeName);
        initialGridNode.RemoveChild(blockToMove);
        blockToMove.QueueFree();

        // and update the other
        var blockToMergeTo = getGridNode(blocksMergedChange.MergeReceiverPosition).GetNode<BlockNode>(BlockNodeName);
        blockToMergeTo.Value = blocksMergedChange.NewBlockValue;
    }

    private void HandleStartButtonClicked() {
        GD.Print("Starting game!");
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