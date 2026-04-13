using UnityEngine;

public class LaserControlManager : MonoBehaviour
{
    [SerializeField] private BoardManager boardManager;
    [SerializeField] private LaserView laserView;
    [SerializeField] private bool simulateOnStart = true;

    private LaserSimulationService laserSimulationService;
    private LaserSimulationResult lastResult;

    public LaserSimulationResult LastResult => lastResult;

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

    if (simulateOnStart)
    {
        SimulateLaser();
    }
    }

 private void OnDestroy()
{
    if (boardManager != null)
    {
        boardManager.OnBoardStateChanged -= HandleBoardStateChanged;
        boardManager.OnBoardLoaded -= HandleBoardLoaded;
    }
}

private void HandleBoardLoaded()
{
    SimulateLaser();
}
    private void HandleBoardStateChanged()
    {
        // For now, always simulate on board change.
        // Later this will depend on auto-fire mode.
        SimulateLaser();
    }

    public void SimulateLaser()
    {
        if (laserSimulationService == null)
            return;

        lastResult = laserSimulationService.Simulate();

        if (laserView != null)
        {
            laserView.Render(lastResult);
        }

        DebugLogResult(lastResult);
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