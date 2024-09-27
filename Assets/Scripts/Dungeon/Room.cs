using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room   
{
    public string id;
    public string templateId;
    public GameObject prefab;
    public RoomNodeTypeSO roomNodeType;
    public Vector2Int lowerBounds;
    public Vector2Int upperBounds;

    public Vector2Int templateLowerBounds;
    public Vector2Int templateUpperBounds;
    public Vector2Int[] spawnPositionArray;
    public List<string> childRoomIdList;
    public string parentRoomId;
    public List<Doorway> doorWayList;
    public bool isPositioned = false;
    public InstantiateRoom instantiateRoom;
    public bool isLit = false;
    public bool isClearedOfEnemies = false;
    public bool isPreviouslyVisited = false;

    //enemy born
    public List<SpawnableObjectByLevel<EnemyDetailsSO>> enemiesByLevelList;
    public List<RoomEnemySpawnParamater> roomLevelEnemySpawnParamatersList;

    public Room()
    {
        childRoomIdList = new List<string>();
        doorWayList = new List<Doorway>();
    }

    public int GetNumberOfEnemiesToSpawn(DungeonLevelSO dungeonLevel)
    {
        foreach (RoomEnemySpawnParamater roomEnemySpawnParamater in roomLevelEnemySpawnParamatersList)
        {
            if (roomEnemySpawnParamater.dungeonLevel == dungeonLevel) {
                return Random.Range(roomEnemySpawnParamater.minTotalEnemiesToSpawn, roomEnemySpawnParamater.maxTotalEnemiesToSpawn);
            }
        }
        return 0;
    }

    public RoomEnemySpawnParamater GetRoomEnemySpawnParamater(DungeonLevelSO dungeonLevel)
    {
        foreach (RoomEnemySpawnParamater roomEnemySpawnParamater in roomLevelEnemySpawnParamatersList)
        {
            if (roomEnemySpawnParamater.dungeonLevel == dungeonLevel)
            {
                return roomEnemySpawnParamater;
            }
        }
        return null;
    }
}
