using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(MovementToPositionEvent))]
[DisallowMultipleComponent]
public class MovementToPosition : MonoBehaviour
{
    private Rigidbody2D rb;
    private MovementToPositionEvent movementToPositionEvent;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        movementToPositionEvent = GetComponent<MovementToPositionEvent>();
    }

    private void OnEnable()
    {
        movementToPositionEvent.OnMovementToPosition += OnMovementToPosition;
    }

    void OnDisable()
    {
        movementToPositionEvent.OnMovementToPosition -= OnMovementToPosition;
    }

    private void OnMovementToPosition(MovementToPositionEvent movementToPositionEvent, MovementToPositionArgs args)
    {
        MoveRigidBody(args.movePosition, args.currentPosition, args.moveSpeed);
    }

    private void MoveRigidBody(Vector3 movePosition, Vector3 currentPosition, float moveSpeed)
    {
        Vector2 unitVector = Vector3.Normalize(movePosition - currentPosition);
        rb.MovePosition(rb.position + (unitVector * moveSpeed * Time.fixedDeltaTime));
    }
}
