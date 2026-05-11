public class BlockBeamBehavior : PieceBeamBehavior
{
    public override PieceType PieceType => PieceType.Block;

    public override BeamInteractionResult HandleHit(
        BoardPiece piece,
        Direction incomingDirection,
        LaserSimulationContext context)
    {
        return BeamInteractionResult.Block(incomingDirection);
    }
}