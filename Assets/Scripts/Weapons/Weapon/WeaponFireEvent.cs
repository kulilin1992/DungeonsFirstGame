using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[DisallowMultipleComponent]
public class WeaponFireEvent : MonoBehaviour
{
    public event Action<WeaponFireEvent, WeaponFireEventArgs> OnWeaponFire;

    public void CallWeaponFireEvent(Weapon weapon)
    {
        OnWeaponFire?.Invoke(this, new WeaponFireEventArgs() { weapon = weapon });
    }
}

public class WeaponFireEventArgs : EventArgs
{
    public Weapon weapon;
}
