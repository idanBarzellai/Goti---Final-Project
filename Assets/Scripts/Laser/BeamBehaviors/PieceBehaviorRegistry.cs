using System.Collections.Generic;

public class PieceBehaviorRegistry
{
    private readonly Dictionary<PieceType, PieceBeamBehavior> behaviors =
        new Dictionary<PieceType, PieceBeamBehavior>();

    public PieceBehaviorRegistry()
    {
        Register(new EntryBeamBehavior());
        Register(new TargetBeamBehavior());
        Register(new BlockBeamBehavior());
        Register(new MirrorBeamBehavior());
        Register(new ReflectBeamBehavior());
        Register(new CheckpointBeamBehavior());
        Register(new PortalBeamBehavior());
    }

    public bool TryGetBehavior(PieceType pieceType, out PieceBeamBehavior behavior)
    {
        return behaviors.TryGetValue(pieceType, out behavior);
    }

    private void Register(PieceBeamBehavior behavior)
    {
        if (behavior == null)
            return;

        behaviors[behavior.PieceType] = behavior;
    }
}