namespace Logic;

public interface IGridChange { }

public record BlockMovedChange : IGridChange {
    public GridPosition InitialPosition { get; init; }
    public GridPosition TargetPosition { get; init; }
}

public record BlocksMergedChange : IGridChange {
    public GridPosition MergedBlockInitialPosition { get; init; }
    public GridPosition MergeReceiverPosition { get; init; }
    public int NewBlockValue { get; init; }
}

public record BlockSpawnedChange : IGridChange {
    public GridPosition NewBlockPosition { get; init; }
    public int NewBlockValue { get; init; }
}