using System.Collections.Generic;
using UnityEngine;

public class LaserSimulationResult
{
    public List<BeamStep> visitedSteps = new List<BeamStep>();
    public List<BeamSegment> segments = new List<BeamSegment>();
    public List<BoardPiece> hitTargets = new List<BoardPiece>();
    public List<BoardPiece> hitPieces = new List<BoardPiece>();

    public bool didHitAnyTarget;
    public bool exitedBoard;
    public bool wasBlocked;
    public bool detectedLoop;
    public bool hadEntry;
}