using System;
using UnityEngine;

[DisallowMultipleComponent]
public class DestoryedEvent : MonoBehaviour
{
    //public event Action<DestoryedEvent> OnDestroyed;
    public event Action<DestoryedEvent, DestoryedEventArgs> OnDestroyed;

    // public void CallDestoryedEvent()
    // {
    //     OnDestroyed?.Invoke(this);
    // }
    public void CallDestoryedEvent(bool playerDied)
    {
        OnDestroyed?.Invoke(this, new DestoryedEventArgs { playerDied = playerDied });
    }
}

public class DestoryedEventArgs : EventArgs
{
    public bool playerDied;
}