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
    private ScoreKeeper _scoreKeeper;

    private Banner _banner;

    // allow next move only when all animations of previous move finished
    private int running = 0;
    private const float MoveAnimationTime = 0.2f;
    private const float SpawnAnimationTime = 0.15f;
    
    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        _startGameButton = (Button)FindChild("RestartGameButton");
        _mainGrid = (GridContainer)FindChild("MainGrid");
        _startGameButton.Pressed += HandleRestartButtonClicked;
        _blockScene = (PackedScene)ResourceLoader.Load("res://Block.tscn");
        _scoreKeeper = (ScoreKeeper)FindChild("ScoreLabelContainer");

        _banner = (Banner)FindChild("Banner");
        _banner.Visible = false;
        _banner.RestartButtonClicked += HandleRestartButtonClicked;
        _banner.QuitButtonClicked += () => { GetTree().Quit(); };

        // only for testing
        // todo: remove
        ((Button)FindChild("TestLoseButton")).Pressed += () => { _banner.Show(false, _scoreKeeper.Score); };
        ((Button)FindChild("TestWinButton")).Pressed += () => { _banner.Show(true, _scoreKeeper.Score); };

        _logicGrid = new LogicGrid(GridDimension);

        HandleBlockSpawned(_logicGrid.Initialize());
    }

    public override void _UnhandledInput(InputEvent inputEvent) {
        base._UnhandledInput(inputEvent);
        if (inputEvent is not InputEventKey { Pressed: true } keyboardEvent) return;
        if (running > 0) return;
        
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
                case NoMovesLeftChange noMovesLeftChange:
                    HandleNoMovesLeft(noMovesLeftChange);
                    break;
            }
        }
    }

    private void HandleBlockSpawned(BlockSpawnedChange blockSpawnedChange) {
        GD.Print(
            $"Block in row {blockSpawnedChange.NewBlockPosition.Row}, column {blockSpawnedChange.NewBlockPosition.Column}" +
            $" was spawned with value {blockSpawnedChange.NewBlockValue}"
        );

        var blockNode = CreateBlockNode();
        var gridCellNode = GetGridNode(blockSpawnedChange.NewBlockPosition);
        // Here order is important, we need to first add block to the tree so that _ready is called
        // and children nodes are assigned to block node variables.
        gridCellNode.AddChild(blockNode);
        blockNode.Value = blockSpawnedChange.NewBlockValue;
        blockNode.CustomMinimumSize = (GetInnerGridField(gridCellNode).Size / 2).ToVector2I();

        var tween = CreateTween();
        tween.TweenProperty(blockNode, "custom_minimum_size", GetInnerGridField(gridCellNode).Size.ToVector2I(), SpawnAnimationTime);
        running++;
        tween.Finished += () => {
            blockNode.SizeFlagsHorizontal = (int)SizeFlags.Fill;
            blockNode.SizeFlagsVertical = (int)SizeFlags.Fill;
            running--;
        };
    }

    private void HandleBlockMoved(BlockMovedChange blockMovedChange) {
        GD.Print($"Moving block from {blockMovedChange.InitialPosition} to {blockMovedChange.TargetPosition}");
        // remove node from one place
        var initialGridNode = GetGridNode(blockMovedChange.InitialPosition);
        var blockToMove = initialGridNode.GetNode<BlockNode>(BlockNodeName);
        initialGridNode.RemoveChild(blockToMove);

        var targetGridNode = GetGridNode(blockMovedChange.TargetPosition);
        targetGridNode.AddChild(blockToMove);

        blockToMove.Visible = false;
        var tween = TweenMovement(initialGridNode, targetGridNode, blockToMove.Value);
        tween.Finished += () => {
            blockToMove.Visible = true;
        };
    }

    private void HandleBlocksMerged(BlocksMergedChange blocksMergedChange) {
        // todo: add log
        // remove one node
        var initialGridNode = GetGridNode(blocksMergedChange.MergedBlockInitialPosition);
        var blockToMove = initialGridNode.GetNode<BlockNode>(BlockNodeName);
        initialGridNode.RemoveChild(blockToMove);
        blockToMove.QueueFree();

        _scoreKeeper.UpdateScore(blocksMergedChange.NewBlockValue);

        // and update the other
        var targetGridNode = GetGridNode(blocksMergedChange.MergeReceiverPosition);
        var blockToMergeTo = targetGridNode.GetNode<BlockNode>(BlockNodeName);
        
        var tween = TweenMovement(initialGridNode, targetGridNode, blockToMove.Value);
        tween.Finished += () => {
            blockToMergeTo.Value = blocksMergedChange.NewBlockValue;
        };

        if (blocksMergedChange.NewBlockValue == 2048) {
            _banner.Show(true, _scoreKeeper.Score);
        }
    }

    private void HandleNoMovesLeft(NoMovesLeftChange noMovesLeftChange) {
        _banner.Show(false, _scoreKeeper.Score);
    }

    private Tween TweenMovement(Node initialGridCell, Node targetGridCell, int tweenBlockValue) {
        running++;

        var blockToTween = CreateBlockNode();

        blockToTween.Position = GetInnerGridField(initialGridCell).GlobalPosition;
        AddChild(blockToTween);
        blockToTween.Value = tweenBlockValue;
        
        blockToTween.CustomMinimumSize = GetInnerGridField(initialGridCell).Size.ToVector2I();

        var tween = CreateTween();
        tween.SetEase(Tween.EaseType.In);
        tween.TweenProperty(blockToTween, "position", GetInnerGridField(targetGridCell).GlobalPosition, MoveAnimationTime);

        tween.Finished += () => {
            RemoveChild(blockToTween);
            running--;
        };
        return tween;
    }

    private void HandleRestartButtonClicked() {
        _scoreKeeper.ResetScore();
        _banner.Visible = false;
        foreach (var gridNode in _mainGrid.GetChildren()) {
            var block = gridNode.GetNode<BlockNode>(BlockNodeName);
            if (block != null) {
                gridNode.RemoveChild(block);
                block.QueueFree();
            }
        }

        _logicGrid = new LogicGrid(GridDimension);
        HandleBlockSpawned(_logicGrid.Initialize());

        GD.Print("Restarted game!");
    }

    private Control GetGridNode(GridPosition gridPosition) {
        return (Control)_mainGrid.GetChild(gridPosition.Row * GridDimension + gridPosition.Column);
    }

    private BlockNode CreateBlockNode() {
        var blockNode = (BlockNode)_blockScene.Instantiate();
        blockNode.Name = BlockNodeName;
        return blockNode;
    }

    // grid node is just an wrapper around color rect
    // this could be made more objective (make grid cell a scene) but why bother now
    private static Control GetInnerGridField(Node gridNode) {
        return (ColorRect)gridNode.FindChild("ColorRect");
    }
}

// todo move this to some utils file and add rounding modes
public static class GodotExtensions {
    public static Vector2i ToVector2I(this Vector2 vec) {
        return new Vector2i((int) vec.x, (int) vec.y);
    }
}