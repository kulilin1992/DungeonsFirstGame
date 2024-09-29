using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class StaticEventHandler
{
    public static event Action<RoomChangedEventArgs> OnRoomChanged;
    public static void CallRoomChangedEvent(Room room) {
        OnRoomChanged?.Invoke(new RoomChangedEventArgs { room = room });
    }

    public static event Action<RoomEnemiesDefeatedArgs> OnRoomEnemiesDefeated;
    public static void CallRoomEnemiesDefeatedEvent(Room room) {
        OnRoomEnemiesDefeated?.Invoke(new RoomEnemiesDefeatedArgs { room = room });
    }

    public static event Action<PointsScoredArgs> OnPointsScored;
    public static void CallPointsScoredEvent(int points) {
        OnPointsScored?.Invoke(new PointsScoredArgs { points = points });
    }

    public static event Action<ScoreChangedArgs> OnScoreChanged;
    public static void CallScoreChangedEvent(long score, int multiplier) {
        OnScoreChanged?.Invoke(new ScoreChangedArgs { score = score ,   multiplier = multiplier });
    }

    public static event Action<multiplierArgs> OnMultiplierChanged;
    
    public static void CallMultiplierChangedEvent(bool multiplier) {
        OnMultiplierChanged?.Invoke(new multiplierArgs { multiplier = multiplier });
    }
}

public class RoomChangedEventArgs : EventArgs
{
    public Room room;
}

public class RoomEnemiesDefeatedArgs : EventArgs
{
    public Room room;
}

public class PointsScoredArgs : EventArgs
{
    public int points;
}
public class ScoreChangedArgs : EventArgs
{
    public long score;
    public int multiplier;
}
public class multiplierArgs : EventArgs
{
    public bool multiplier;
}
