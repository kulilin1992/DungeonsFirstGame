using Cinemachine;
using UnityEngine;

[DisallowMultipleComponent]
public class MiniMap : MonoBehaviour
{
    [SerializeField] private GameObject miniMapPlayer;
    private Transform playerTransform;

    private void Start()
    {
        playerTransform = GameManager.Instance.GetPlayer().transform;

        CinemachineVirtualCamera vcam = GetComponentInChildren<CinemachineVirtualCamera>();
        vcam.Follow = playerTransform;

        SpriteRenderer renderer = miniMapPlayer.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.sprite = GameManager.Instance.GetPlayerMiniMapIcon();
        }
    }

    private void Update()
    {
        if (playerTransform != null && miniMapPlayer != null)
        {
            miniMapPlayer.transform.position = playerTransform.position;
        }
    }
}
