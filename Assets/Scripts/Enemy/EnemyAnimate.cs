using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Enemy))]
[DisallowMultipleComponent]
public class EnemyAnimate : MonoBehaviour
{
    private Enemy enemy;
    
    private void Awake()
    {
        enemy = GetComponent<Enemy>();
    }

    private void OnEnable()
    {
        enemy.movementToPositionEvent.OnMovementToPosition += OnMovementToPosition;
        enemy.idleEvent.OnIdle += OnIdle;

        enemy.aimWeaponEvent.OnAimWeapon += OnAimWeapon;
    }

    private void OnDisable()
    {
        enemy.movementToPositionEvent.OnMovementToPosition -= OnMovementToPosition;
        enemy.idleEvent.OnIdle -= OnIdle;
        enemy.aimWeaponEvent.OnAimWeapon -= OnAimWeapon;
    }

    private void OnMovementToPosition(MovementToPositionEvent movementToPositionEvent, MovementToPositionArgs args)
    {
        //test code
        // if (enemy.transform.position.x < GameManager.Instance.GetPlayer().transform.position.x) {
        //     SetAimWeaponAnimationParameters(AimDirection.Right);
        // }
        // else
        // {
        //     SetAimWeaponAnimationParameters(AimDirection.Left);
        // }

        SetMovementAnimationParameters();
    }

    private void SetAimWeaponAnimationParameters(AimDirection aimDirection)
    {
        //InitialiseAimAnimationParameters();
        switch (aimDirection)
        {
            case AimDirection.Up:
                enemy.animator.SetBool(Settings.aimUp, true);
                break;
            case AimDirection.UpRight:
                enemy.animator.SetBool(Settings.aimUpRight, true);
                break;
            case AimDirection.UpLeft:
                enemy.animator.SetBool(Settings.aimUpLeft, true);
                break;
            case AimDirection.Down:
                enemy.animator.SetBool(Settings.aimDown, true);
                break;
            case AimDirection.Left:
                enemy.animator.SetBool(Settings.aimLeft, true);
                break;
            case AimDirection.Right:
                enemy.animator.SetBool(Settings.aimRight, true);
                break;
        }
    }

    private void OnIdle(IdleEvent idleEvent)
    {
        SetIdleAnimationParameters();
    }

    private void OnAimWeapon(AimWeaponEvent aimWeaponEvent, AimWeaponEventArgs args)
    {
        InitialiseAimAnimationParameters();
        SetAimWeaponAnimationParameters(args.aimDirection);
    }

    private void InitialiseAimAnimationParameters()
    {
        enemy.animator.SetBool(Settings.aimUp, false);
        enemy.animator.SetBool(Settings.aimUpRight, false);
        enemy.animator.SetBool(Settings.aimUpLeft, false);
        enemy.animator.SetBool(Settings.aimDown, false);
        enemy.animator.SetBool(Settings.aimLeft, false);
        enemy.animator.SetBool(Settings.aimRight, false);
    }

    private void SetMovementAnimationParameters()
    {
        enemy.animator.SetBool(Settings.isMoving, true);
        enemy.animator.SetBool(Settings.isIdle, false);
    }

    private void SetIdleAnimationParameters()
    {
        enemy.animator.SetBool(Settings.isMoving, false);
        enemy.animator.SetBool(Settings.isIdle, true);
    }
}
