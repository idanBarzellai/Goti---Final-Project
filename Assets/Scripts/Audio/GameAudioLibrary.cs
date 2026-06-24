using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameAudioLibrary", menuName = "LaserPuzzle/Game Audio Library")]
public class GameAudioLibrary : ScriptableObject
{
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
