using System.Collections.Generic;
using UnityEngine;

public class LaserSimulationService
{
    private readonly BoardManager boardManager;
    private readonly int maxSteps;

    public LaserSimulationService(BoardManager boardManager, int maxSteps = 100)
    {
        this.boardManager = boardManager;
        this.maxSteps = maxSteps;
    }

    public LaserSimulationResult Simulate()
    {
        LaserSimulationResult result = new LaserSimulationResult();

        if (boardManager == null)
            return result;

        BoardPiece entryPiece = boardManager.FindEntryPiece();
        if (entryPiece == null)
            return result;

        result.hadEntry = true;

        Vector2Int currentCell = entryPiece.GridPosition;
        Direction currentDirection = entryPiece.Direction;

        HashSet<string> visitedStates = new HashSet<string>();

        for (int i = 0; i < maxSteps; i++)
        {
            string stateKey = $"{currentCell.x}_{currentCell.y}_{(int)currentDirection}";
            if (visitedStates.Contains(stateKey))
            {
                result.detectedLoop = true;
                return result;
            }

            visitedStates.Add(stateKey);

            Vector2Int nextCell = currentCell + PieceRotationUtility.ToVector2Int(currentDirection);

            result.segments.Add(new BeamSegment(currentCell, nextCell));
            result.visitedSteps.Add(new BeamStep(nextCell, currentDirection));

            if (!boardManager.IsInsideBounds(nextCell))
            {
                result.exitedBoard = true;
                return result;
            }

            BoardPiece hitPiece = boardManager.GetPieceAt(nextCell);

            if (hitPiece == null)
{
    currentCell = nextCell;
    continue;
}

if (!result.hitPieces.Contains(hitPiece))
{
    result.hitPieces.Add(hitPiece);
}

            switch (hitPiece.PieceType)
            {
                case PieceType.Block:
                    result.wasBlocked = true;
                    return result;

               case PieceType.Target:
    if (!result.hitTargets.Contains(hitPiece))
    {
        result.hitTargets.Add(hitPiece);
    }

    result.didHitAnyTarget = true;

    return result;

               case PieceType.Mirror:
    if (LaserReflectionUtility.TryReflect(currentDirection, hitPiece.Direction, out Direction mirrorDirection))
    {
        currentCell = nextCell;
        currentDirection = mirrorDirection;
        continue;
    }

    result.wasBlocked = true;
    return result;

case PieceType.Reflect:
    if (TryTriangleReflect(hitPiece.Direction, currentDirection, out Direction triangleReflectDirection))
    {
        currentCell = nextCell;
        currentDirection = triangleReflectDirection;
        continue;
    }

    result.wasBlocked = true;
    return result;

case PieceType.Checkpoint:
    if (IsCheckpointPassAllowed(hitPiece.Direction, currentDirection))
    {
        currentCell = nextCell;
        continue;
    }

    result.wasBlocked = true;
    return result;

case PieceType.Portal:
    if (currentDirection != hitPiece.Direction)
    {
        result.wasBlocked = true;
        return result;
    }

    BoardPiece pairedPortal = FindPairedPortal(hitPiece);

    if (pairedPortal == null)
    {
        result.wasBlocked = true;
        return result;
    }

    currentCell = pairedPortal.GridPosition;
    continue;

                case PieceType.Entry:
                    currentCell = nextCell;
                    continue;

                default:
                    currentCell = nextCell;
                    continue;
            }
        }

        result.detectedLoop = true;
        return result;
    }

    private BoardPiece FindPairedPortal(BoardPiece currentPortal)
{
    if (currentPortal == null)
        return null;

    foreach (BoardPiece piece in boardManager.GetAllPieces())
    {
        if (piece == null)
            continue;

        if (piece == currentPortal)
            continue;

        if (piece.PieceType != PieceType.Portal)
            continue;

        if (piece.PortalPairId == currentPortal.PortalPairId)
            return piece;
    }

    return null;
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

private bool TryTriangleReflect(
    Direction reflectorDirection,
    Direction incomingDirection,
    out Direction reflectedDirection)
{
    // Only allow laser to hit the reflective/front side.
    if (incomingDirection != reflectorDirection)
    {
        reflectedDirection = incomingDirection;
        return false;
    }

    return LaserReflectionUtility.TryReflect(
        incomingDirection,
        reflectorDirection,
        out reflectedDirection
    );
}
}