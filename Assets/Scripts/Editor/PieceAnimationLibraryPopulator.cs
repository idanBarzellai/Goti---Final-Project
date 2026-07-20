#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class PieceAnimationLibraryPopulator
{
    static PieceAnimationLibraryPopulator() { EditorApplication.delayCall += Populate; }

    private static void Populate()
    {
        string[] libraries = AssetDatabase.FindAssets("t:PieceSpriteLibrary", new[] { "Assets/Art/Game/New" });
        if (libraries.Length == 0) return;
        PieceSpriteLibrary library = AssetDatabase.LoadAssetAtPath<PieceSpriteLibrary>(AssetDatabase.GUIDToAssetPath(libraries[0]));
        if (library == null) return;
        var sets = new List<PieceSpriteLibrary.AnimationSet>();
        Add(sets, PieceType.Entry, "Assets/Art/Game/Animation/Entry/Idle");
        Add(sets, PieceType.Block, "Assets/Art/Game/Animation/Block");
        Add(sets, PieceType.Mirror, "Assets/Art/Game/Animation/Mirror");
        Add(sets, PieceType.Reflect, "Assets/Art/Game/Animation/Reflect");
        Add(sets, PieceType.Checkpoint, "Assets/Art/Game/Animation/Checkpoint");
        Add(sets, PieceType.Portal, "Assets/Art/Game/Animation/Portal");
        library.pieceAnimations = sets.ToArray();
        library.rollUpFrames = Load("Assets/Art/Game/Animation/Entry/RollUp");
        library.rollDownFrames = Load("Assets/Art/Game/Animation/Entry/RollDown");
        library.rollLeftFrames = Load("Assets/Art/Game/Animation/Entry/RollLeft");
        library.rollRightFrames = Load("Assets/Art/Game/Animation/Entry/RollRight");
        library.winFrames = Load("Assets/Art/Game/Animation/Entry/Win");
        Sprite fallbackEntryPoint = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Game/Animation/Entry/EntryPoint 1.png");
        library.rotatableEntryPointSprite = null;
        library.fixedEntryPointSprite = fallbackEntryPoint;
        foreach (var set in sets) if (set.idleFrames.Length > 0) SetBase(library, set.pieceType, set.idleFrames[0]);
        EditorUtility.SetDirty(library); AssetDatabase.SaveAssets();
    }

    private static void Add(List<PieceSpriteLibrary.AnimationSet> sets, PieceType type, string path) { sets.Add(new PieceSpriteLibrary.AnimationSet { pieceType = type, idleFrames = Load(path) }); }
    private static Sprite[] Load(string folder)
    {
        var list = new List<Sprite>();
        for (int i = 0; ; i++) { string path = $"{folder}/{i}.png"; if (!File.Exists(path)) break; Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path); if (sprite != null) list.Add(sprite); }
        return list.ToArray();
    }
    private static void SetBase(PieceSpriteLibrary l, PieceType t, Sprite s) { switch(t) { case PieceType.Entry:l.entrySprite=s;break; case PieceType.Block:l.blockSprite=s;break; case PieceType.Mirror:l.mirrorSprite=s;break;case PieceType.Reflect:l.reflectSprite=s;break;case PieceType.Checkpoint:l.checkpointSprite=s;break;case PieceType.Portal:l.portalSprite=s;break;} }
}
#endif
