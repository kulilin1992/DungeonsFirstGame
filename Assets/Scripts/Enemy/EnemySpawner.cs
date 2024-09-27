
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class EnemySpawner : SingletonMonobehaviour<EnemySpawner>
{
    private int enemiesToSpawn;
    private int currentEnemyCount;
    private int enemiesSpawnedSoFar;
    private int enemyMaxConcurrentSpawnNumber;
    private Room currentRoom;
    private RoomEnemySpawnParamater roomEnemySpawnParameter;

    private void OnEnable()
    {
        StaticEventHandler.OnRoomChanged += StaticEventHandler_OnRoomChanged;
    }

    private void OnDisable()
    {
        StaticEventHandler.OnRoomChanged -= StaticEventHandler_OnRoomChanged;
    }

    private void StaticEventHandler_OnRoomChanged(RoomChangedEventArgs roomChangedEventArgs)
    {
        enemiesSpawnedSoFar = 0;
        currentEnemyCount = 0;
        currentRoom = roomChangedEventArgs.room;

        if (currentRoom.roomNodeType.isCorridorEW || currentRoom.roomNodeType.isCorridorNS)
            return;

        if (currentRoom.isClearedOfEnemies) return;

        enemiesToSpawn = currentRoom.GetNumberOfEnemiesToSpawn(GameManager.Instance.GetCurrentDungeonLevel());

        roomEnemySpawnParameter = currentRoom.GetRoomEnemySpawnParamater(GameManager.Instance.GetCurrentDungeonLevel());

        if (enemiesToSpawn == 0) {
            currentRoom.isClearedOfEnemies = true;
            return;
        }

        enemyMaxConcurrentSpawnNumber = GetConcurrentEnemies();

        currentRoom.instantiateRoom.LockDoors();

        SpawnEnemies();
    
    }

    private int GetConcurrentEnemies()
    {
         return Random.Range(roomEnemySpawnParameter.minConcurrentEnemies, roomEnemySpawnParameter.maxConcurrentEnemies);
    }

    private void SpawnEnemies()
    {
        if (GameManager.Instance.gameState == GameState.playingLevel) {
            GameManager.Instance.previousGameState = GameState.playingLevel;
            GameManager.Instance.gameState = GameState.engagingEnemies;
        }
        StartCoroutine(SpawnEnemiesRoutine());
    }

    private IEnumerator SpawnEnemiesRoutine()
    {
        Grid grid = currentRoom.instantiateRoom.grid;

        RandomSpawnableObject<EnemyDetailsSO> randomSpawnableObject = new RandomSpawnableObject<EnemyDetailsSO>(currentRoom.enemiesByLevelList);

        if (currentRoom.spawnPositionArray.Length > 0) {
            for (int i = 0; i < enemiesToSpawn; i++) {
                while (currentEnemyCount >= enemyMaxConcurrentSpawnNumber) {
                    yield return null;
                }
                Vector3Int cellPosition = (Vector3Int)currentRoom.spawnPositionArray[Random.Range(0, currentRoom.spawnPositionArray.Length)];
                CreateEnemy(randomSpawnableObject.GetItem(), grid.CellToWorld(cellPosition));

                yield return new WaitForSeconds(GetEnemySpawnInterval());
            }
        }
    }

    private void CreateEnemy(EnemyDetailsSO enemyDetails, Vector3 position)
    {
        enemiesSpawnedSoFar++;

        currentEnemyCount++;

        DungeonLevelSO dungeonLevel = GameManager.Instance.GetCurrentDungeonLevel();

        GameObject enemy = Instantiate(enemyDetails.enemyPrefab, position, Quaternion.identity,transform);

        enemy.GetComponent<Enemy>().EnemyInit(enemyDetails, enemiesSpawnedSoFar, dungeonLevel);

        enemy.GetComponent<DestoryedEvent>().OnDestroyed += Enemy_OnDestroyed;
    }

    private void Enemy_OnDestroyed(DestoryedEvent destoryedEvent, DestoryedEventArgs destoryedEventArgs)
    {
        destoryedEvent.OnDestroyed -= Enemy_OnDestroyed;

        currentEnemyCount--;

        if (currentEnemyCount <= 0 && enemiesSpawnedSoFar == enemiesToSpawn) {
            currentRoom.isClearedOfEnemies = true;
            
            if (GameManager.Instance.gameState == GameState.playingLevel) {
                GameManager.Instance.previousGameState = GameState.engagingEnemies;

            }
            else if (GameManager.Instance.gameState == GameState.engagingBoss) {
            
                GameManager.Instance.gameState = GameState.bossStage;
                GameManager.Instance.previousGameState = GameState.engagingBoss;

            }

            currentRoom.instantiateRoom.UnlockDoors(Settings.doorUnlockDelay);

            StaticEventHandler.CallRoomEnemiesDefeatedEvent(currentRoom);
        }
    }
    

    private float GetEnemySpawnInterval()
    {
        return Random.Range(roomEnemySpawnParameter.minSpawnInterval, roomEnemySpawnParameter.maxSpawnInterval);
    }
}
