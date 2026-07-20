using System;
using UnityEngine;

public class LaserControlManager : MonoBehaviour
{
    [SerializeField] private BoardManager boardManager;
    [SerializeField] private LaserView laserView;

    private LaserSimulationService laserSimulationService;
    private LaserSimulationResult lastResult;

    public LaserSimulationResult LastResult => lastResult;

    public event Action<LaserSimulationResult> OnLaserFired;

    private void Start()
{
    if (boardManager == null)
    {
        Debug.LogError("LaserControlManager: BoardManager reference is missing.");
        return;
    }

    laserSimulationService = new LaserSimulationService(boardManager);

    boardManager.OnBoardStateChanged += HandleBoardStateChanged;
    boardManager.OnBoardLoaded += HandleBoardLoaded;

    ClearLaser();
}

private void OnDestroy()
{
    if (boardManager != null)
    {
        boardManager.OnBoardStateChanged -= HandleBoardStateChanged;
        boardManager.OnBoardLoaded -= HandleBoardLoaded;
    }
}

private void HandleBoardStateChanged()
{
    ClearLaser();
}

private void HandleBoardLoaded()
{
    ClearLaser();
}

    public LaserSimulationResult FireLaser()
    {
        if (laserSimulationService == null)
            return null;

        lastResult = laserSimulationService.Simulate();

        DebugLogResult(lastResult);

        OnLaserFired?.Invoke(lastResult);

        return lastResult;
    }

    public void ClearLaser()
    {
        lastResult = null;

        if (laserView != null)
            laserView.Clear();

        boardManager?.ResetCellTraversalAnimations();
    }

    private void DebugLogResult(LaserSimulationResult result)
    {
        if (result == null)
            return;

        Debug.Log(
            $"Laser Result | Entry: {result.hadEntry} | " +
            $"Targets Hit: {result.hitTargets.Count} | " +
            $"Exited: {result.exitedBoard} | " +
            $"Blocked: {result.wasBlocked} | " +
            $"Loop: {result.detectedLoop}"
        );
    }
}
