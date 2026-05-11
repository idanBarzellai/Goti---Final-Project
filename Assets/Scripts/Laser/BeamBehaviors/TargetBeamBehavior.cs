public class TargetBeamBehavior : PieceBeamBehavior
{
    public override PieceType PieceType => PieceType.Target;

    public override BeamInteractionResult HandleHit(
        BoardPiece piece,
        Direction incomingDirection,
        LaserSimulationContext context)
    {
        return BeamInteractionResult.Target(incomingDirection);
    }
}