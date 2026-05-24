using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private GameAudioLibrary audioLibrary;

    [Header("Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Volume")]
    [SerializeField, Range(0f, 1f)] private float bgmVolume = 0.6f;
    [SerializeField, Range(0f, 1f)] private float sfxVolume = 1f;

    [Header("Piece Hit Sequence")]
    [SerializeField] private float pieceHitDelay = 0.08f;

    private Coroutine pieceHitRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureAudioSources();
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void Start()
    {
        PlayBgmForScene(SceneManager.GetActiveScene().name);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void EnsureAudioSources()
    {
        if (bgmSource == null)
            bgmSource = gameObject.AddComponent<AudioSource>();

        if (sfxSource == null)
            sfxSource = gameObject.AddComponent<AudioSource>();

        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        bgmSource.volume = bgmVolume;

        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
        sfxSource.volume = sfxVolume;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayBgmForScene(scene.name);
    }

    private void PlayBgmForScene(string sceneName)
    {
        if (audioLibrary == null)
            return;

        AudioClip clipToPlay = null;

        if (sceneName == SceneFlowManager.MainMenuSceneName)
            clipToPlay = audioLibrary.mainMenuBgm;
        else if (sceneName == SceneFlowManager.GameSceneName)
            clipToPlay = audioLibrary.gameplayBgm;

        PlayBgm(clipToPlay);
    }

    private void PlayBgm(AudioClip clip)
    {
        if (clip == null || bgmSource == null)
            return;

        if (bgmSource.clip == clip && bgmSource.isPlaying)
            return;

        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.volume = bgmVolume;
        bgmSource.Play();
    }

    public void PlayButtonClick() => PlaySfx(audioLibrary?.buttonClick);
    public void PlayWin() => PlaySfx(audioLibrary?.win);
    public void PlayLose() => PlaySfx(audioLibrary?.lose);
    public void PlayFireLaser() => PlaySfx(audioLibrary?.fireLaser);
    public void PlayPlacePiece() => PlaySfx(audioLibrary?.placePiece);
    public void PlayMovePiece() => PlaySfx(audioLibrary?.movePiece);
    public void PlayRotatePiece() => PlaySfx(audioLibrary?.rotatePiece);
    public void PlayReturnPiece() => PlaySfx(audioLibrary?.returnPiece);

    public void PlaySfx(AudioClip clip)
    {
        if (clip == null || sfxSource == null)
            return;

        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    public void PlayPieceHitSequence(List<BoardPiece> hitPieces)
    {
        if (hitPieces == null || hitPieces.Count == 0)
            return;

        if (pieceHitRoutine != null)
            StopCoroutine(pieceHitRoutine);

        pieceHitRoutine = StartCoroutine(PieceHitSequenceRoutine(hitPieces));
    }

    private IEnumerator PieceHitSequenceRoutine(List<BoardPiece> hitPieces)
    {
        foreach (BoardPiece piece in hitPieces)
        {
            if (piece == null || audioLibrary == null)
                continue;

            AudioClip clip = audioLibrary.GetPieceHitSound(piece.PieceType);
            PlaySfx(clip);

            yield return new WaitForSeconds(pieceHitDelay);
        }

        pieceHitRoutine = null;
    }
}