public class EntryBeamBehavior : PieceBeamBehavior
{
    public override PieceType PieceType => PieceType.Entry;

    public override BeamInteractionResult HandleHit(
        BoardPiece piece,
        Direction incomingDirection,
        LaserSimulationContext context)
    {
        return BeamInteractionResult.Continue(incomingDirection);
    }
}