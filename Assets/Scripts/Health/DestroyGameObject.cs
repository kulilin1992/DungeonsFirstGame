using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(DestoryedEvent))]
[DisallowMultipleComponent]
public class DestroyGameObject : MonoBehaviour
{
    private DestoryedEvent destoryedEvent;
    private void Awake()
    {
        destoryedEvent = GetComponent<DestoryedEvent>();
    }
    
    private void OnEnable()
    {
        destoryedEvent.OnDestroyed += Destory;
    }

    private void OnDisable()
    {
        destoryedEvent.OnDestroyed -= Destory;
    }

    private void Destory(DestoryedEvent destoryedEvent, DestoryedEventArgs args)
    {
        if (args.playerDied) {
            gameObject.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // private void Destory(DestoryedEvent destoryedEvent)
    // {
    //     Destroy(gameObject);
    // }
}
