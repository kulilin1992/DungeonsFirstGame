
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(ActiveWeapon))]
[RequireComponent(typeof(FireWeaponEvent))]
[RequireComponent(typeof(WeaponFireEvent))]
[DisallowMultipleComponent]
public class FireWeapon : MonoBehaviour
{
    private float fireRateCoolDownTimer = 0f;
    private ActiveWeapon activeWeapon;
    private FireWeaponEvent fireWeaponEvent;
    private WeaponFireEvent weaponFireEvent;

    private void Awake()
    {
        activeWeapon = GetComponent<ActiveWeapon>();
        fireWeaponEvent = GetComponent<FireWeaponEvent>();
        weaponFireEvent = GetComponent<WeaponFireEvent>();
    }
    private void OnEnable()
    {
        fireWeaponEvent.OnFireWeapon += FireWeaponEvent_OnFireWeapon;
    }

    private void OnDisable()
    {
        fireWeaponEvent.OnFireWeapon -= FireWeaponEvent_OnFireWeapon;
    }

    private void Update()
    {
        fireRateCoolDownTimer -= Time.deltaTime;
    }

    private void FireWeaponEvent_OnFireWeapon(FireWeaponEvent fireWeaponEvent, FireWeaponEventArgs args)
    {
        WeaponFire(args);
    }

    private void WeaponFire(FireWeaponEventArgs args)
    {
        if (args.fire)
        {
            if (IsWeaponReadyToFire()) {
                Debug.Log("Weapon Fired");
                FireAmmo(args.aimAngle, args.weaponAimAngle, args.weaponAimDirectionVector);
                ResetCoolDownTimer();
            }
        }
    }

    private bool IsWeaponReadyToFire()
    {
        if (activeWeapon.GetCurrentWeapon().weaponRemainingAmmo <= 0 && 
            !activeWeapon.GetCurrentWeapon().weaponDetails.hasInfiniteAmmo)
            return false;
        if (activeWeapon.GetCurrentWeapon().isWeaponReloading)
            return false;
        if (fireRateCoolDownTimer > 0)
            return false;
        if (!activeWeapon.GetCurrentWeapon().weaponDetails.hasInfiniteClipCapacity &&
        activeWeapon.GetCurrentWeapon().weaponClipRemainingAmmo <= 0)
            return false;
        return true;
    }

    private void FireAmmo(float aimAngle, float weaponAimAngle, Vector3 weaponAimDirectionVector)
    {
        AmmoDetailsSO currentAmmo = activeWeapon.GetCurrentAmmo();

        if (currentAmmo != null)
        {
            Debug.Log("asss");
            GameObject ammoPrefab = currentAmmo.ammoPrefabs[Random.Range(0, currentAmmo.ammoPrefabs.Length)];

            float ammoSpeed = Random.Range(currentAmmo.ammoSpeedMin, currentAmmo.ammoSpeedMax);

            IFireable ammo = (IFireable)PoolManager.Instance.ReuseComponent(ammoPrefab, activeWeapon.GetShootPosition(),
                Quaternion.identity);
            
            ammo.InitialiseAmmo(currentAmmo, aimAngle, weaponAimAngle, ammoSpeed, weaponAimDirectionVector);

            if (!activeWeapon.GetCurrentWeapon().weaponDetails.hasInfiniteClipCapacity) {
                activeWeapon.GetCurrentWeapon().weaponClipRemainingAmmo--;
                activeWeapon.GetCurrentWeapon().weaponRemainingAmmo--;
            }

            weaponFireEvent.CallWeaponFireEvent(activeWeapon.GetCurrentWeapon());
        }
    }

    private void ResetCoolDownTimer()
    {
        fireRateCoolDownTimer = activeWeapon.GetCurrentWeapon().weaponDetails.weaponFireRate;
    }
}
