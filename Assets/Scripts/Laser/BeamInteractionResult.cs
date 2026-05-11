using UnityEngine;

public class BeamInteractionResult
{
    public bool shouldStop;
    public bool wasBlocked;
    public bool hitTarget;

    public bool teleport;
    public Vector2Int teleportCell;

    public Direction outgoingDirection;

    public static BeamInteractionResult Continue(Direction outgoingDirection)
    {
        return new BeamInteractionResult
        {
            shouldStop = false,
            wasBlocked = false,
            hitTarget = false,
            teleport = false,
            outgoingDirection = outgoingDirection
        };
    }

    public static BeamInteractionResult Block(Direction outgoingDirection)
    {
        return new BeamInteractionResult
        {
            shouldStop = true,
            wasBlocked = true,
            hitTarget = false,
            teleport = false,
            outgoingDirection = outgoingDirection
        };
    }

    public static BeamInteractionResult Target(Direction outgoingDirection)
    {
        return new BeamInteractionResult
        {
            shouldStop = true,
            wasBlocked = false,
            hitTarget = true,
            teleport = false,
            outgoingDirection = outgoingDirection
        };
    }

    public static BeamInteractionResult Teleport(Vector2Int teleportCell, Direction outgoingDirection)
    {
        return new BeamInteractionResult
        {
            shouldStop = false,
            wasBlocked = false,
            hitTarget = false,
            teleport = true,
            teleportCell = teleportCell,
            outgoingDirection = outgoingDirection
        };
    }
}