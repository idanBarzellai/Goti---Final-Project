using System.Collections;
using System.Collections.Generic;
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

    [Header("Confetti")]
    [SerializeField] private RectTransform confettiContainer;
    [SerializeField] private int confettiPieceCount = 160;
    [SerializeField] private float confettiDuration = 3.6f;
    [SerializeField] private float winScreenDelay = 1f;
    [SerializeField] private Vector2 confettiSizeRange = new Vector2(8f, 18f);
    [SerializeField] private Vector2 confettiHorizontalSpeedRange = new Vector2(-170f, 170f);
    [SerializeField] private Vector2 confettiFallSpeedRange = new Vector2(180f, 560f);
    [SerializeField] private float confettiGravity = 170f;

    private readonly List<GameObject> activeConfetti = new List<GameObject>();
    private Coroutine confettiRoutine;
    private Coroutine revealWinRoutine;
    private CanvasGroup winScreenCanvasGroup;
    private bool winStateShown;
    private bool showNextLevelButton;
    private static readonly Color[] ConfettiPalette =
    {
        HexToColor(0x393D3F),
        HexToColor(0xFDFDFF),
        HexToColor(0xC6C5B9),
        HexToColor(0x5AFF15),
        HexToColor(0xC8F9CE)
    };

    protected override void Start()
    {
        base.Start();

        if (GameManager.Instance != null)
            GameManager.Instance.OnLevelSolved += ShowWin;

        if (nextLevelButton != null)
            nextLevelButton.onClick.AddListener(NextLevel);

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartFromPanel);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(GoToMainMenuFromPanel);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnLevelSolved -= ShowWin;
    }

    public void ShowFailed()
    {
        winStateShown = false;
        showNextLevelButton = false;
        StopRevealWinRoutine();
        StopConfetti();
        Show();
        SetWinScreenVisible(true);

        if (titleText != null)
            titleText.text = "Level Failed!";
        if (nextLevelButton != null)
            nextLevelButton.gameObject.SetActive(false);
    }

    public void ShowWin()
    {
        if (winStateShown)
            return;

        winStateShown = true;
        Show();
        SetWinScreenVisible(false);

        if(titleText != null)
            titleText.text = "LEVEL SOLVED!";
        if (nextLevelButton != null)
        {
            showNextLevelButton =
                LevelManager.Instance != null &&
                LevelManager.Instance.HasNextLevel();

            nextLevelButton.gameObject.SetActive(false);
        }

        PlayConfetti();
        StopRevealWinRoutine();
        revealWinRoutine = StartCoroutine(RevealWinScreenAfterDelay());
    }

    private void NextLevel()
    {
        AudioManager.Instance?.PlayButtonClick();
        Hide();

        if (GameManager.Instance != null)
            GameManager.Instance.LoadNextLevel();
    }

    private void RestartFromPanel()
    {
        AudioManager.Instance?.PlayButtonClick();
        RestartLevel();
    }

    private void GoToMainMenuFromPanel()
    {
        AudioManager.Instance?.PlayButtonClick();
        GoToMainMenu();
    }

    public override void Hide()
    {
        winStateShown = false;
        StopRevealWinRoutine();
        StopConfetti();
        SetWinScreenVisible(true);
        base.Hide();
    }

    private void PlayConfetti()
    {
        StopConfetti();
        EnsureConfettiContainer();

        if (confettiContainer == null)
            return;

        confettiRoutine = StartCoroutine(ConfettiRoutine());
    }

    private void StopConfetti()
    {
        if (confettiRoutine != null)
        {
            StopCoroutine(confettiRoutine);
            confettiRoutine = null;
        }

        for (int i = 0; i < activeConfetti.Count; i++)
        {
            if (activeConfetti[i] != null)
                Destroy(activeConfetti[i]);
        }

        activeConfetti.Clear();
    }

    private void StopRevealWinRoutine()
    {
        if (revealWinRoutine == null)
            return;

        StopCoroutine(revealWinRoutine);
        revealWinRoutine = null;
    }

    private IEnumerator RevealWinScreenAfterDelay()
    {
        yield return new WaitForSecondsRealtime(winScreenDelay);
        SetWinScreenVisible(true);
        revealWinRoutine = null;
    }

    private void SetWinScreenVisible(bool visible)
    {
        EnsureWinScreenCanvasGroup();

        if (winScreenCanvasGroup != null)
        {
            winScreenCanvasGroup.alpha = visible ? 1f : 0f;
            winScreenCanvasGroup.interactable = visible;
            winScreenCanvasGroup.blocksRaycasts = visible;
        }

        if (nextLevelButton != null)
            nextLevelButton.gameObject.SetActive(showNextLevelButton);

        if (visible)
            transform.SetAsLastSibling();
    }

    private void EnsureWinScreenCanvasGroup()
    {
        if (winScreenCanvasGroup != null)
            return;

        winScreenCanvasGroup = GetComponent<CanvasGroup>();

        if (winScreenCanvasGroup == null)
            winScreenCanvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    private void EnsureConfettiContainer()
    {
        if (confettiContainer != null)
            return;

        Transform confettiParent = transform.parent != null ? transform.parent : transform;
        GameObject containerObject = new GameObject("ConfettiContainer", typeof(RectTransform));
        containerObject.transform.SetParent(confettiParent, false);
        containerObject.transform.SetAsLastSibling();

        confettiContainer = containerObject.GetComponent<RectTransform>();
        confettiContainer.anchorMin = Vector2.zero;
        confettiContainer.anchorMax = Vector2.one;
        confettiContainer.offsetMin = Vector2.zero;
        confettiContainer.offsetMax = Vector2.zero;
    }

    private IEnumerator ConfettiRoutine()
    {
        Rect bounds = confettiContainer.rect;
        float width = bounds.width > 0f ? bounds.width : Screen.width;
        float height = bounds.height > 0f ? bounds.height : Screen.height;

        ConfettiPiece[] pieces = new ConfettiPiece[Mathf.Max(1, confettiPieceCount)];

        for (int i = 0; i < pieces.Length; i++)
            pieces[i] = CreateConfettiPiece(width, height);

        float elapsed = 0f;
        while (elapsed < confettiDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            for (int i = 0; i < pieces.Length; i++)
                UpdateConfettiPiece(pieces[i], Time.unscaledDeltaTime);

            yield return null;
        }

        StopConfetti();
    }

    private ConfettiPiece CreateConfettiPiece(float width, float height)
    {
        GameObject pieceObject = new GameObject("ConfettiPiece", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        pieceObject.transform.SetParent(confettiContainer, false);

        RectTransform pieceRect = pieceObject.GetComponent<RectTransform>();
        pieceRect.anchorMin = new Vector2(0.5f, 0.5f);
        pieceRect.anchorMax = new Vector2(0.5f, 0.5f);
        pieceRect.pivot = new Vector2(0.5f, 0.5f);

        float widthScale = Random.Range(confettiSizeRange.x, confettiSizeRange.y);
        pieceRect.sizeDelta = new Vector2(widthScale * 0.55f, widthScale);
        pieceRect.anchoredPosition = new Vector2(
            Random.Range(-width * 0.55f, width * 0.55f),
            Random.Range(height * 0.55f, height * 0.9f)
        );
        pieceRect.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

        Image image = pieceObject.GetComponent<Image>();
        image.color = ConfettiPalette[Random.Range(0, ConfettiPalette.Length)];
        image.raycastTarget = false;

        activeConfetti.Add(pieceObject);

        return new ConfettiPiece
        {
            RectTransform = pieceRect,
            Velocity = new Vector2(
                Random.Range(confettiHorizontalSpeedRange.x, confettiHorizontalSpeedRange.y),
                -Random.Range(confettiFallSpeedRange.x, confettiFallSpeedRange.y)
            ),
            RotationSpeed = Random.Range(-540f, 540f)
        };
    }

    private void UpdateConfettiPiece(ConfettiPiece piece, float deltaTime)
    {
        if (piece.RectTransform == null)
            return;

        piece.Velocity += Vector2.down * confettiGravity * deltaTime;
        piece.RectTransform.anchoredPosition += piece.Velocity * deltaTime;
        piece.RectTransform.Rotate(0f, 0f, piece.RotationSpeed * deltaTime);
    }

    private static Color HexToColor(int hex)
    {
        return new Color(
            ((hex >> 16) & 0xFF) / 255f,
            ((hex >> 8) & 0xFF) / 255f,
            (hex & 0xFF) / 255f,
            1f
        );
    }

    private class ConfettiPiece
    {
        public RectTransform RectTransform;
        public Vector2 Velocity;
        public float RotationSpeed;
    }
}
