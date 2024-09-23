using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class AimWeaponEvent : MonoBehaviour
{
    public event Action<AimWeaponEvent, AimWeaponEventArgs> OnAimWeapon;

    public void CallAimWeaponEvent(AimDirection aimDirection, float aimAngle, float weaponAimAngle, Vector3 weaponAimDirectionVector)
    {
        OnAimWeapon?.Invoke(this, new AimWeaponEventArgs() {
            aimDirection = aimDirection,
            aimAngle = aimAngle,
            weaponAimDirectionVector = weaponAimDirectionVector,
            weaponAimAngle = weaponAimAngle,
        });
    }
}

public class AimWeaponEventArgs : EventArgs
{
    public AimDirection aimDirection;
    public float aimAngle;
    public float weaponAimAngle;
    public Vector3 weaponAimDirectionVector;
}
