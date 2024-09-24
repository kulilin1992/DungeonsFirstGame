using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Player))]
[DisallowMultipleComponent]
public class PlayerController : MonoBehaviour
{
    //[SerializeField] private Transform weaponShootPosition;

    [SerializeField] private MovementDetailsSO movementDetails;

    private Player player;
    private float moveSpeed;

    private Coroutine playerRollCoroutine;
    private WaitForFixedUpdate waitForFixedUpdate;
    private bool isPlayerRolling = false;
    private float playerRollCooldownTimer = 0f;

    private int currentWeaponIndex = 1;

    #region Validation
#if UNITY_EDITOR
    public void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(movementDetails), movementDetails);
    }
#endif
    #endregion

    private void Awake()
    {
        player = GetComponent<Player>();
        moveSpeed = movementDetails.GetMoveSpeed();
    }

    private void Start()
    {
        waitForFixedUpdate = new WaitForFixedUpdate();
        SetPlayerAnimationSpeed();
        SetStartingWeapon();
    }
    private void Update()
    {
        if (isPlayerRolling) return;
        MovementInput();
        WeaponInput();

        PlayerRollCooldownTimer();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        StopPlayerRollRoutine();
    }

    private void OnCollistionStay2D(Collision2D collision)
    {
        StopPlayerRollRoutine();
    }

    private void MovementInput() {

        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");
        bool rightMouseButtonDown = Input.GetMouseButtonDown(1);

        Vector2 direction = new Vector2(horizontalInput, verticalInput);
        if (horizontalInput != 0 && verticalInput != 0) {
            direction *= 0.7f; //diagonal movement
        }

        if (direction != Vector2.zero) {

            if (!rightMouseButtonDown) {
                player.movementByVelocityEvent.CallMovementByVelocityEvent(direction, moveSpeed);
            }
            else if (playerRollCooldownTimer <= 0f) {
                PlayerRoll((Vector3)direction);

            }
        }
        else {
            player.idleEvent.CallIdleEvent();
        }

        //player.idleEvent.CallIdleEvent();
    }

    private void WeaponInput() {
        Vector3 weaponDirection;
        float weaponAngleDegress, playerAngleDegrees;
        AimDirection playerAimDirection;

        AimWeaponInput(out weaponDirection, out weaponAngleDegress, out playerAngleDegrees, out playerAimDirection);
        FireWeaponInput(weaponDirection, weaponAngleDegress, playerAngleDegrees, playerAimDirection);
    }

    private void AimWeaponInput(out Vector3 weaponDirection, out float weaponAngleDegress, out float playerAngleDegrees, out AimDirection playerAimDirection)
    {
        Vector3 mouseWorldPosition = HelperUtilities.GetMouseWorldPosition();

        //weaponDirection = mouseWorldPosition - weaponShootPosition.position;
        weaponDirection = mouseWorldPosition - player.activeWeapon.GetShootPosition();

        Vector3 playerDirection = mouseWorldPosition - transform.position;

        weaponAngleDegress = HelperUtilities.GetAngleFromVector(weaponDirection);

        playerAngleDegrees = HelperUtilities.GetAngleFromVector(playerDirection);

        playerAimDirection = HelperUtilities.GetAimDirection(playerAngleDegrees);

        player.aimWeaponEvent.CallAimWeaponEvent(playerAimDirection, playerAngleDegrees, weaponAngleDegress, weaponDirection);
    }

    private void FireWeaponInput(Vector3 weaponDirection, float weaponAngleDegress, float playerAngleDegrees, AimDirection playerAimDirection)
    {
        if (Input.GetMouseButton(0)) {
            Debug.Log("Fire Weapon");
            player.fireWeaponEvent.CallFireWeaponEvent(true, playerAimDirection, playerAngleDegrees, weaponAngleDegress, weaponDirection);
        }
    }

    private void PlayerRoll(Vector3 direction)
    {
        playerRollCoroutine = StartCoroutine(PlayerRollCoroutine(direction));
    }

    private IEnumerator PlayerRollCoroutine(Vector3 direction)
    {
        float minDistance = 0.2f;
        isPlayerRolling = true;

        Vector3 targetPosition = player.transform.position + (Vector3)direction * movementDetails.rollDistance;
        while (Vector3.Distance(player.transform.position, targetPosition) > minDistance) {
            player.movementToPositionEvent.CallMovementToPositionEvent(targetPosition, player.transform.position, movementDetails.rollSpeed,
                direction, isPlayerRolling);
            yield return waitForFixedUpdate;
        }
        isPlayerRolling = false;
        playerRollCooldownTimer = movementDetails.rollCooldownTime;
        player.transform.position = targetPosition;
    }

    private void PlayerRollCooldownTimer() {
        if (playerRollCooldownTimer >= 0f) {
            playerRollCooldownTimer -= Time.deltaTime;
        }
    }
    private void StopPlayerRollRoutine() {
        if (playerRollCoroutine != null) {
            StopCoroutine(playerRollCoroutine);
            isPlayerRolling = false;
        }
    }

    private void SetPlayerAnimationSpeed()
    {
        player.animator.speed = moveSpeed / Settings.baseSpeedForPlayerAnimations;
    }

    private void SetStartingWeapon()
    {
        int index = 1;
        foreach (Weapon weapon in player.weaponList) {
            if (weapon.weaponDetails == player.playerDetails.startingWeapon) {
                SetWeaponByIndex(index);
                break;
            }
            index++;
        }
    }

    private void SetWeaponByIndex(int index)
    {
        if (index - 1 < player.weaponList.Count) {
            currentWeaponIndex = index - 1;
            player.setActiveWeaponEvent.CallSetActiveWeaponEvent(player.weaponList[index - 1]);
        }
    }
}
