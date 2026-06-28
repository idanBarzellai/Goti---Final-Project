public class TargetBeamBehavior : PieceBeamBehavior
{
    public override PieceType PieceType => PieceType.Target;

    public override BeamInteractionResult HandleHit(
        BoardPiece piece,
        Direction incomingDirection,
        LaserSimulationContext context)
    {
        // Targets accept laser hits from every side; their rotation is visual only.
        return BeamInteractionResult.Target(incomingDirection);
    }
}
