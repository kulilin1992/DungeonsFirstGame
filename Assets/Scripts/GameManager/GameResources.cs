using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Tilemaps;

public class GameResources : MonoBehaviour
{
    private static GameResources instance;
    public static GameResources Instance 
    {
        get
        {
            if (instance == null)
            {
                instance = Resources.Load<GameResources>("GameResources");
                //instance = FindObjectOfType<GameResources>();
                //DontDestroyOnLoad(instance);
            }
            return instance;
        }
    }

    #region Header DUNGEON
    [Space(10)]
    [Header("DUNGEON")]
    #endregion
    #region Tooltip
    [Tooltip("Populated with the dungeon RoomNodeTypeListSO")]
    #endregion
    public RoomNodeTypeListSO roomNodeTypeList;

    [Space(10)]
    [Header("MATERIALS")]
    [Tooltip("Dimmed Material")]
    public Material dimmedMaterial;

    #region Header PLAYER
    [Space(10)]
    [Header("PLAYER")]
    #endregion Header PLAYER
    #region Tooltip
    [Tooltip("the current player scriptable object")]
    #endregion Tooltip
    public CurrentPlayerSO currentPlayer;

    public Material litDefaultMaterial;
    public Shader variableLitShader;

    #region Header UI
    [Space(10)]
    [Header("UI")]
    #endregion Header UI
    #region Tooltip
    [Tooltip("populate with ammo icon prefab")]
    #endregion Tooltip
    public GameObject ammoIconPrefab;
    public GameObject heartPrefab;

    //sound
    public AudioMixerGroup soundMasterMixerGroup;

    public SoundEffectSO doorOpenCloseSoundEffect;

    public TileBase[] enemyUnwalkableCollisionTilesArray;
    public TileBase preferedEnemyPathTile;

    #region Validation

#if UNITY_EDITOR

    // Validate SO fields
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(roomNodeTypeList), roomNodeTypeList);
        HelperUtilities.ValidateCheckNullValue(this, nameof(currentPlayer), currentPlayer);
        HelperUtilities.ValidateCheckNullValue(this, nameof(litDefaultMaterial), litDefaultMaterial);
        HelperUtilities.ValidateCheckNullValue(this, nameof(dimmedMaterial), dimmedMaterial);
        HelperUtilities.ValidateCheckNullValue(this, nameof(variableLitShader), variableLitShader);
        HelperUtilities.ValidateCheckNullValue(this, nameof(ammoIconPrefab), ammoIconPrefab);
        HelperUtilities.ValidateCheckNullValue(this, nameof(doorOpenCloseSoundEffect), doorOpenCloseSoundEffect);

        HelperUtilities.ValidateCheckEnumerableValues(this,nameof(enemyUnwalkableCollisionTilesArray), enemyUnwalkableCollisionTilesArray);
        HelperUtilities.ValidateCheckNullValue(this,nameof(preferedEnemyPathTile), preferedEnemyPathTile);
    }

#endif

    #endregion Validation
}
