
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(ActiveWeapon))]
[RequireComponent(typeof(FireWeaponEvent))]
[RequireComponent(typeof(WeaponFireEvent))]
[RequireComponent(typeof(ReloadWeaponEvent))]
[DisallowMultipleComponent]
public class FireWeapon : MonoBehaviour
{
    private float fireRateCoolDownTimer = 0f;
    private ActiveWeapon activeWeapon;
    private FireWeaponEvent fireWeaponEvent;
    private WeaponFireEvent weaponFireEvent;
    private ReloadWeaponEvent reloadWeaponEvent;

    private float firePreChargeTimer = 0f;

    private void Awake()
    {
        activeWeapon = GetComponent<ActiveWeapon>();
        fireWeaponEvent = GetComponent<FireWeaponEvent>();
        weaponFireEvent = GetComponent<WeaponFireEvent>();
        reloadWeaponEvent = GetComponent<ReloadWeaponEvent>();
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
        WeaponPreCharge(args);
        if (args.fire)
        {
            if (IsWeaponReadyToFire()) {
                //Debug.Log("Weapon Fired");
                FireAmmo(args.aimAngle, args.weaponAimAngle, args.weaponAimDirectionVector);
                ResetCoolDownTimer();

                ResetPrechargeTimer();
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
        if (firePreChargeTimer > 0f || fireRateCoolDownTimer > 0f)
            return false;
        // if (!activeWeapon.GetCurrentWeapon().weaponDetails.hasInfiniteClipCapacity &&
        // activeWeapon.GetCurrentWeapon().weaponClipRemainingAmmo <= 0)
        //     return false;
        if (!activeWeapon.GetCurrentWeapon().weaponDetails.hasInfiniteClipCapacity &&
             activeWeapon.GetCurrentWeapon().weaponClipRemainingAmmo <= 0)
            {
                reloadWeaponEvent.CallReloadWeaponEvent(activeWeapon.GetCurrentWeapon(), 0);
                return false;
            }
        return true;
    }

    private void FireAmmo(float aimAngle, float weaponAimAngle, Vector3 weaponAimDirectionVector)
    {
        AmmoDetailsSO currentAmmo = activeWeapon.GetCurrentAmmo();

        if (currentAmmo != null)
        {
            StartCoroutine(FireAmmoRoutine(currentAmmo, aimAngle, weaponAimAngle, weaponAimDirectionVector));
            // GameObject ammoPrefab = currentAmmo.ammoPrefabs[Random.Range(0, currentAmmo.ammoPrefabs.Length)];

            // float ammoSpeed = Random.Range(currentAmmo.ammoSpeedMin, currentAmmo.ammoSpeedMax);

            // IFireable ammo = (IFireable)PoolManager.Instance.ReuseComponent(ammoPrefab, activeWeapon.GetShootPosition(),
            //     Quaternion.identity);
            
            // ammo.InitialiseAmmo(currentAmmo, aimAngle, weaponAimAngle, ammoSpeed, weaponAimDirectionVector);

            // if (!activeWeapon.GetCurrentWeapon().weaponDetails.hasInfiniteClipCapacity) {
            //     activeWeapon.GetCurrentWeapon().weaponClipRemainingAmmo--;
            //     activeWeapon.GetCurrentWeapon().weaponRemainingAmmo--;
            // }

            // weaponFireEvent.CallWeaponFireEvent(activeWeapon.GetCurrentWeapon());
        }
    }

    private void ResetCoolDownTimer()
    {
        fireRateCoolDownTimer = activeWeapon.GetCurrentWeapon().weaponDetails.weaponFireRate;
    }
    
    private void WeaponPreCharge(FireWeaponEventArgs args)
    {
        if (args.firePreviousFrame) {
            firePreChargeTimer -= Time.deltaTime;
        }
        else
        {
            ResetPrechargeTimer();
        }
    }

    private void ResetPrechargeTimer()
    {
        firePreChargeTimer = activeWeapon.GetCurrentWeapon().weaponDetails.weaponPrechargeTime;
    }

    private IEnumerator FireAmmoRoutine(AmmoDetailsSO currentAmmo, float aimAngle,
        float weaponAimAngle, Vector3 weaponAimDirectionVector)
    {
        int ammoCounter = 0;

        int ammoPerShot = Random.Range(currentAmmo.ammoSpawnAmountMin, currentAmmo.ammoSpawnAmountMax + 1);
        
        float ammoSpawnInterval;

        if (ammoPerShot > 1) {
            ammoSpawnInterval = Random.Range(currentAmmo.ammoSpawnIntervalMin, currentAmmo.ammoSpawnIntervalMax);
        }
        else
        {
            ammoSpawnInterval = 0f;
        }

        while (ammoCounter < ammoPerShot)
        {
            ammoCounter++;
            GameObject ammoPrefab = currentAmmo.ammoPrefabs[Random.Range(0, currentAmmo.ammoPrefabs.Length)];

            float ammoSpeed = Random.Range(currentAmmo.ammoSpeedMin, currentAmmo.ammoSpeedMax);

            IFireable ammo = (IFireable)PoolManager.Instance.ReuseComponent(ammoPrefab, activeWeapon.GetShootPosition(),
                Quaternion.identity);
            
            ammo.InitialiseAmmo(currentAmmo, aimAngle, weaponAimAngle, ammoSpeed, weaponAimDirectionVector);
            yield return new WaitForSeconds(ammoSpawnInterval);
        }
            if (!activeWeapon.GetCurrentWeapon().weaponDetails.hasInfiniteClipCapacity) {
                activeWeapon.GetCurrentWeapon().weaponClipRemainingAmmo--;
                activeWeapon.GetCurrentWeapon().weaponRemainingAmmo--;
            }

            weaponFireEvent.CallWeaponFireEvent(activeWeapon.GetCurrentWeapon());

            WeaponShootEffect(aimAngle);

            WeaponSoundEffect();
    }

    private void WeaponShootEffect(float aimAngle)
    {
        if (activeWeapon.GetCurrentWeapon().weaponDetails.weaponShootEffect != null &&
            activeWeapon.GetCurrentWeapon().weaponDetails.weaponShootEffect.weaponShootEffectPrefab != null) {
            WeaponShootEffect weaponShootEffect = (WeaponShootEffect) PoolManager.Instance.ReuseComponent
                (activeWeapon.GetCurrentWeapon().weaponDetails.weaponShootEffect.weaponShootEffectPrefab,
                activeWeapon.GetShootEffectPosition(), Quaternion.identity);

            weaponShootEffect.SetShootEffect(activeWeapon.GetCurrentWeapon().weaponDetails.weaponShootEffect, aimAngle);    
            weaponShootEffect.gameObject.SetActive(true);
        }

    }

    private void WeaponSoundEffect()
    {
        if (activeWeapon.GetCurrentWeapon().weaponDetails.weaponFiringSoundEffect != null) {
            SoundEffectManager.Instance.PlaySoundEffect(activeWeapon.GetCurrentWeapon().weaponDetails.weaponFiringSoundEffect);
        }
    }
}
