using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "Room_", menuName = "Scriptable Objects/Dungeon/Room")]
public class RoomTemplateSO : ScriptableObject
{
    [HideInInspector] public string guid;

    #region Header ROOM PREFAB

    [Space(10)]
    [Header("ROOM PREFAB")]

    #endregion Header ROOM PREFAB

    #region Tooltip

    [Tooltip("The gameobject prefab for the room (this will contain all the tilemaps for the room and environment game objects")]

    #endregion Tooltip

    public GameObject prefab;

    [HideInInspector] public GameObject previousPrefab; // this is used to regenerate the guid if the so is copied and the prefab is changed


    #region Header ROOM CONFIGURATION

    [Space(10)]
    [Header("ROOM CONFIGURATION")]

    #endregion Header ROOM CONFIGURATION

    #region Tooltip

    [Tooltip("The room node type SO. The room node types correspond to the room nodes used in the room node graph.  The exceptions being with corridors.  In the room node graph there is just one corridor type 'Corridor'.  For the room templates there are 2 corridor node types - CorridorNS and CorridorEW.")]

    #endregion Tooltip

    public RoomNodeTypeSO roomNodeType;

    #region Tooltip

    [Tooltip("If you imagine a rectangle around the room tilemap that just completely encloses it, the room lower bounds represent the bottom left corner of that rectangle. This should be determined from the tilemap for the room (using the coordinate brush pointer to get the tilemap grid position for that bottom left corner (Note: this is the local tilemap position and NOT world position")]

    #endregion Tooltip

    public Vector2Int lowerBounds;

    #region Tooltip

    [Tooltip("If you imagine a rectangle around the room tilemap that just completely encloses it, the room upper bounds represent the top right corner of that rectangle. This should be determined from the tilemap for the room (using the coordinate brush pointer to get the tilemap grid position for that top right corner (Note: this is the local tilemap position and NOT world position")]

    #endregion Tooltip

    public Vector2Int upperBounds;

    #region Tooltip

    [Tooltip("There should be a maximum of four doorways for a room - one for each compass direction.  These should have a consistent 3 tile opening size, with the middle tile position being the doorway coordinate 'position'")]

    #endregion Tooltip

    [SerializeField] public List<Doorway> doorwayList;

    #region Tooltip

    [Tooltip("Each possible spawn position (used for enemies and chests) for the room in tilemap coordinates should be added to this array")]

    #endregion Tooltip

    public Vector2Int[] spawnPositionArray;


    //enemy spawn born
    public List<SpawnableObjectByLevel<EnemyDetailsSO>> enemiesByLevelList;

    public List<RoomEnemySpawnParamater> roomEnemySpawnParamaterList;

    //game music
    public MusicTrackSO battleMusic;
    public MusicTrackSO ambientMusic;
    //public MusicTrackSO bossMusic;

    /// <summary>
    /// Returns the list of Entrances for the room template
    /// </summary>
    public List<Doorway> GetDoorwayList()
    {
        return doorwayList;
    }

    #region Validation

#if UNITY_EDITOR

    // Validate SO fields
    private void OnValidate()
    {
        // Set unique GUID if empty or the prefab changes
        if (guid == "" || previousPrefab != prefab)
        {
            guid = GUID.Generate().ToString();
            previousPrefab = prefab;
            EditorUtility.SetDirty(this);
        }

        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(doorwayList), doorwayList);

        // Check spawn positions populated
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(spawnPositionArray), spawnPositionArray);
        HelperUtilities.ValidateCheckNullValue(this, nameof(prefab), prefab);
        HelperUtilities.ValidateCheckNullValue(this, nameof(roomNodeType), roomNodeType);

        if (enemiesByLevelList.Count > 0 || roomEnemySpawnParamaterList.Count > 0) {
            HelperUtilities.ValidateCheckEnumerableValues(this, nameof(enemiesByLevelList), enemiesByLevelList);
            HelperUtilities.ValidateCheckEnumerableValues(this, nameof(roomEnemySpawnParamaterList), roomEnemySpawnParamaterList);

            foreach (RoomEnemySpawnParamater roomEnemySpawnParamater in roomEnemySpawnParamaterList) {
                HelperUtilities.ValidateCheckNullValue(this, nameof(roomEnemySpawnParamater.dungeonLevel), roomEnemySpawnParamater.dungeonLevel);
                HelperUtilities.ValidateCheckPositiveRange(this, nameof(roomEnemySpawnParamater.minTotalEnemiesToSpawn),
                    roomEnemySpawnParamater.minTotalEnemiesToSpawn, nameof(roomEnemySpawnParamater.maxTotalEnemiesToSpawn),
                    roomEnemySpawnParamater.maxTotalEnemiesToSpawn, true);
                HelperUtilities.ValidateCheckPositiveRange(this, nameof(roomEnemySpawnParamater.minSpawnInterval),
                    roomEnemySpawnParamater.minSpawnInterval, nameof(roomEnemySpawnParamater.maxSpawnInterval),
                    roomEnemySpawnParamater.maxSpawnInterval, true);
                HelperUtilities.ValidateCheckPositiveRange(this, nameof(roomEnemySpawnParamater.minConcurrentEnemies),
                    roomEnemySpawnParamater.minConcurrentEnemies, nameof(roomEnemySpawnParamater.maxConcurrentEnemies),
                    roomEnemySpawnParamater.maxConcurrentEnemies, true);

                bool isEnemyTypesListForDungeonLevel = false;

                //validate enemy type list
                foreach (SpawnableObjectByLevel<EnemyDetailsSO> dungeonObjectByLevel in enemiesByLevelList) {
                    if (dungeonObjectByLevel.dungeonLevel == roomEnemySpawnParamater.dungeonLevel &&
                        dungeonObjectByLevel.spawnableObjectRatioList.Count > 0) {
                        isEnemyTypesListForDungeonLevel = true;
                    
                    HelperUtilities.ValidateCheckNullValue(this, nameof(dungeonObjectByLevel.dungeonLevel), dungeonObjectByLevel.dungeonLevel);

                    foreach (SpawnableObjectRatio<EnemyDetailsSO> spawnableObjectRatio in dungeonObjectByLevel.spawnableObjectRatioList) {
                        HelperUtilities.ValidateCheckNullValue(this, nameof(spawnableObjectRatio.dungeonObject), spawnableObjectRatio.dungeonObject);
                        HelperUtilities.ValidateCheckPositiveValue(this, nameof(spawnableObjectRatio.ratio), spawnableObjectRatio.ratio, false);
                    }

                    }
                    if (isEnemyTypesListForDungeonLevel == false && roomEnemySpawnParamater.dungeonLevel != null) {
                        Debug.Log($"No enemy types found for dungeon level {roomEnemySpawnParamater.dungeonLevel.levelName} in {name}.");
                    }
                }
            }
        }
    }

#endif

    #endregion Validation
}