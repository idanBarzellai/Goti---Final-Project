using UnityEngine;
using UnityEngine.UI;

public class GameWinPanelUI : MonoBehaviour
{
    [SerializeField] private GameObject winPanel;
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button levelSelectButton;
    [SerializeField] private Button mainMenuButton;

    [SerializeField] private LevelManager levelManager;

    private void Start()
    {
        Hide();

        if (GameManager.Instance != null)
            GameManager.Instance.OnLevelSolved += Show;

        if (nextLevelButton != null)
            nextLevelButton.onClick.AddListener(NextLevel);

        if (restartButton != null)
            restartButton.onClick.AddListener(Restart);

        if (levelSelectButton != null)
            levelSelectButton.onClick.AddListener(SceneFlowManager.GoToMainMenu);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(SceneFlowManager.GoToMainMenu);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnLevelSolved -= Show;
    }

    private void Show()
    {
        if (winPanel != null)
            winPanel.SetActive(true);

        if (nextLevelButton != null && levelManager != null)
            nextLevelButton.gameObject.SetActive(levelManager.HasNextLevel());
    }

    private void Hide()
    {
        if (winPanel != null)
            winPanel.SetActive(false);
    }

    private void NextLevel()
    {
        Hide();

        if (GameManager.Instance != null)
            GameManager.Instance.LoadNextLevel();
    }

    private void Restart()
    {
        Hide();

        if (GameManager.Instance != null)
            GameManager.Instance.ReloadLevel();
    }
}