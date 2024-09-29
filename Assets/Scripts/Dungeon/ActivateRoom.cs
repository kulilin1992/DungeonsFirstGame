using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateRoom : MonoBehaviour
{
    [SerializeField] private Camera miniMapCamera;

    private Camera cameraMain;

    private void Start()
    {
        cameraMain = Camera.main;
        
        InvokeRepeating("EnableRooms", 0.5f, 0.5f);
    }

    private void EnableRooms()
    {

        if (GameManager.Instance.gameState == GameState.dungeonOverviewMap)
            return;


        HelperUtilities.CameraWorldPositionBounds(out Vector2Int miniMapCameraWorldPositionLowerBounds, out Vector2Int
                miniMapCameraWorldPositionUpperBounds, miniMapCamera);
        HelperUtilities.CameraWorldPositionBounds(out Vector2Int mainCameraWorldPositionLowerBounds, out Vector2Int
                mainCameraWorldPositionUpperBounds, cameraMain);

        foreach (KeyValuePair<string, Room> keyValuePai in DungeonBuilder.Instance.dungeonBuilderRoomDictionary) {
            Room room = keyValuePai.Value;

            // HelperUtilities.CameraWorldPositionBounds(out Vector2Int miniMapCameraWorldPositionLowerBounds, out Vector2Int
            //     miniMapCameraWorldPositionUpperBounds, miniMapCamera);
            if ((room.lowerBounds.x <= miniMapCameraWorldPositionUpperBounds.x && room.lowerBounds.y <= miniMapCameraWorldPositionUpperBounds.y) && (
                room.upperBounds.x >= miniMapCameraWorldPositionLowerBounds.x && room.upperBounds.y >= miniMapCameraWorldPositionLowerBounds.y))
            {
                room.instantiateRoom.gameObject.SetActive(true);

                if ((room.lowerBounds.x <= mainCameraWorldPositionUpperBounds.x && room.lowerBounds.y <= mainCameraWorldPositionUpperBounds.y) && (
                    room.upperBounds.x >= mainCameraWorldPositionLowerBounds.x && room.upperBounds.y >= mainCameraWorldPositionLowerBounds.y))
                {
                    room.instantiateRoom.ActivateEnviromentGameObjects();
                }
                else
                {
                    room.instantiateRoom.DeactivateEnviromentGameObjects();
                }


            }
            else
            {
                room.instantiateRoom.gameObject.SetActive(false);
            }
        }
    }
}

