using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerDetails", menuName = "Scriptable Objects/Player/Player Details")]
public class PlayerDetailsSO : ScriptableObject
{
    public string playerCharacterName;

    public GameObject playerPrefab;
    public RuntimeAnimatorController runtimeAnimatorController;

    public int playerHealthAmount;

    public bool isImmuneAfterHit = false;
    public float hitImmunityTime;

    public Sprite playerMiniMapIcon;
    public Sprite playerHandSprite;

    //weapon
    public WeaponDetailsSO startingWeapon;
    public List<WeaponDetailsSO> startingWeaponList;

    #region Validation
#if UNITY_EDITOR
    public void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(playerCharacterName), playerCharacterName);
        HelperUtilities.ValidateCheckNullValue(this, nameof(playerPrefab), playerPrefab);
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(playerHealthAmount), playerHealthAmount,false);
        HelperUtilities.ValidateCheckNullValue(this, nameof(playerMiniMapIcon), playerMiniMapIcon);
        HelperUtilities.ValidateCheckNullValue(this, nameof(playerHandSprite), playerHandSprite);
        HelperUtilities.ValidateCheckNullValue(this, nameof(runtimeAnimatorController), runtimeAnimatorController);

        HelperUtilities.ValidateCheckNullValue(this, nameof(startingWeapon), startingWeapon);
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(startingWeaponList), startingWeaponList);
    }
#endif
    #endregion
}
