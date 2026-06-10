using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameWinPanelUI : BaseMenuUI
{
    [Header("Win / Lose Buttons")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;

    protected override void Start()
    {
        base.Start();

        if (GameManager.Instance != null)
            GameManager.Instance.OnLevelSolved += ShowWin;

        if (nextLevelButton != null)
            nextLevelButton.onClick.AddListener(NextLevel);

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartLevel);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(GoToMainMenu);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnLevelSolved -= ShowWin;
    }

    public void ShowFailed()
    {
        Show();

        if (titleText != null)
            titleText.text = "Level Failed!";
        if (nextLevelButton != null)
            nextLevelButton.gameObject.SetActive(false);
    }

    public void ShowWin()
    {
        Show();

        if(titleText != null)
            titleText.text = "LEVEL SOLVED!";
        if (nextLevelButton != null)
        {
            bool hasNextLevel =
                LevelManager.Instance != null &&
                LevelManager.Instance.HasNextLevel();

            nextLevelButton.gameObject.SetActive(hasNextLevel);
        }
    }

    private void NextLevel()
    {
        Hide();

        if (GameManager.Instance != null)
            GameManager.Instance.LoadNextLevel();
    }
}