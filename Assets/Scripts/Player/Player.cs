using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


#region REQUIRE COMPONENTS
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(SortingGroup))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(PolygonCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]

[RequireComponent(typeof(IdleEvent))]
[RequireComponent(typeof(Idle))]
[RequireComponent(typeof(AimWeapon))]
[RequireComponent(typeof(AimWeaponEvent))]

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(AnimatePlayer))]

[RequireComponent(typeof(MovementByVelocity))]
[RequireComponent(typeof(MovementByVelocityEvent))]

[RequireComponent(typeof(MovementToPosition))]
[RequireComponent(typeof(MovementToPositionEvent))]

[RequireComponent(typeof(SetActiveWeaponEvent))]

[RequireComponent(typeof(ActiveWeapon))]

[RequireComponent(typeof(FireWeaponEvent))]
[RequireComponent(typeof(WeaponFireEvent))]
[RequireComponent(typeof(FireWeapon))]

[DisallowMultipleComponent]
#endregion
public class Player : MonoBehaviour
{
   [HideInInspector] public PlayerDetailsSO playerDetails;
   [HideInInspector] public Health health;
   [HideInInspector] public SpriteRenderer spriteRenderer;
   [HideInInspector] public Animator animator;

   //Events
   [HideInInspector] public IdleEvent idleEvent;
   [HideInInspector] public AimWeaponEvent aimWeaponEvent;

   //movement
   [HideInInspector] public MovementByVelocity movementByVelocity;
   [HideInInspector] public MovementByVelocityEvent movementByVelocityEvent;

   [HideInInspector] public MovementToPositionEvent movementToPositionEvent;

   [HideInInspector] public SetActiveWeaponEvent setActiveWeaponEvent;

   [HideInInspector] public ActiveWeapon activeWeapon;
   [HideInInspector] public WeaponFireEvent weaponFireEvent;
   [HideInInspector] public FireWeaponEvent fireWeaponEvent;
   
   public List<Weapon> weaponList = new List<Weapon>();

   private void Awake()
   {
        health = GetComponent<Health>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        idleEvent = GetComponent<IdleEvent>();
        aimWeaponEvent = GetComponent<AimWeaponEvent>();

        movementByVelocityEvent = GetComponent<MovementByVelocityEvent>();

        movementToPositionEvent = GetComponent<MovementToPositionEvent>();
        
        setActiveWeaponEvent = GetComponent<SetActiveWeaponEvent>();
        activeWeapon = GetComponent<ActiveWeapon>();

        weaponFireEvent = GetComponent<WeaponFireEvent>();
        fireWeaponEvent = GetComponent<FireWeaponEvent>();
   }

   public void Initialize(PlayerDetailsSO playerDetails)
   {
        this.playerDetails = playerDetails;
        SetPlayerHealth();

        CreatePlayerStartingWeapons();
   }
   public void SetPlayerHealth()
   {
        health.SetStartingHealth(playerDetails.playerHealthAmount);
   }
   private void CreatePlayerStartingWeapons()
   {
        weaponList.Clear();
        foreach (WeaponDetailsSO weaponDetails in playerDetails.startingWeaponList)
        {
            AddWeaponToPlayer(weaponDetails);
        }
   }

    private Weapon AddWeaponToPlayer(WeaponDetailsSO weaponDetails)
    {
        Weapon weapon = new Weapon()
        {
            weaponDetails = weaponDetails,
            weaponReloadTimer = 0f,
            weaponClipRemainingAmmo = weaponDetails.weaponClipAmmoCapacity,
            weaponRemainingAmmo = weaponDetails.weaponAmmoCapacity,
            isWeaponReloading = false
        };

        weaponList.Add(weapon);

        weapon.weaponListPosition = weaponList.Count;

        setActiveWeaponEvent.CallSetActiveWeaponEvent(weapon);

        return weapon;
    }
}
