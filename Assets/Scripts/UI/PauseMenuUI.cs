using UnityEngine;
using UnityEngine.UI;

public class PauseMenuUI : BaseMenuUI
{
    [Header("Pause Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private AudioMuteToggleButtonUI muteToggleButton;

    private bool listenersRegistered;

    private void Awake()
    {
        if (!enabled)
            return;

        EnsureMuteToggleButton();
        RegisterButtonListeners();
    }

    protected override void Start()
    {
        base.Start();
        RegisterButtonListeners();
    }

    protected override void OnShown()
    {
        EnsureMuteToggleButton();
        Time.timeScale = 0f;
    }

    protected override void OnHidden()
    {
        Time.timeScale = 1f;
    }

    private void RegisterButtonListeners()
    {
        if (listenersRegistered)
            return;

        if (resumeButton != null)
            resumeButton.onClick.AddListener(Resume);

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartFromPause);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(GoToMainMenuFromPause);

        listenersRegistered = true;
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }

    public void Resume()
    {
        AudioManager.Instance?.PlayButtonClick();
        Hide();
    }

    private void RestartFromPause()
    {
        AudioManager.Instance?.PlayButtonClick();
        Hide();
        RestartLevel();
    }

    private void GoToMainMenuFromPause()
    {
        AudioManager.Instance?.PlayButtonClick();
        GoToMainMenu();
    }

    private void EnsureMuteToggleButton()
    {
        if (muteToggleButton != null)
            return;

        muteToggleButton = AudioMuteToggleButtonUI.CreateDefault(transform, "MuteToggleButton");
    }
}
