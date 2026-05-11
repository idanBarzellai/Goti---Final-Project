public class ReflectBeamBehavior : PieceBeamBehavior
{
    public override PieceType PieceType => PieceType.Reflect;

    public override BeamInteractionResult HandleHit(
        BoardPiece piece,
        Direction incomingDirection,
        LaserSimulationContext context)
    {
        if (TryTriangleReflect(piece.Direction, incomingDirection, out Direction reflectedDirection))
        {
            return BeamInteractionResult.Continue(reflectedDirection);
        }

        return BeamInteractionResult.Block(incomingDirection);
    }

    private bool TryTriangleReflect(
        Direction reflectorDirection,
        Direction incomingDirection,
        out Direction reflectedDirection)
    {
        Direction localIncoming = RotateToLocal(incomingDirection, reflectorDirection);

        Direction localReflected;

        switch (localIncoming)
        {
            // Laser comes from bottom, so it is moving Up.
            case Direction.Up:
                localReflected = Direction.Left;
                break;

            // Laser comes from left, so it is moving Right.
            case Direction.Right:
                localReflected = Direction.Down;
                break;

            default:
                reflectedDirection = incomingDirection;
                return false;
        }

        reflectedDirection = RotateToWorld(localReflected, reflectorDirection);
        return true;
    }

    private Direction RotateToLocal(Direction worldDirection, Direction pieceDirection)
    {
        return (Direction)(((int)worldDirection - (int)pieceDirection + 4) % 4);
    }

    private Direction RotateToWorld(Direction localDirection, Direction pieceDirection)
    {
        return (Direction)(((int)localDirection + (int)pieceDirection) % 4);
    }
}