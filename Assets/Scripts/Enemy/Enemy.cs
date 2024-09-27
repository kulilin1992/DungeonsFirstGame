using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SortingGroup))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(PolygonCollider2D))]
[RequireComponent(typeof(Animator))]

[RequireComponent(typeof(EnemyMovementAI))]
[RequireComponent(typeof(MovementToPosition))]
[RequireComponent(typeof(MovementToPositionEvent))]
[RequireComponent(typeof(Idle))]
[RequireComponent(typeof(IdleEvent))]

[RequireComponent(typeof(EnemyAnimate))]

[RequireComponent(typeof(MaterializeEffect))]

[DisallowMultipleComponent]
public class Enemy : MonoBehaviour
{
    [HideInInspector] public EnemyDetailsSO enemyDetails;
    [HideInInspector] public SpriteRenderer[] spriteRendererArray;
    [HideInInspector] public Animator animator;
    [HideInInspector] public MovementToPositionEvent movementToPositionEvent;
    [HideInInspector] public IdleEvent idleEvent;
    private CircleCollider2D circleCollider;
    private PolygonCollider2D polygonCollider;

    private EnemyMovementAI enemyMovementAI;
    private MaterializeEffect materializeEffect;
    //public EnemyDetailsSO enemyDetails;

    //weapon
    [HideInInspector] public AimWeaponEvent aimWeaponEvent;
    [HideInInspector] public FireWeaponEvent fireWeaponEvent;
    

    private FireWeapon fireWeapon;
    private SetActiveWeaponEvent setActiveWeaponEvent;

    //health
    private Health health;
    private HealthEvent healthEvent;


    private void Awake()
    {
        circleCollider = GetComponent<CircleCollider2D>();
        polygonCollider = GetComponent<PolygonCollider2D> ();
        spriteRendererArray = GetComponentsInChildren<SpriteRenderer>();
        animator = GetComponent<Animator>();

        enemyMovementAI = GetComponent<EnemyMovementAI>();
        movementToPositionEvent = GetComponent<MovementToPositionEvent>();
        idleEvent = GetComponent<IdleEvent>();
        materializeEffect = GetComponent<MaterializeEffect>();

        //weapon
        aimWeaponEvent = GetComponent<AimWeaponEvent>();
        fireWeaponEvent = GetComponent<FireWeaponEvent>();
        fireWeapon = GetComponent<FireWeapon>();
        setActiveWeaponEvent = GetComponent<SetActiveWeaponEvent>();

        //health
        health = GetComponent<Health>();
        healthEvent = GetComponent<HealthEvent>();

    }

    private void OnEnable()
    {
        healthEvent.OnHealthEvent += OnHealthLost;
    }

    private void OnDisable()
    {
        healthEvent.OnHealthEvent -= OnHealthLost;
    }

    private void OnHealthLost(HealthEvent healthEvent, HealthEventArgs args)
    {
        if (args.healthAmount <= 0) {
            EnemyDestroyed();
            //EnemyDisable();
        }
    }

    private void EnemyDestroyed()
    {
        DestoryedEvent destoryedEvent = GetComponent<DestoryedEvent>();
        destoryedEvent.CallDestoryedEvent(false);
    }

    public void EnemyInit(EnemyDetailsSO enemyDetails, int enemySpawnNumber, DungeonLevelSO dungeonLevel)
    {
        this.enemyDetails = enemyDetails;

        //optimize
        SetEnemyMovementUpdateFrame(enemySpawnNumber);

        SetEnemyStartingHealth(dungeonLevel);

        SetEnemyStartWeapon();

        SetEnemyAnimationSpeed();

        StartCoroutine(MaterializeEnemy());
    }

    private void SetEnemyMovementUpdateFrame(int enemySpawnNumber)
    {
        enemyMovementAI.SetUpdateFrameNumber(enemySpawnNumber % Settings.targetFrameRateToSpreadPathFindingOver);
    }


    private void SetEnemyStartingHealth(DungeonLevelSO dungeonLevel) {
        foreach (EnemyHealthDetails enemyHealthDetails in enemyDetails.enemyHealthDetailsArray) {
            if (enemyHealthDetails.dungeonLevel == dungeonLevel) {
                health.SetStartingHealth(enemyHealthDetails.enemyHealthAmount);
                return;
            }
        }
        health.SetStartingHealth(Settings.defaultEnemyHealth);
    }
    private void SetEnemyStartWeapon()
    {
        if (enemyDetails.enemyWeapon != null) {
            Weapon weapon = new Weapon() {
                weaponDetails = enemyDetails.enemyWeapon,
                weaponReloadTimer = 0f,
                weaponClipRemainingAmmo = enemyDetails.enemyWeapon.weaponClipAmmoCapacity,
                weaponRemainingAmmo = enemyDetails.enemyWeapon.weaponAmmoCapacity,
                isWeaponReloading = false
            };

            setActiveWeaponEvent.CallSetActiveWeaponEvent(weapon);
        }
    }

    private void SetEnemyAnimationSpeed()
    {
        animator.speed = enemyMovementAI.moveSpeed / Settings.baseSpeedForEnemyAnimations;
    }

    private IEnumerator MaterializeEnemy()
    {
        EnemyEnable(false);

        yield return StartCoroutine(materializeEffect.MaterializeRoutine(enemyDetails.enemyMaterializeShader, enemyDetails.enemyMaterializeColor,
            enemyDetails.enemyMaterializeTime, spriteRendererArray, enemyDetails.enemyStandardMaterial));
    
        EnemyEnable(true);
    }

    private void EnemyEnable(bool enable)
    {
        circleCollider.enabled = enable;
        polygonCollider.enabled = enable;

        enemyMovementAI.enabled = enable;
        fireWeapon.enabled = enable;
    }
}
