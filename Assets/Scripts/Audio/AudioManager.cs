using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    private const string AudioLibraryResourcePath = "GameAudioLibrary";
    private const string MutedPlayerPrefsKey = "AudioMuted";

    [SerializeField] private GameAudioLibrary audioLibrary;

    [Header("Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource rollSource;

    private bool isMuted;

    public bool IsMuted => isMuted;
    public event Action<bool> OnMuteChanged;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance != null)
            return;

        GameObject audioManagerObject = new GameObject("AudioManager");
        audioManagerObject.AddComponent<AudioManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (audioLibrary == null)
            audioLibrary = Resources.Load<GameAudioLibrary>(AudioLibraryResourcePath);

        isMuted = PlayerPrefs.GetInt(MutedPlayerPrefsKey, 0) == 1;
        EnsureAudioSources();
        ApplyMuteState();
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

        if (rollSource == null)
            rollSource = gameObject.AddComponent<AudioSource>();

        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        bgmSource.volume = BgmVolume;
        bgmSource.mute = isMuted;

        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
        sfxSource.volume = 1f;
        sfxSource.mute = isMuted;

        rollSource.loop = true;
        rollSource.playOnAwake = false;
        rollSource.volume = SfxVolume;
        rollSource.mute = isMuted;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StopRoll();
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
        if (bgmSource == null)
            return;

        if (clip == null)
        {
            bgmSource.Stop();
            bgmSource.clip = null;
            return;
        }

        if (bgmSource.clip == clip && bgmSource.isPlaying)
            return;

        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.volume = BgmVolume;
        bgmSource.mute = isMuted;
        bgmSource.Play();
    }

    public void PlayButtonClick() => PlaySfx(audioLibrary != null ? audioLibrary.GetButtonClickSound() : null);
    public void PlayWinArrival() => PlaySfx(audioLibrary != null ? audioLibrary.GetWinArrivalSound() : null);
    public void PlayWin() => PlaySfx(audioLibrary?.win);
    public void PlayLose() => PlaySfx(audioLibrary != null ? audioLibrary.GetLoseSound() : null);
    public void PlayFireLaser() => PlaySfx(audioLibrary != null ? audioLibrary.GetFireLaserSound() : null);
    public void PlayPlacePiece() => PlaySfx(audioLibrary != null ? audioLibrary.GetPlacePieceSound() : null);
    public void PlayRotatePiece() => PlaySfx(audioLibrary != null ? audioLibrary.GetRotatePieceSound() : null);
    public void PlayReturnPiece() => PlaySfx(audioLibrary?.returnPiece);
    public void PlayBump() => PlaySfx(audioLibrary != null ? audioLibrary.GetBumpSound() : null);
    public void PlayWhoosh() => PlaySfx(audioLibrary != null ? audioLibrary.GetWhooshSound() : null);

    public void StartRoll()
    {
        AudioClip clip = audioLibrary != null ? audioLibrary.roll : null;
        if (clip == null || rollSource == null)
            return;

        if (rollSource.clip == clip && rollSource.isPlaying)
            return;

        rollSource.clip = clip;
        rollSource.volume = SfxVolume;
        rollSource.Play();
    }

    public void StopRoll()
    {
        if (rollSource == null)
            return;

        rollSource.Stop();
        rollSource.clip = null;
    }

    public void PlaySfx(AudioClip clip)
    {
        if (isMuted || clip == null || sfxSource == null)
            return;

        sfxSource.PlayOneShot(clip, SfxVolume);
    }

    public void ToggleMuted()
    {
        SetMuted(!isMuted);
    }

    public void SetMuted(bool muted)
    {
        if (isMuted == muted)
            return;

        isMuted = muted;
        PlayerPrefs.SetInt(MutedPlayerPrefsKey, isMuted ? 1 : 0);
        PlayerPrefs.Save();
        ApplyMuteState();
        OnMuteChanged?.Invoke(isMuted);
    }

    private void ApplyMuteState()
    {
        if (bgmSource != null)
            bgmSource.mute = isMuted;

        if (sfxSource != null)
            sfxSource.mute = isMuted;

        if (rollSource != null)
            rollSource.mute = isMuted;
    }

    private float BgmVolume => audioLibrary != null ? audioLibrary.bgmVolume : 0.6f;
    private float SfxVolume => audioLibrary != null ? audioLibrary.sfxVolume : 1f;

}
