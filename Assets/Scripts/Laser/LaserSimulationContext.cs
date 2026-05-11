public class LaserSimulationContext
{
    public BoardManager BoardManager { get; }

    public LaserSimulationContext(BoardManager boardManager)
    {
        BoardManager = boardManager;
    }
}