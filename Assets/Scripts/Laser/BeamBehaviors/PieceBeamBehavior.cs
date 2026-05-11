public abstract class PieceBeamBehavior
{
    public abstract PieceType PieceType { get; }

    public abstract BeamInteractionResult HandleHit(
        BoardPiece piece,
        Direction incomingDirection,
        LaserSimulationContext context
    );
}