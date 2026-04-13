public static class LaserReflectionUtility
{
    public static bool TryReflect(Direction incomingDirection, Direction mirrorDirection, out Direction reflectedDirection)
    {
        bool slashMirror = mirrorDirection == Direction.Up || mirrorDirection == Direction.Down;

        if (slashMirror)
        {
            // "/" mirror
            switch (incomingDirection)
            {
                case Direction.Up:
                    reflectedDirection = Direction.Right;
                    return true;
                case Direction.Right:
                    reflectedDirection = Direction.Up;
                    return true;
                case Direction.Down:
                    reflectedDirection = Direction.Left;
                    return true;
                case Direction.Left:
                    reflectedDirection = Direction.Down;
                    return true;
            }
        }
        else
        {
            // "\" mirror
            switch (incomingDirection)
            {
                case Direction.Up:
                    reflectedDirection = Direction.Left;
                    return true;
                case Direction.Left:
                    reflectedDirection = Direction.Up;
                    return true;
                case Direction.Down:
                    reflectedDirection = Direction.Right;
                    return true;
                case Direction.Right:
                    reflectedDirection = Direction.Down;
                    return true;
            }
        }

        reflectedDirection = incomingDirection;
        return false;
    }
}