using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyDetails_", menuName = "Scriptable Objects/Enemy/EnemyDetails")]
public class EnemyDetailsSO : ScriptableObject
{
    public string enemyName;
    public GameObject enemyPrefab;

    public float chaseDistance = 50f;

    public Material enemyStandardMaterial;
    public float enemyMaterializeTime;
    public Shader enemyMaterializeShader;

    [ColorUsage(true, true)]
    public Color enemyMaterializeColor;


    //enemy weapon details
    public WeaponDetailsSO enemyWeapon;
    public float firingIntervalMin = 0.1f;
    public float firingIntervalMax = 1f;
    public float firingDurationMin = 1f;
    public float firingDurationMax = 2f;
    public bool firingLineOfSightRequired; //if true, enemy will only fire if player is in line of sight

    //enemy health
    public EnemyHealthDetails[] enemyHealthDetailsArray;
    public bool isImmuneAfterHit = false;
    public float hitImmunityTime;

    #region Validation

#if UNITY_EDITOR

    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(enemyName), enemyName);
        HelperUtilities.ValidateCheckNullValue(this, nameof(enemyPrefab), enemyPrefab);
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(chaseDistance), chaseDistance, false);
    }

#endif

    #endregion Validation
}
