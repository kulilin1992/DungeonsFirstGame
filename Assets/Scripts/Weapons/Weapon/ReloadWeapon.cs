using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(WeaponReloadEvent))]
[RequireComponent(typeof(ReloadWeaponEvent))]
[RequireComponent(typeof(SetActiveWeaponEvent))]

[DisallowMultipleComponent]
public class ReloadWeapon : MonoBehaviour
{
    private WeaponReloadEvent weaponReloadEvent;
    private ReloadWeaponEvent reloadWeaponEvent;
    private SetActiveWeaponEvent setActiveWeaponEvent;
    private Coroutine reloadWeaponCoroutine;

    private void Awake()
    {
        weaponReloadEvent = GetComponent<WeaponReloadEvent>();
        reloadWeaponEvent = GetComponent<ReloadWeaponEvent>();
        setActiveWeaponEvent = GetComponent<SetActiveWeaponEvent>();
    }

    private void OnEnable()
    {
        
        reloadWeaponEvent.OnReloadWeapon += OnReloadWeapon;
        setActiveWeaponEvent.OnSetActiveWeapon += OnSetActiveWeapon;
    }

    private void OnDisable()
    {
        reloadWeaponEvent.OnReloadWeapon -= OnReloadWeapon;
        setActiveWeaponEvent.OnSetActiveWeapon -= OnSetActiveWeapon;
    }

    private void OnReloadWeapon(ReloadWeaponEvent reloadWeaponEvent, ReloadWeaponEventArgs args)
    {
        StartReloadWeapon(args);
    }

    private void StartReloadWeapon(ReloadWeaponEventArgs args)
    {
        if (reloadWeaponCoroutine != null)
        {
            StopCoroutine(reloadWeaponCoroutine);
        }
        reloadWeaponCoroutine = StartCoroutine(ReloadWeaponRoutine(args.weapon, args.topUpAmmoPercent));
    }
    private IEnumerator ReloadWeaponRoutine(Weapon weapon, float topUpAmmoPercent)
    {

        if (weapon.weaponDetails.weaponFiringSoundEffect != null) {
            SoundEffectManager.Instance.PlaySoundEffect(weapon.weaponDetails.weaponReloadingSoundEffect);
        }

        weapon.isWeaponReloading = true;

        while (weapon.weaponReloadTimer < weapon.weaponDetails.weaponReloadTime) {
            weapon.weaponReloadTimer += Time.deltaTime;
            yield return null;
        }

        if (topUpAmmoPercent != 0) {
            int ammoIncrease = Mathf.RoundToInt((weapon.weaponDetails.weaponAmmoCapacity * topUpAmmoPercent) / 100f);
            int totalAmmo = weapon.weaponRemainingAmmo + ammoIncrease;

            if (totalAmmo > weapon.weaponDetails.weaponClipAmmoCapacity) {
                weapon.weaponRemainingAmmo = weapon.weaponDetails.weaponClipAmmoCapacity;
            }
            else {
                weapon.weaponRemainingAmmo = totalAmmo;
            }
        }
        if (weapon.weaponDetails.hasInfiniteAmmo) {
            weapon.weaponClipRemainingAmmo = weapon.weaponDetails.weaponClipAmmoCapacity;
        }
        else if (weapon.weaponRemainingAmmo > weapon.weaponDetails.weaponClipAmmoCapacity) {
            //int remainSum = weapon.weaponDetails.weaponClipAmmoCapacity - (int)weapon.weaponClipRemainingAmmo;
            weapon.weaponClipRemainingAmmo = weapon.weaponDetails.weaponClipAmmoCapacity;
            //weapon.weaponRemainingAmmo = weapon.weaponRemainingAmmo - remainSum;
        }
        else {
            weapon.weaponClipRemainingAmmo = weapon.weaponRemainingAmmo;
        }

        weapon.weaponReloadTimer = 0;
        weapon.isWeaponReloading = false;
        weaponReloadEvent.CallWeaponReloadEvent(weapon);
    }

    private void OnSetActiveWeapon(SetActiveWeaponEvent setActiveWeaponEvent, SetActiveWeaponEventArgs args)
    {
        if (args.weapon.isWeaponReloading) {
            if (reloadWeaponCoroutine != null) {
                StopCoroutine(reloadWeaponCoroutine);
            }
            reloadWeaponCoroutine = StartCoroutine(ReloadWeaponRoutine(args.weapon, 0));
        }
    }
}
