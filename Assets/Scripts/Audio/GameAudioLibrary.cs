using System;
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
    public AudioClip win;
    public AudioClip lose;
    public AudioClip fireLaser;
    public AudioClip placePiece;
    public AudioClip movePiece;
    public AudioClip rotatePiece;
    public AudioClip returnPiece;

    [Header("Piece Hit SFX")]
    public List<PieceAudioEntry> pieceHitSounds = new List<PieceAudioEntry>();

    public AudioClip GetPieceHitSound(PieceType pieceType)
    {
        foreach (PieceAudioEntry entry in pieceHitSounds)
        {
            if (entry.pieceType == pieceType)
                return entry.clip;
        }

        return null;
    }
}

[Serializable]
public class PieceAudioEntry
{
    public PieceType pieceType;
    public AudioClip clip;
}