#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class PlayFromMainMenu
{
    private const string MainMenuScenePath = "Assets/Scenes/MainMenuScene.unity";
    private const string PreviousSceneKey = "LaserMazer_PreviousSceneBeforePlay";

    static PlayFromMainMenu()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            string currentScenePath = SceneManager.GetActiveScene().path;

            EditorPrefs.SetString(PreviousSceneKey, currentScenePath);

            bool shouldContinue = EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            if (!shouldContinue)
            {
                EditorApplication.isPlaying = false;
                return;
            }

            if (currentScenePath != MainMenuScenePath)
            {
                EditorSceneManager.OpenScene(MainMenuScenePath);
            }
        }

        if (state == PlayModeStateChange.EnteredEditMode)
        {
            string previousScenePath = EditorPrefs.GetString(PreviousSceneKey, "");

            if (!string.IsNullOrEmpty(previousScenePath) &&
                previousScenePath != SceneManager.GetActiveScene().path)
            {
                EditorSceneManager.OpenScene(previousScenePath);
            }

            EditorPrefs.DeleteKey(PreviousSceneKey);
        }
    }
}

#endif