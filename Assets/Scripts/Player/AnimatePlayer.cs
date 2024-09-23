using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Player))]
[DisallowMultipleComponent]
public class AnimatePlayer : MonoBehaviour
{

    private Player player;

    private void Awake () {
        player = GetComponent<Player>();
    }

    private void OnEnable() {
        player.idleEvent.OnIdle += IdleEvent_OnIdle;
        player.aimWeaponEvent.OnAimWeapon += AimWeaponEvent_OnAimWeapon;

        player.movementByVelocityEvent.OnMovementByVelocity += MovementEvent_OnMove;
    }

    private void OnDisable() {
        player.idleEvent.OnIdle -= IdleEvent_OnIdle;
        player.aimWeaponEvent.OnAimWeapon -= AimWeaponEvent_OnAimWeapon;
        player.movementByVelocityEvent.OnMovementByVelocity -= MovementEvent_OnMove;
    }

    private void IdleEvent_OnIdle(IdleEvent idleEvent)
    {
        SetIdleAnimationParameters();
    }

    private void AimWeaponEvent_OnAimWeapon(AimWeaponEvent aimWeaponEvent, AimWeaponEventArgs args)
    {
        InitializeAimAnimationParameters();
        SetAimWeaponAnimationParameters(args.aimDirection);
    }

    private void MovementEvent_OnMove(MovementByVelocityEvent movementEvent, MovementByVelocityArgs args)
    {
        SetMovementAnimationParameters();
    }

    private void SetIdleAnimationParameters()
    {
        player.animator.SetBool(Settings.isMoving, false);
        player.animator.SetBool(Settings.isIdle, true);
    }

    private void SetMovementAnimationParameters()
    {
        player.animator.SetBool(Settings.isMoving, true);
        player.animator.SetBool(Settings.isIdle, false);
    }

    private void InitializeAimAnimationParameters()
    {
        player.animator.SetBool(Settings.aimUp, false);
        player.animator.SetBool(Settings.aimUpLeft, false);
        player.animator.SetBool(Settings.aimUpRight, false);
        player.animator.SetBool(Settings.aimDown, false);
        player.animator.SetBool(Settings.aimLeft, false);
        player.animator.SetBool(Settings.aimRight, false);
    }
    
    private void SetAimWeaponAnimationParameters(AimDirection aimDirection)
    {
        switch (aimDirection) {
            case AimDirection.Up:
                player.animator.SetBool(Settings.aimUp, true);
                break;
            case AimDirection.UpLeft:
                player.animator.SetBool(Settings.aimUpLeft, true);
                break;
            case AimDirection.UpRight:
                player.animator.SetBool(Settings.aimUpRight, true);
                break;
            case AimDirection.Down:
                player.animator.SetBool(Settings.aimDown, true);
                break;
            case AimDirection.Left:
                player.animator.SetBool(Settings.aimLeft, true);
                break;
            case AimDirection.Right:
                player.animator.SetBool(Settings.aimRight, true);
                break;
        }
    }
}
