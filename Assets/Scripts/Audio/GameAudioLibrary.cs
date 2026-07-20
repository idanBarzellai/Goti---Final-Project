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
    public AudioClip buttonClick;
    public List<AudioClip> buttonClickOptions = new List<AudioClip>();
    public AudioClip win;
    public AudioClip lose;
    public List<AudioClip> loseOptions = new List<AudioClip>();
    public AudioClip fireLaser;
    public List<AudioClip> fireLaserOptions = new List<AudioClip>();
    public AudioClip placePiece;
    public List<AudioClip> placePieceOptions = new List<AudioClip>();
    public AudioClip rotatePiece;
    public List<AudioClip> rotatePieceOptions = new List<AudioClip>();
    public AudioClip returnPiece;
    public List<AudioClip> bumpOptions = new List<AudioClip>();
    public List<AudioClip> whooshOptions = new List<AudioClip>();

    public AudioClip GetButtonClickSound()
    {
        return GetRandomClip(buttonClick, buttonClickOptions);
    }

    public AudioClip GetPlacePieceSound()
    {
        return GetRandomClip(placePiece, placePieceOptions);
    }

    public AudioClip GetRotatePieceSound()
    {
        return GetRandomClip(rotatePiece, rotatePieceOptions);
    }

    public AudioClip GetFireLaserSound()
    {
        return GetRandomClip(fireLaser, fireLaserOptions);
    }

    public AudioClip GetLoseSound()
    {
        return GetRandomClip(lose, loseOptions);
    }

    public AudioClip GetBumpSound() => GetRandomClip(null, bumpOptions);
    public AudioClip GetWhooshSound() => GetRandomClip(null, whooshOptions);

    private AudioClip GetRandomClip(AudioClip defaultClip, List<AudioClip> options)
    {
        List<AudioClip> availableClips = new List<AudioClip>();

        if (defaultClip != null)
            availableClips.Add(defaultClip);

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
