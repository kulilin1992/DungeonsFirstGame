using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(AimWeaponEvent))]
[DisallowMultipleComponent]
public class AimWeapon : MonoBehaviour
{
    [SerializeField] private Transform weaponRotationPointTransform;

    private AimWeaponEvent aimWeaponEvent;

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(weaponRotationPointTransform), weaponRotationPointTransform);
    }
#endif
    #endregion

    private void Awake()
    {
        aimWeaponEvent = GetComponent<AimWeaponEvent>();
    }

    private void OnEnable()
    {
        aimWeaponEvent.OnAimWeapon += AimWeaponEvent_OnAimWeapon;
    }

    private void OnDisable()
    {
        aimWeaponEvent.OnAimWeapon -= AimWeaponEvent_OnAimWeapon;
    }

    private void AimWeaponEvent_OnAimWeapon(AimWeaponEvent aimWeaponEvent, AimWeaponEventArgs args)
    {
        Aim(args.aimDirection, args.aimAngle);
    }

    private void Aim(AimDirection aimDirection, float aimAngle)
    {
        //Set angle of the weapon transform
        weaponRotationPointTransform.eulerAngles = new Vector3(0f, 0f, aimAngle);

        switch (aimDirection)
        {
            case AimDirection.Left:
            case AimDirection.UpLeft:
                weaponRotationPointTransform.localScale = new Vector3(1f, -1f, 0f);
                break;
            case AimDirection.Right:
            case AimDirection.UpRight:
            case AimDirection.Up:
            case AimDirection.Down:
                weaponRotationPointTransform.localScale = new Vector3(1f, 1f, 0f);
                break;
        }
    }
}
