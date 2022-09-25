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

    // todo: spawn should happen after tween movement
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
        targetGridNode.AddChild(blockToMove);
        
        // it will be shown at the end of the tween
        blockToMove.Visible = false;
        
        // tween block to show movement of blocks
        var blockToTween = createBlockNode();
        // todo: take into account margin
        blockToTween.Position = initialGridNode.GlobalPosition;
        AddChild(blockToTween);
        blockToTween.Value = blockToMove.Value;
        // todo: civilize this
        blockToTween.Resize(((ColorRect)targetGridNode.FindChild("ColorRect")).Size);
        
        var tween = CreateTween();
        tween.TweenProperty(blockToTween, "position", targetGridNode.GlobalPosition, 0.1f);
        
        tween.Finished += () => {
            RemoveChild(blockToTween);
            blockToMove.Visible = true;
        };
    }

    // todo: movement here should use tween
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