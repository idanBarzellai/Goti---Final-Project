using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject levelSelectPanel;

    [SerializeField] private Button startButton;
    [SerializeField] private Button chooseLevelButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button backButton;

    private void Start()
    {
        ShowMainMenu();

        startButton.onClick.AddListener(() => SceneFlowManager.StartLevel(0));
        chooseLevelButton.onClick.AddListener(ShowLevelSelect);
        quitButton.onClick.AddListener(SceneFlowManager.QuitGame);
        backButton.onClick.AddListener(ShowMainMenu);
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
}