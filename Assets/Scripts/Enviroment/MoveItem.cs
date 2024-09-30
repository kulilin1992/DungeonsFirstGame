using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class MoveItem : MonoBehaviour
{
    [SerializeField] private SoundEffectSO moveSoundEffect;

    [HideInInspector] public BoxCollider2D boxCollider;
    private Rigidbody2D rb;
    private InstantiateRoom instantiateRoom;
    private Vector3 previousPosition;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        instantiateRoom = FindObjectOfType<InstantiateRoom>();

        instantiateRoom.moveableItemsList.Add(this);
    }

    private void OnCollistionStay2D(Collision2D collision)
    {
        UpdateObstacles();
    }

    private void UpdateObstacles()
    {
        ConfineItemToRoomBounds();

        instantiateRoom.UpdateMoveableObstacles();

        previousPosition = transform.position;

        if (Mathf.Abs(rb.velocity.x) > 0.001f || Mathf.Abs(rb.velocity.y) > 0.001f)
        {
            if (moveSoundEffect != null && Time.frameCount % 10 == 0)
            {
                SoundEffectManager.Instance.PlaySoundEffect(moveSoundEffect);
            } 
        }
    }
    private void ConfineItemToRoomBounds()
    {
        Bounds itemBounds = boxCollider.bounds;
        Bounds roomBounds = instantiateRoom.roomColliderBounds;

        if (itemBounds.min.x <= roomBounds.min.x ||
            itemBounds.max.x >= roomBounds.max.x ||
            itemBounds.min.y <= roomBounds.min.y ||
            itemBounds.max.y >= roomBounds.max.y)
        {
            transform.position = previousPosition;
        }
    }
}
