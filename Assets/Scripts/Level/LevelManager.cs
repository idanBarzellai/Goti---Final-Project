using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [SerializeField] private LevelData[] levels;
    [SerializeField] private int startingLevelIndex = 0;
    [SerializeField] private string gameplaySceneName = "GameScene";

    private BoardManager boardManager;
    private InventoryBarUI inventoryBarUI;

    public LevelData CurrentLevel { get; private set; }
    public int CurrentLevelIndex { get; private set; }
    public int LevelCount => levels != null ? levels.Length : 0;

private void Awake()
{
    if (Instance != null && Instance != this)
    {
        Destroy(gameObject);
        return;
    }

    Instance = this;
    DontDestroyOnLoad(gameObject);

    CurrentLevelIndex = SceneFlowManager.SelectedLevelIndex;

    if (levels != null && levels.Length > 0)
        CurrentLevel = levels[CurrentLevelIndex];
}
    public void RegisterGameplaySceneReferences(
        BoardManager boardManager,
        InventoryBarUI inventoryBarUI)
    {
        this.boardManager = boardManager;
        this.inventoryBarUI = inventoryBarUI;

        ReloadCurrentLevel();
    }

   public void SelectLevelAndLoadGame(int levelIndex)
{
    if (!IsValidLevelIndex(levelIndex))
        return;

    CurrentLevelIndex = levelIndex;
    CurrentLevel = levels[levelIndex];

    SceneManager.LoadScene(gameplaySceneName);
}
    public void LoadLevelByIndex(int levelIndex)
    {
        if (!IsValidLevelIndex(levelIndex))
            return;

        CurrentLevelIndex = levelIndex;
        CurrentLevel = levels[levelIndex];

        LoadCurrentLevelIntoScene();
    }

    public void ReloadCurrentLevel()
    {
        if (!IsValidLevelIndex(CurrentLevelIndex))
            return;

        CurrentLevel = levels[CurrentLevelIndex];
        LoadCurrentLevelIntoScene();
    }

public void LoadNextLevel()
{
    int nextIndex = CurrentLevelIndex + 1;

    if (!IsValidLevelIndex(nextIndex))
    {
        SceneFlowManager.GoToMainMenu();
        return;
    }

    CurrentLevelIndex = nextIndex;
    CurrentLevel = levels[CurrentLevelIndex];

    LoadCurrentLevelIntoScene();
}

    private void LoadCurrentLevelIntoScene()
    {
        if (CurrentLevel == null)
            return;

        if (boardManager != null)
            boardManager.LoadBoard(CurrentLevel);

        if (inventoryBarUI != null)
            inventoryBarUI.LoadInventory(CurrentLevel);
    }

    public bool HasNextLevel()
{
    return CurrentLevelIndex + 1 < LevelCount;
}

    private bool IsValidLevelIndex(int levelIndex)
    {
        if (levels == null || levels.Length == 0)
        {
            Debug.LogError("LevelManager: No levels assigned.");
            return false;
        }

        if (levelIndex < 0 || levelIndex >= levels.Length)
        {
            Debug.LogError($"LevelManager: Invalid level index {levelIndex}");
            return false;
        }

        return true;
    }
}