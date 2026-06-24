using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject levelSelectPanel;

    [SerializeField] private Button startButton;
    [SerializeField] private Button chooseLevelButton;
    [SerializeField] private Button backButton;
    [SerializeField] private AudioMuteToggleButtonUI muteToggleButton;

    private void Start()
    {
        ShowMainMenu();

        EnsureMuteToggleButton();

        if (startButton != null)
            startButton.onClick.AddListener(StartFirstLevel);

        if (chooseLevelButton != null)
            chooseLevelButton.onClick.AddListener(ShowLevelSelectWithClick);

        if (backButton != null)
            backButton.onClick.AddListener(ShowMainMenuWithClick);
    }

    private void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        levelSelectPanel.SetActive(false);
    }

    private void ShowLevelSelect()
    {
        mainMenuPanel.SetActive(false);
        levelSelectPanel.SetActive(true);
    }

    private void StartFirstLevel()
    {
        AudioManager.Instance?.PlayButtonClick();
        SceneFlowManager.StartLevel(0);
    }

    private void ShowMainMenuWithClick()
    {
        AudioManager.Instance?.PlayButtonClick();
        ShowMainMenu();
    }

    private void ShowLevelSelectWithClick()
    {
        AudioManager.Instance?.PlayButtonClick();
        ShowLevelSelect();
    }

    private void EnsureMuteToggleButton()
    {
        if (muteToggleButton != null || mainMenuPanel == null)
            return;

        muteToggleButton = AudioMuteToggleButtonUI.CreateDefault(mainMenuPanel.transform, "MuteToggleButton");
    }
}
