using System.Collections.Generic;
using UnityEngine;

public class LaserSimulationService
{
    private readonly BoardManager boardManager;
    private readonly PieceBehaviorRegistry behaviorRegistry;
    private readonly LaserSimulationContext context;
    private readonly int maxSteps;

    public LaserSimulationService(BoardManager boardManager, int maxSteps = 100)
    {
        this.boardManager = boardManager;
        this.maxSteps = maxSteps;

        behaviorRegistry = new PieceBehaviorRegistry();
        context = new LaserSimulationContext(boardManager);
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

            Vector2Int nextCell =
                currentCell + PieceRotationUtility.ToVector2Int(currentDirection);

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

            if (!behaviorRegistry.TryGetBehavior(hitPiece.PieceType, out PieceBeamBehavior behavior))
            {
                currentCell = nextCell;
                continue;
            }

            BeamInteractionResult interactionResult =
                behavior.HandleHit(hitPiece, currentDirection, context);

            if (interactionResult.hitTarget)
            {
                if (!result.hitTargets.Contains(hitPiece))
                    result.hitTargets.Add(hitPiece);

                result.didHitAnyTarget = true;
                return result;
            }

            if (interactionResult.wasBlocked)
            {
                result.wasBlocked = true;
                return result;
            }

            if (interactionResult.teleport)
            {
                currentCell = interactionResult.teleportCell;
                currentDirection = interactionResult.outgoingDirection;
                continue;
            }

            if (interactionResult.shouldStop)
            {
                return result;
            }

            currentCell = nextCell;
            currentDirection = interactionResult.outgoingDirection;
        }

        result.detectedLoop = true;
        return result;
    }
}