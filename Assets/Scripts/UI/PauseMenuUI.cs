using UnityEngine;
using UnityEngine.UI;

public class PauseMenuUI : BaseMenuUI
{
    [Header("Pause Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;

    protected override void Start()
    {
        base.Start();

        if (resumeButton != null)
            resumeButton.onClick.AddListener(Resume);

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartFromPause);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(GoToMainMenu);
    }

    protected override void OnShown()
    {
        Time.timeScale = 0f;
    }

    protected override void OnHidden()
    {
        Time.timeScale = 1f;
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }

    public void Resume()
    {
        Hide();
    }

    private void RestartFromPause()
    {
        Hide();
        RestartLevel();
    }
}