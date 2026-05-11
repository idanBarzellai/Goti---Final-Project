public class CheckpointBeamBehavior : PieceBeamBehavior
{
    public override PieceType PieceType => PieceType.Checkpoint;

    public override BeamInteractionResult HandleHit(
        BoardPiece piece,
        Direction incomingDirection,
        LaserSimulationContext context)
    {
        if (IsCheckpointPassAllowed(piece.Direction, incomingDirection))
        {
            return BeamInteractionResult.Continue(incomingDirection);
        }

        return BeamInteractionResult.Block(incomingDirection);
    }

    private bool IsCheckpointPassAllowed(Direction checkpointDirection, Direction incomingDirection)
    {
        bool checkpointIsVertical =
            checkpointDirection == Direction.Up ||
            checkpointDirection == Direction.Down;

        bool beamIsVertical =
            incomingDirection == Direction.Up ||
            incomingDirection == Direction.Down;

        return checkpointIsVertical == beamIsVertical;
    }
}