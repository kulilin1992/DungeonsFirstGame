using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class DoorLightingController : MonoBehaviour
{
    private bool isLit = false;
    private Door door;
    private void Awake()
    {
        door = GetComponentInParent<Door>();
    }

    public void FadeInDoor(Door door)
    {
        Material material = new Material(GameResources.Instance.variableLitShader);
        if (!isLit) {
            SpriteRenderer[] spriteRenderers = GetComponentsInParent<SpriteRenderer>();
            foreach (SpriteRenderer spriteRenderer in spriteRenderers)
            {
                StartCoroutine(FadeInDoorCoutine(spriteRenderer, material));
            }
            isLit = true;
        }
    }

    private IEnumerator FadeInDoorCoutine(SpriteRenderer spriteRenderer, Material material)
    {
        spriteRenderer.material = material;
        for (float i = 0.05f; i <= 1f; i+=Time.deltaTime / Settings.fadeInTime) {
            material.SetFloat("Alpha_Slider", i);
            yield return null;
        }

        spriteRenderer.material = GameResources.Instance.litDefaultMaterial;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // if (collision.CompareTag("Player")) {
        //     FadeInDoor(door);
        // }
        FadeInDoor(door);
    }
}
