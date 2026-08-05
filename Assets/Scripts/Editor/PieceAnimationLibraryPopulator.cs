#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
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

        const string pieceSheets = "Assets/Art/Game/Animation/pieces spritesheets";
        const string entrySheets = "Assets/Art/Game/Animation/entry spritesheets";
        var sets = new List<PieceSpriteLibrary.AnimationSet>();
        Add(sets, PieceType.Entry, LoadSpriteSheet($"{entrySheets}/idle.png"));
        Add(sets, PieceType.Block, LoadSpriteSheet($"{pieceSheets}/block.png"));
        Add(sets, PieceType.Mirror, LoadSpriteSheet($"{pieceSheets}/mirror.png"));
        Add(sets, PieceType.Reflect, LoadSpriteSheet($"{pieceSheets}/reflect.png"));
        Add(sets, PieceType.Checkpoint, LoadSpriteSheet($"{pieceSheets}/checkpoint.png"));
        Add(sets, PieceType.Portal, LoadSpriteSheet($"{pieceSheets}/portal.png"));
        library.pieceAnimations = sets.ToArray();
        library.rollUpFrames = LoadSpriteSheet($"{entrySheets}/rollup.png");
        library.rollDownFrames = LoadSpriteSheet($"{entrySheets}/rolldown.png");
        library.rollLeftFrames = LoadSpriteSheet($"{entrySheets}/rollleft.png");
        library.rollRightFrames = LoadSpriteSheet($"{entrySheets}/rollright.png");
        library.winFrames = LoadSpriteSheet($"{entrySheets}/win.png");
        library.loseFrames = LoadSpriteSheet($"{entrySheets}/lose.png");
        Sprite fallbackEntryPoint = LoadSpriteSheet($"{entrySheets}/EntryPoint 1.png").FirstOrDefault();
        library.rotatableEntryPointSprite = null;
        library.fixedEntryPointSprite = fallbackEntryPoint;
        foreach (var set in sets) if (set.idleFrames.Length > 0) SetBase(library, set.pieceType, set.idleFrames[0]);
        EditorUtility.SetDirty(library); AssetDatabase.SaveAssets();
    }

    private static void Add(List<PieceSpriteLibrary.AnimationSet> sets, PieceType type, Sprite[] frames) { sets.Add(new PieceSpriteLibrary.AnimationSet { pieceType = type, idleFrames = frames }); }

    private static Sprite[] LoadSpriteSheet(string path)
    {
        return AssetDatabase.LoadAllAssetsAtPath(path)
            .OfType<Sprite>()
            .OrderBy(sprite => GetFrameIndex(sprite.name))
            .ThenBy(sprite => sprite.name)
            .ToArray();
    }

    private static int GetFrameIndex(string spriteName)
    {
        int separatorIndex = spriteName.LastIndexOf('_');
        return separatorIndex >= 0 && int.TryParse(spriteName.Substring(separatorIndex + 1), out int index)
            ? index
            : int.MaxValue;
    }

    private static void SetBase(PieceSpriteLibrary l, PieceType t, Sprite s) { switch(t) { case PieceType.Entry:l.entrySprite=s;break; case PieceType.Block:l.blockSprite=s;break; case PieceType.Mirror:l.mirrorSprite=s;break;case PieceType.Reflect:l.reflectSprite=s;break;case PieceType.Checkpoint:l.checkpointSprite=s;break;case PieceType.Portal:l.portalSprite=s;break;} }
}
#endif
