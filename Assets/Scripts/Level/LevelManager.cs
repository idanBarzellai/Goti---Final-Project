using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private LevelData[] levels;
    [SerializeField] private int startingLevelIndex = 0;

    [Header("Targets")]
    [SerializeField] private BoardManager boardManager;
    [SerializeField] private InventoryBarUI inventoryBarUI;

    public LevelData CurrentLevel { get; private set; }
    public int CurrentLevelIndex { get; private set; }

    private void Start()
{
    LoadLevelByIndex(SceneFlowManager.SelectedLevelIndex);
}

public bool HasNextLevel()
{
    return levels != null && CurrentLevelIndex + 1 < levels.Length;
}

public int LevelCount()
{
    return levels == null ? 0 : levels.Length;
}

    public void LoadLevelByIndex(int levelIndex)
    {
        if (levels == null || levels.Length == 0)
        {
            Debug.LogError("LevelManager: No levels assigned.");
            return;
        }

        if (levelIndex < 0 || levelIndex >= levels.Length)
        {
            Debug.LogError($"LevelManager: Invalid level index {levelIndex}");
            return;
        }

        CurrentLevelIndex = levelIndex;
        CurrentLevel = levels[levelIndex];

        if (boardManager != null)
        {
            boardManager.LoadBoard(CurrentLevel);
        }

        if (inventoryBarUI != null)
        {
            inventoryBarUI.LoadInventory(CurrentLevel);
        }
    }

    public void ReloadCurrentLevel()
    {
        LoadLevelByIndex(CurrentLevelIndex);
    }

    public void LoadNextLevel()
    {
        int nextIndex = CurrentLevelIndex + 1;
        if (nextIndex >= levels.Length)
        {
            Debug.Log("No more levels.");
            return;
        }

        LoadLevelByIndex(nextIndex);
    }
}