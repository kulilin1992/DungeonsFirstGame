using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[DisallowMultipleComponent]
public class WeaponReloadEvent : MonoBehaviour
{
    public event Action<WeaponReloadEvent, WeaponReloadEventArgs> OnWeaponReloaded;

    public void CallWeaponReloadEvent(Weapon weapon)
    {
        OnWeaponReloaded?.Invoke(this, new WeaponReloadEventArgs { weapon = weapon });
    }
}

public class WeaponReloadEventArgs : EventArgs
{
    public Weapon weapon;
}
