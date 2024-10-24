using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 定义一个Room类，用于表示房间
public class Room   
{
    // 房间ID
    public string id;
    // 模板ID
    public string templateId;
    // 预制体
    public GameObject prefab;
    // 房间节点类型
    public RoomNodeTypeSO roomNodeType;
    // 房间下界
    public Vector2Int lowerBounds;
    // 房间上界
    public Vector2Int upperBounds;

    // 模板下界
    public Vector2Int templateLowerBounds;
    // 模板上界
    public Vector2Int templateUpperBounds;
    // 生成位置数组
    public Vector2Int[] spawnPositionArray;
    // 子房间ID列表
    public List<string> childRoomIdList;
    // 父房间ID
    public string parentRoomId;
    // 门列表
    public List<Doorway> doorWayList;
    // 是否定位
    public bool isPositioned = false;
    // 实例化房间
    public InstantiateRoom instantiateRoom;
    // 是否照亮
    public bool isLit = false;
    // 是否清除敌人
    public bool isClearedOfEnemies = false;
    // 是否已访问
    public bool isPreviouslyVisited = false;

    //enemy born
    public List<SpawnableObjectByLevel<EnemyDetailsSO>> enemiesByLevelList;
    public List<RoomEnemySpawnParamater> roomLevelEnemySpawnParamatersList;


    //game music
    public MusicTrackSO battleMusic;
    public MusicTrackSO ambientMusic;

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
