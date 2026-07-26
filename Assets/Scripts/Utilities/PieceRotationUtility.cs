using System.Runtime.InteropServices;
using UnityEngine;

public static class PieceRotationUtility
{
    public static Direction RotateClockwise(Direction direction)
    {
        return (Direction)(((int)direction + 1) % 4);
    }

    public static float ToZRotation(Direction direction)
    {
        switch (direction)
        {
            case Direction.Up: return 0f;
            case Direction.Right: return -90f;
            case Direction.Down: return 180f;
            case Direction.Left: return 90f;
            default: return 0f;
        }
    }

    public static Vector2Int ToVector2Int(Direction direction)
    {
        switch (direction)
        {
            case Direction.Up: return new Vector2Int(0, 1);
            case Direction.Right: return new Vector2Int(1, 0);
            case Direction.Down: return new Vector2Int(0, -1);
            case Direction.Left: return new Vector2Int(-1, 0);
            default: return Vector2Int.zero;
        }
    }
}

public static class HapticFeedback
{
    public const int RotateDurationMilliseconds = 50;
    public const int PlaceDurationMilliseconds = 70;

#if UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern int WebGLVibrate(int durationMilliseconds, int pulseCount);
#endif

    public static void Vibrate(int durationMilliseconds, string source, int pulseCount = 1)
    {
        int safeDuration = Mathf.Clamp(durationMilliseconds, 1, 200);
        int safePulseCount = Mathf.Clamp(pulseCount, 1, 3);
        Debug.Log($"[Haptics] {source} vibration requested ({safeDuration}ms x {safePulseCount})");

#if UNITY_WEBGL
        if (!Application.isEditor)
        {
            int vibrationAccepted = WebGLVibrate(safeDuration, safePulseCount);
            Debug.Log(
                vibrationAccepted == 1
                    ? $"[Haptics] {source} browser vibration accepted"
                    : $"[Haptics] {source} browser vibration unavailable or rejected");
        }
#elif UNITY_ANDROID || UNITY_IOS
        Handheld.Vibrate();
#endif
    }
}
