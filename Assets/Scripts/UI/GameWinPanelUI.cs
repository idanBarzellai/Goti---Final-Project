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
    [SerializeField] private Sprite[] batSprites;
    [SerializeField] private Sprite[] boneSprites;
    [SerializeField, Min(1)] private int confettiAmount = 200;
    [SerializeField] private float confettiDuration = 4.6f;
    [SerializeField] private float winScreenDelay = 1f;
    [Tooltip("Minimum and maximum particle scale.")]
    [SerializeField] private Vector2 confettiSizeRange = new Vector2(0.7f, 1.2f);
    [Tooltip("Minimum and maximum launch speed in canvas units per second.")]
    [SerializeField] private Vector2 confettiSpeedRange = new Vector2(800f, 1150f);
    [SerializeField] private Vector2 confettiLifetimeRange = new Vector2(2.5f, 4f);
    [SerializeField] private Vector2 confettiGravityRange = new Vector2(350f, 550f);
    [Tooltip("Minimum and maximum emitter Y position as a fraction of screen height. 0 is center; -0.5 is bottom; 0.5 is top.")]
    [SerializeField] private Vector2 emitterYRange = new Vector2(-0.22f, 0.08f);

    private readonly List<GameObject> activeConfetti = new List<GameObject>();
    private Coroutine confettiRoutine;
    private Coroutine revealWinRoutine;
    private CanvasGroup winScreenCanvasGroup;
    private bool winStateShown;
    private bool showNextLevelButton;
    private bool confettiPlayedForCurrentWin;
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
        confettiPlayedForCurrentWin = false;
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
            titleText.text = "HURRAY!";
        if (nextLevelButton != null)
        {
            showNextLevelButton =
                LevelManager.Instance != null &&
                LevelManager.Instance.HasNextLevel();

            nextLevelButton.gameObject.SetActive(false);
        }

        if (!confettiPlayedForCurrentWin)
            PlayConfettiNow();
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
        confettiPlayedForCurrentWin = false;
        winStateShown = false;
        StopRevealWinRoutine();
        StopConfetti();
        SetWinScreenVisible(true);
        base.Hide();
    }

    private void PlayConfetti()
    {
        if (!gameObject.activeInHierarchy)
            return;

        StopConfetti();
        EnsureConfettiContainer();

        if (confettiContainer == null)
            return;

        confettiRoutine = StartCoroutine(ConfettiRoutine());
    }

    public void PlayConfettiNow()
    {
        // The goal callback can arrive before ShowWin activates this panel.
        // Leave the request unconsumed so ShowWin can play it once active.
        if (!gameObject.activeInHierarchy)
            return;

        confettiPlayedForCurrentWin = true;
        PlayConfetti();
    }

    public void PlayWinAnimationConfetti()
    {
        if (confettiPlayedForCurrentWin)
            return;

        if (!gameObject.activeSelf)
        {
            Show();
            SetWinScreenVisible(false);
        }

        PlayConfettiNow();
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
        {
            transform.SetAsLastSibling();
            if (confettiContainer != null)
                confettiContainer.SetAsLastSibling();
        }
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

        ConfettiPiece[] pieces = new ConfettiPiece[Mathf.Max(1, confettiAmount)];

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

        bool fromLeft = Random.value < 0.5f;
        bool hasBats = HasValidSprite(batSprites);
        bool hasBones = HasValidSprite(boneSprites);
        bool isBat = hasBats && (!hasBones || Random.value < 0.5f);
        Sprite selectedSprite = GetRandomSprite(isBat ? batSprites : boneSprites);

        if (selectedSprite == null)
        {
            isBat = !isBat;
            selectedSprite = GetRandomSprite(isBat ? batSprites : boneSprites);
        }

        float size = Random.Range(confettiSizeRange.x, confettiSizeRange.y);
        pieceRect.sizeDelta = (isBat ? new Vector2(880f, 880f) : new Vector2(720f, 720f)) * size;
        pieceRect.anchoredPosition = new Vector2(
            (fromLeft ? -1f : 1f) * width * 0.49f,
            Random.Range(height * emitterYRange.x, height * emitterYRange.y)
        );
        pieceRect.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

        Image image = pieceObject.GetComponent<Image>();
        image.sprite = selectedSprite;
        image.preserveAspect = true;
        image.color = Color.white;
        image.raycastTarget = false;

        activeConfetti.Add(pieceObject);

        float angle = fromLeft ? Random.Range(55f, 75f) : Random.Range(105f, 125f);
        float speed = Random.Range(confettiSpeedRange.x, confettiSpeedRange.y);
        return new ConfettiPiece
        {
            RectTransform = pieceRect,
            Image = image,
            Velocity = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * speed,
            RotationSpeed = Random.Range(-250f, 250f),
            Gravity = Random.Range(confettiGravityRange.x, confettiGravityRange.y),
            Lifetime = Random.Range(confettiLifetimeRange.x, confettiLifetimeRange.y),
            Delay = Random.value < 0.82f ? Random.Range(0f, 0.3f) : Random.Range(0.3f, 0.65f),
            NoiseStrength = Random.Range(15f, 35f),
            NoiseFrequency = Random.Range(0.4f, 0.8f),
            NoiseSeed = Random.Range(0f, 100f),
            IsBat = isBat
        };
    }

    private static bool HasValidSprite(Sprite[] sprites)
    {
        if (sprites == null)
            return false;

        for (int i = 0; i < sprites.Length; i++)
        {
            if (sprites[i] != null)
                return true;
        }

        return false;
    }

    private static Sprite GetRandomSprite(Sprite[] sprites)
    {
        if (sprites == null || sprites.Length == 0)
            return null;

        int startIndex = Random.Range(0, sprites.Length);
        for (int offset = 0; offset < sprites.Length; offset++)
        {
            Sprite sprite = sprites[(startIndex + offset) % sprites.Length];
            if (sprite != null)
                return sprite;
        }

        return null;
    }

    private void UpdateConfettiPiece(ConfettiPiece piece, float deltaTime)
    {
        if (piece.RectTransform == null)
            return;

        piece.Age += deltaTime;
        if (piece.Age < piece.Delay) { piece.Image.enabled = false; return; }
        piece.Image.enabled = true;
        float lifeAge = piece.Age - piece.Delay;
        if (lifeAge >= piece.Lifetime) { piece.Image.enabled = false; return; }

        piece.Velocity += Vector2.down * piece.Gravity * deltaTime;
        float noise = Mathf.PerlinNoise(piece.NoiseSeed, lifeAge * piece.NoiseFrequency) * 2f - 1f;
        piece.RectTransform.anchoredPosition += (piece.Velocity + Vector2.right * noise * piece.NoiseStrength) * deltaTime;
        float wobble = piece.IsBat ? Mathf.Sin((lifeAge + piece.NoiseSeed) * 5f) * 35f : 0f;
        piece.RectTransform.Rotate(0f, 0f, (piece.RotationSpeed + wobble) * deltaTime);

        float fadeStart = piece.Lifetime * 0.75f;
        float alpha = lifeAge <= fadeStart ? 1f : 1f - Mathf.InverseLerp(fadeStart, piece.Lifetime, lifeAge);
        piece.Image.color = new Color(1f, 1f, 1f, alpha);
    }

    private class ConfettiPiece
    {
        public RectTransform RectTransform;
        public Image Image;
        public Vector2 Velocity;
        public float RotationSpeed;
        public float Gravity;
        public float Lifetime;
        public float Delay;
        public float Age;
        public float NoiseStrength;
        public float NoiseFrequency;
        public float NoiseSeed;
        public bool IsBat;
    }
}
