
using UnityEngine;

[System.Serializable]
public class RoomEnemySpawnParamater
{
    public DungeonLevelSO dungeonLevel;
    public int minTotalEnemiesToSpawn;
    public int maxTotalEnemiesToSpawn;

    public int minConcurrentEnemies;
    public int maxConcurrentEnemies;

    public int minSpawnInterval;
    public int maxSpawnInterval;
}
