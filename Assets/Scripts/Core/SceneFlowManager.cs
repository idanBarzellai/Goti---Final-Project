using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneFlowManager
{
    public const string MainMenuSceneName = "MainMenuScene";
    public const string GameSceneName = "GameScene";

    public static int SelectedLevelIndex { get; private set; } = 0;

    public static void StartLevel(int levelIndex)
    {
        SelectedLevelIndex = levelIndex;
        SceneManager.LoadScene(GameSceneName);
    }

    public static void GoToMainMenu()
    {
        SceneManager.LoadScene(MainMenuSceneName);
    }

    public static void QuitGame()
    {
        Debug.Log("Quit Game");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}