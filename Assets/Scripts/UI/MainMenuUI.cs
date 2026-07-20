using System.Collections;
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

    [Header("Background GOTI")]
    [SerializeField] private PieceSpriteLibrary spriteLibrary;
    [SerializeField] private Vector2 gotiSize = new Vector2(220f, 220f);
    [SerializeField] private float gotiMoveSpeed = 260f;
    [SerializeField] private float animationFrameDuration = 0.06f;
    [SerializeField] private float smileHoldDuration = 0.75f;

    private RectTransform backgroundGoti;
    private Image backgroundGotiImage;
    private Coroutine rollingRoutine;
    private bool startingGame;

    private void Start()
    {
        Time.timeScale = 1f;
        ShowMainMenu();

        EnsureMuteToggleButton();
        CreateBackgroundGoti();

        if (startButton != null)
            startButton.onClick.AddListener(StartFirstLevel);

        if (chooseLevelButton != null)
            chooseLevelButton.onClick.AddListener(ShowLevelSelectWithClick);

        if (backButton != null)
            backButton.onClick.AddListener(ShowMainMenuWithClick);
    }

    private void OnDisable()
    {
        if (rollingRoutine != null)
        {
            StopCoroutine(rollingRoutine);
            rollingRoutine = null;
        }
    }

    private void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        levelSelectPanel.SetActive(false);
        ResumeBackgroundGoti();
    }

    private void ShowLevelSelect()
    {
        mainMenuPanel.SetActive(false);
        levelSelectPanel.SetActive(true);
    }

    private void ResumeBackgroundGoti()
    {
        if (!startingGame && backgroundGoti != null && rollingRoutine == null)
            rollingRoutine = StartCoroutine(RollRandomly());
    }

    private void StartFirstLevel()
    {
        if (startingGame)
            return;

        AudioManager.Instance?.PlayButtonClick();
        StartCoroutine(PlaySmileAndStart());
    }

    private void CreateBackgroundGoti()
    {
        if (spriteLibrary == null || mainMenuPanel == null)
            return;

        Transform parent = mainMenuPanel.transform.parent != null ? mainMenuPanel.transform.parent : mainMenuPanel.transform;
        GameObject goti = new GameObject("BackgroundGOTI", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        goti.transform.SetParent(parent, false);
        Transform title = parent.Find("GameTItle");
        if (title != null)
            goti.transform.SetSiblingIndex(title.GetSiblingIndex());
        else
            goti.transform.SetAsFirstSibling();
        backgroundGoti = goti.GetComponent<RectTransform>();
        backgroundGoti.anchorMin = backgroundGoti.anchorMax = new Vector2(0.5f, 0.5f);
        backgroundGoti.sizeDelta = gotiSize;
        backgroundGotiImage = goti.GetComponent<Image>();
        backgroundGotiImage.raycastTarget = false;
        backgroundGotiImage.preserveAspect = true;
        rollingRoutine = StartCoroutine(RollRandomly());
    }

    private IEnumerator RollRandomly()
    {
        RectTransform parentRect = backgroundGoti.parent as RectTransform;
        while (true)
        {
            Rect bounds = parentRect != null ? parentRect.rect : new Rect(-Screen.width * 0.5f, -Screen.height * 0.5f, Screen.width, Screen.height);
            bool horizontal = Random.value < 0.5f;
            bool positive = Random.value < 0.5f;
            float margin = Mathf.Max(gotiSize.x, gotiSize.y) * 0.6f;
            Vector2 start;
            Vector2 end;
            Direction direction;

            if (horizontal)
            {
                float y = Random.Range(bounds.yMin + margin, bounds.yMax - margin);
                start = new Vector2(positive ? bounds.xMin - margin : bounds.xMax + margin, y);
                end = new Vector2(positive ? bounds.xMax + margin : bounds.xMin - margin, y);
                direction = positive ? Direction.Right : Direction.Left;
            }
            else
            {
                float x = Random.Range(bounds.xMin + margin, bounds.xMax - margin);
                start = new Vector2(x, positive ? bounds.yMin - margin : bounds.yMax + margin);
                end = new Vector2(x, positive ? bounds.yMax + margin : bounds.yMin - margin);
                direction = positive ? Direction.Up : Direction.Down;
            }

            backgroundGoti.anchoredPosition = start;
            Sprite[] frames = spriteLibrary.GetRollFrames(direction);
            float distance = Vector2.Distance(start, end);
            float elapsed = 0f;
            int frame = 0;
            float nextFrame = 0f;

            while (elapsed < distance / gotiMoveSpeed)
            {
                elapsed += Time.unscaledDeltaTime;
                if (frames != null && frames.Length > 0 && elapsed >= nextFrame)
                {
                    backgroundGotiImage.sprite = frames[frame++ % frames.Length];
                    nextFrame += animationFrameDuration;
                }
                backgroundGoti.anchoredPosition = Vector2.Lerp(start, end, Mathf.Clamp01(elapsed / (distance / gotiMoveSpeed)));
                yield return null;
            }

            yield return new WaitForSecondsRealtime(Random.Range(0.15f, 0.65f));
        }
    }

    private IEnumerator PlaySmileAndStart()
    {
        yield return PlaySmileSequence();
        SceneFlowManager.StartLevel(0);
    }

    private IEnumerator PlaySmileSequence()
    {
        startingGame = true;
        if (startButton != null) startButton.interactable = false;
        if (chooseLevelButton != null) chooseLevelButton.interactable = false;
        if (rollingRoutine != null) { StopCoroutine(rollingRoutine); rollingRoutine = null; }

        RectTransform parentRect = backgroundGoti != null ? backgroundGoti.parent as RectTransform : null;
        if (backgroundGoti != null && parentRect != null)
        {
            Rect bounds = parentRect.rect;
            Vector2 halfSize = backgroundGoti.sizeDelta * 0.5f;
            Vector2 position = backgroundGoti.anchoredPosition;
            position.x = Mathf.Clamp(position.x, bounds.xMin + halfSize.x, bounds.xMax - halfSize.x);
            position.y = Mathf.Clamp(position.y, bounds.yMin + halfSize.y, bounds.yMax - halfSize.y);
            backgroundGoti.anchoredPosition = position;
            backgroundGotiImage.enabled = true;
        }

        Sprite[] frames = spriteLibrary != null ? spriteLibrary.winFrames : null;
        if (frames != null)
            foreach (Sprite frame in frames) { if (frame != null) backgroundGotiImage.sprite = frame; yield return new WaitForSecondsRealtime(animationFrameDuration); }

        yield return new WaitForSecondsRealtime(smileHoldDuration);
        if (startButton != null) startButton.interactable = true;
        if (chooseLevelButton != null) chooseLevelButton.interactable = true;
    }

    private void ShowMainMenuWithClick()
    {
        AudioManager.Instance?.PlayButtonClick();
        ShowMainMenu();
    }

    private void ShowLevelSelectWithClick()
    {
        if (startingGame)
            return;

        AudioManager.Instance?.PlayButtonClick();
        StartCoroutine(PlaySmileAndShowLevelSelect());
    }

    private IEnumerator PlaySmileAndShowLevelSelect()
    {
        yield return PlaySmileSequence();
        startingGame = false;
        ShowLevelSelect();
    }

    private void EnsureMuteToggleButton()
    {
        if (muteToggleButton != null || mainMenuPanel == null)
            return;

        muteToggleButton = AudioMuteToggleButtonUI.CreateDefault(mainMenuPanel.transform, "MuteToggleButton");
    }
}
