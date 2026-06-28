public class PortalBeamBehavior : PieceBeamBehavior
{
    public override PieceType PieceType => PieceType.Portal;

    public override BeamInteractionResult HandleHit(
        BoardPiece piece,
        Direction incomingDirection,
        LaserSimulationContext context)
    {
        if (piece == null || context == null || context.BoardManager == null)
            return BeamInteractionResult.Block(incomingDirection);

        BoardPiece pairedPortal = FindPairedPortal(piece, context.BoardManager);

        if (pairedPortal == null)
            return BeamInteractionResult.Block(incomingDirection);

        return BeamInteractionResult.Teleport(
            pairedPortal.GridPosition,
            incomingDirection
        );
    }

    private BoardPiece FindPairedPortal(BoardPiece currentPortal, BoardManager boardManager)
    {
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
}
