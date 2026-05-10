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
            GameManager.Instance.OnLevelSolved += ShowWin;

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
            GameManager.Instance.OnLevelSolved -= ShowWin;
    }



public void ShowFailed()
{
    if (winPanel != null)
        winPanel.SetActive(true);

    if (nextLevelButton != null)
        nextLevelButton.gameObject.SetActive(false);
}

public void ShowWin()
{
    if (winPanel != null)
        winPanel.SetActive(true);

    if (nextLevelButton != null)
    {
        bool hasNextLevel =
            LevelManager.Instance != null &&
            LevelManager.Instance.HasNextLevel();

        nextLevelButton.gameObject.SetActive(hasNextLevel);
    }
}

    public void Hide()
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