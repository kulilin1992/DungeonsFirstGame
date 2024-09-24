using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


[DisallowMultipleComponent]
[RequireComponent(typeof(InstantiateRoom))]
public class RoomLightingController : MonoBehaviour
{
    private InstantiateRoom instantiateRoom;

    private void Awake()
    {
        instantiateRoom = GetComponent<InstantiateRoom>();
    }
    private void OnEnable()
    {
        StaticEventHandler.OnRoomChanged += OnRoomChanged;
    }
    void OnDisable()
    {
        StaticEventHandler.OnRoomChanged -= OnRoomChanged;
    }

    private void OnRoomChanged(RoomChangedEventArgs args)
    {
        if (args.room == instantiateRoom.room && !instantiateRoom.room.isLit) {
            FadeInRoomLighting();
            FadeInDoors();

            instantiateRoom.room.isLit = true;
        }
    }

    private void FadeInRoomLighting()
    {
        StartCoroutine(FadeInRoomLightingRoutine(instantiateRoom));
    }

    private IEnumerator FadeInRoomLightingRoutine(InstantiateRoom instantiateRoom)
    {
        Material material = new Material(GameResources.Instance.variableLitShader);
        instantiateRoom.groundTilemap.GetComponent<TilemapRenderer>().material = material;
        instantiateRoom.decoration1Tilemap.GetComponent<TilemapRenderer>().material = material;
        instantiateRoom.decoration2Tilemap.GetComponent<TilemapRenderer>().material = material;
        instantiateRoom.frontTilemap.GetComponent<TilemapRenderer>().material = material;
        instantiateRoom.minimapTilemap.GetComponent<TilemapRenderer>().material = material;

        for (float i = 0.05f; i <= 1f; i+=Time.deltaTime / Settings.fadeInTime) {
            material.SetFloat("Alpha_Slider", i);
            yield return null;
        }

        instantiateRoom.groundTilemap.GetComponent<TilemapRenderer>().material = GameResources.Instance.litDefaultMaterial;
        instantiateRoom.decoration1Tilemap.GetComponent<TilemapRenderer>().material = GameResources.Instance.litDefaultMaterial;
        instantiateRoom.decoration2Tilemap.GetComponent<TilemapRenderer>().material = GameResources.Instance.litDefaultMaterial;
        instantiateRoom.frontTilemap.GetComponent<TilemapRenderer>().material = GameResources.Instance.litDefaultMaterial;
        instantiateRoom.minimapTilemap.GetComponent<TilemapRenderer>().material = GameResources.Instance.litDefaultMaterial;
    }

    private void FadeInDoors()
    {
        Door[] doors = GetComponentsInChildren<Door>();
        foreach (Door door in doors) {
            DoorLightingController doorLightingController = door.GetComponentInChildren<DoorLightingController>();
            doorLightingController.FadeInDoor(door);
        }
    }
}
