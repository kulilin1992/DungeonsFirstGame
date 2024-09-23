using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(MovementByVelocityEvent))]
[DisallowMultipleComponent]
public class MovementByVelocity : MonoBehaviour
{
    private Rigidbody2D rb;
    private MovementByVelocityEvent movementEvent;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        movementEvent = GetComponent<MovementByVelocityEvent>();
    }

    private void OnEnable()
    {
        movementEvent.OnMovementByVelocity += OnMovementByVelocity;
    }
    void OnDisable()
    {
        movementEvent.OnMovementByVelocity -= OnMovementByVelocity;
    }

    private void OnMovementByVelocity(MovementByVelocityEvent movementByVelocityEvent, MovementByVelocityArgs args)
    {
        MoveRighdBody(args.moveDirection, args.moveSpeed);
    }

    private void MoveRighdBody(Vector2 moveDirection, float moveSpeed)
    {
        rb.velocity = moveDirection * moveSpeed;
        //rb.AddForce(moveDirection * moveSpeed, ForceMode2D.Impulse
    }
}

