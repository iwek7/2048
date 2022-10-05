namespace Logic;

public interface IGridChange {
    IGridChange Transpose(IGridTransformer gridTransformer); // todo: generics
}

public record BlockMovedChange : IGridChange {
    public GridPosition InitialPosition { get; init; }
    public GridPosition TargetPosition { get; init; }
    public IGridChange Transpose(IGridTransformer gridTransformer) {
        return new BlockMovedChange {
            InitialPosition = gridTransformer.TransposeGridPosition(InitialPosition),
            TargetPosition = gridTransformer.TransposeGridPosition(TargetPosition)
        };
    }
}

public record BlocksMergedChange : IGridChange {
    public GridPosition MergedBlockInitialPosition { get; init; }
    public GridPosition MergeReceiverTargetPosition { get; init; }
    public GridPosition MergeReceiverInitialPosition { get; init; }
    public BlockValue NewBlockValue { get; init; }
    public IGridChange Transpose(IGridTransformer gridTransformer) {
        return new BlocksMergedChange {
            MergeReceiverInitialPosition = gridTransformer.TransposeGridPosition(MergeReceiverInitialPosition),
            MergeReceiverTargetPosition = gridTransformer.TransposeGridPosition(MergeReceiverTargetPosition),
            MergedBlockInitialPosition = gridTransformer.TransposeGridPosition(MergedBlockInitialPosition),
            NewBlockValue = NewBlockValue
        };
    }
}

public record BlockSpawnedChange : IGridChange {
    public GridPosition NewBlockPosition { get; init; }
    public BlockValue NewBlockValue { get; init; }
    public IGridChange Transpose(IGridTransformer gridTransformer) {
        throw new NotImplementedException();
    }
}

public record NoMovesLeftChange : IGridChange {
    public IGridChange Transpose(IGridTransformer gridTransformer) {
        throw new NotImplementedException();
    }
}