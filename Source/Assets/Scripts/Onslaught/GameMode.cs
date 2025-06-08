using UnityEngine;

public static class GameMode
{
    public enum Mode
    {
        Normal,
        Onslaught
    }

    public static Mode Current = Mode.Normal;
}