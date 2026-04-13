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

                    currentCell = nextCell;
                    continue;

                case PieceType.Reflect:
                    if (LaserReflectionUtility.TryReflect(currentDirection, hitPiece.Direction, out Direction reflectedDirection))
                    {
                        currentCell = nextCell;
                        currentDirection = reflectedDirection;
                        continue;
                    }

                    result.wasBlocked = true;
                    return result;

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
}