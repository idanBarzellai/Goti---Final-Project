using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameAudioLibrary", menuName = "LaserPuzzle/Game Audio Library")]
public class GameAudioLibrary : ScriptableObject
{
    [Header("Master Volume")]
    [Range(0f, 1f)] public float bgmVolume = 0.6f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    [Header("BGM")]
    public AudioClip mainMenuBgm;
    public AudioClip gameplayBgm;

    [Header("General SFX")]
    public List<AudioClip> buttonClickOptions = new List<AudioClip>();
    [Tooltip("One available sound is chosen at random when GOTI reaches the grave.")]
    public List<AudioClip> winArrivalOptions = new List<AudioClip>();
    [Tooltip("Played together with the smile animation and confetti.")]
    public AudioClip win;
    [Tooltip("Looped while GOTI rolls across the board.")]
    public AudioClip roll;
    public List<AudioClip> loseOptions = new List<AudioClip>();
    public List<AudioClip> fireLaserOptions = new List<AudioClip>();
    public List<AudioClip> placePieceOptions = new List<AudioClip>();
    public List<AudioClip> rotatePieceOptions = new List<AudioClip>();
    public AudioClip returnPiece;
    public List<AudioClip> bumpOptions = new List<AudioClip>();
    public List<AudioClip> whooshOptions = new List<AudioClip>();

    public AudioClip GetButtonClickSound()
    {
        return GetRandomClip(buttonClickOptions);
    }

    public AudioClip GetWinArrivalSound()
    {
        return GetRandomClip(winArrivalOptions);
    }

    public AudioClip GetPlacePieceSound()
    {
        return GetRandomClip(placePieceOptions);
    }

    public AudioClip GetRotatePieceSound()
    {
        return GetRandomClip(rotatePieceOptions);
    }

    public AudioClip GetFireLaserSound()
    {
        return GetRandomClip(fireLaserOptions);
    }

    public AudioClip GetLoseSound()
    {
        return GetRandomClip(loseOptions);
    }

    public AudioClip GetBumpSound() => GetRandomClip(bumpOptions);
    public AudioClip GetWhooshSound() => GetRandomClip(whooshOptions);

    private AudioClip GetRandomClip(List<AudioClip> options)
    {
        List<AudioClip> availableClips = new List<AudioClip>();

        if (options == null)
            return null;

        for (int i = 0; i < options.Count; i++)
        {
            if (options[i] != null)
                availableClips.Add(options[i]);
        }

        if (availableClips.Count == 0)
            return null;

        return availableClips[Random.Range(0, availableClips.Count)];
    }
}
