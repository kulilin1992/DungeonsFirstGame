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

   private void Awake()
   {
        health = GetComponent<Health>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        idleEvent = GetComponent<IdleEvent>();
        aimWeaponEvent = GetComponent<AimWeaponEvent>();

        movementByVelocityEvent = GetComponent<MovementByVelocityEvent>();
   }

   public void Initialize(PlayerDetailsSO playerDetails)
   {
        this.playerDetails = playerDetails;
        SetPlayerHealth();
   }
   public void SetPlayerHealth()
   {
        health.SetStartingHealth(playerDetails.playerHealthAmount);
   }
}
