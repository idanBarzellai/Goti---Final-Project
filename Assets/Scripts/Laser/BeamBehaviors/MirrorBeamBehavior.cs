public class MirrorBeamBehavior : PieceBeamBehavior
{
    public override PieceType PieceType => PieceType.Mirror;

    public override BeamInteractionResult HandleHit(
        BoardPiece piece,
        Direction incomingDirection,
        LaserSimulationContext context)
    {
        if (LaserReflectionUtility.TryReflect(
                incomingDirection,
                piece.Direction,
                out Direction reflectedDirection))
        {
            return BeamInteractionResult.Continue(reflectedDirection);
        }

        return BeamInteractionResult.Block(incomingDirection);
    }
}