using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AmmoDetails_", menuName = "Scriptable Objects/Weapons/AmmoDetails")]
public class AmmoDetailsSO : ScriptableObject
{
    public string ammoName;
    public bool isPlayerAmmo;
    public Sprite ammoIcon;

    public GameObject[] ammoPrefabs;
    public Material ammoMaterial;

    public float ammoChargeTime = 0.1f;

    public Material ammoChargeMaterial;
    public int ammoDamage = 1;

    public float ammoSpeedMin = 20f;
    public float ammoSpeedMax = 20f;

    public float ammoRange = 20f;
    public float ammoRotationSpeed = 1f;

    public float ammoSpreadMin = 0f;
    public float ammoSpreadMax = 0f;

    public int ammoSpawnAmountMin = 1;
    public int ammoSpawnAmountMax = 1;

    public float ammoSpawnIntervalMin = 0f;
    public float ammoSpawnIntervalMax = 0f;

    public bool isAmmoTrail = false;
    public float ammoTrailTime = 3f;
    public Material ammoTrailMaterial;

    [Range(0f, 1f)] public float ammoTrailStartWidth;
    [Range(0f, 1f)] public float ammoTrailEndWidth;

    public AmmoHitEffectSO ammoHitEffect;

        #region Validation

#if UNITY_EDITOR

    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(ammoName), ammoName);
        HelperUtilities.ValidateCheckNullValue(this, nameof(ammoIcon), ammoIcon);
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(ammoPrefabs), ammoPrefabs);
        HelperUtilities.ValidateCheckNullValue(this, nameof(ammoMaterial), ammoMaterial);

        if (ammoChargeTime > 0)
        {
            HelperUtilities.ValidateCheckNullValue(this, nameof(ammoChargeMaterial), ammoChargeMaterial);
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(ammoDamage), ammoDamage, false);
            HelperUtilities.ValidateCheckPositiveRange(this, nameof(ammoSpeedMin), ammoSpeedMin, nameof(ammoSpeedMax), ammoSpeedMax, false);
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(ammoRange), ammoRange, false);
            HelperUtilities.ValidateCheckPositiveRange(this, nameof(ammoSpreadMin), ammoSpreadMin, nameof(ammoSpreadMax), ammoSpreadMax, true);
            HelperUtilities.ValidateCheckPositiveRange(this, nameof(ammoSpawnAmountMin), ammoSpawnAmountMin, nameof(ammoSpawnAmountMax), ammoSpawnAmountMax,false);
            HelperUtilities.ValidateCheckPositiveRange(this, nameof(ammoSpawnIntervalMin), ammoSpawnIntervalMin, nameof(ammoSpawnIntervalMax), ammoSpawnIntervalMax, true);
        }
        if (isAmmoTrail)
        {
            HelperUtilities.ValidateCheckNullValue(this, nameof(ammoTrailMaterial), ammoTrailMaterial);
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(ammoTrailTime), ammoTrailTime, false);
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(ammoTrailStartWidth), ammoTrailStartWidth, false);
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(ammoTrailEndWidth), ammoTrailEndWidth, false);
        }
    }

#endif

    #endregion Validation
}
