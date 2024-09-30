using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[DisallowMultipleComponent]
[RequireComponent(typeof(BoxCollider2D))]
public class InstantiateRoom : MonoBehaviour
{
    [HideInInspector] public Room room;
    [HideInInspector] public Grid grid;
    [HideInInspector] public Tilemap groundTilemap;
    [HideInInspector] public Tilemap decoration1Tilemap;
    [HideInInspector] public Tilemap decoration2Tilemap;
    [HideInInspector] public Tilemap frontTilemap;
    [HideInInspector] public Tilemap collisionTilemap;
    [HideInInspector] public Tilemap minimapTilemap;
    [HideInInspector] public Bounds roomColliderBounds;


    //astar
    [HideInInspector] public int[,] aStarMovementPenalty;
    [HideInInspector] public int[,] aStarItemObstacles;

    private BoxCollider2D boxCollider2D;

    //enviroment
    [SerializeField] private GameObject enviromentGameObject;

    //moveItem
    [HideInInspector] public List<MoveItem> moveableItemsList = new List<MoveItem>();

    private void Awake() {
        boxCollider2D = GetComponent<BoxCollider2D>();
        roomColliderBounds = boxCollider2D.bounds;
    }

    private void Start() {
        UpdateMoveableObstacles();
    }

    private void OnTriggerEnter2D(Collider2D collision){
        if (collision.tag == Settings.playerTag && room != GameManager.Instance.GetCurrentRoom()) {
            this.room.isPreviouslyVisited = true;
            StaticEventHandler.CallRoomChangedEvent(room);
        }
    }
    public void Initialise(GameObject roomGameObject) {
        PopulateTilemapMemberVariables(roomGameObject);

        BlockOffUnusedDoorWays();

        AddObstaclesAndPreferredPaths();

        CreateItemObstaclesArray();

        AddDoorsToRoom();

        DisableCollisionTilemapRenderer();
    }

    private void PopulateTilemapMemberVariables(GameObject roomGameObject)
    {
        grid = roomGameObject.GetComponentInChildren<Grid>();

        Tilemap[] tilemaps = roomGameObject.GetComponentsInChildren<Tilemap>();

        foreach (Tilemap tilemap in tilemaps)
        {
            if (tilemap.gameObject.tag == "groundTilemap") {
                groundTilemap = tilemap;
            }
            else if (tilemap.gameObject.tag == "decoration1Tilemap") {
                decoration1Tilemap = tilemap;
            }
            else if (tilemap.gameObject.tag == "decoration2Tilemap") {
                decoration2Tilemap = tilemap;
            }
            else if (tilemap.gameObject.tag == "frontTilemap") {
                frontTilemap = tilemap;
            }
            else if (tilemap.gameObject.tag == "collisionTilemap") {
                collisionTilemap = tilemap;
            }
            else if (tilemap.gameObject.tag == "minimapTilemap") {
                minimapTilemap = tilemap;
            }
        }
    }

    private void DisableCollisionTilemapRenderer()
    {
        collisionTilemap.gameObject.GetComponent<TilemapRenderer>().enabled = false;
    }

    private void BlockOffUnusedDoorWays()
    {
        // TODO: Block off unused door ways
        foreach (Doorway doorway in room.doorWayList) {
            if (doorway.isConnected) {
                continue;
            }

            if (collisionTilemap != null) {
                BlockDoorwayOnTilemapLayer(collisionTilemap, doorway);
            }

            if (minimapTilemap != null) {
                BlockDoorwayOnTilemapLayer(minimapTilemap, doorway);
            }

            if (groundTilemap != null) {
                BlockDoorwayOnTilemapLayer(groundTilemap, doorway);
            }

            if (decoration1Tilemap != null) {
                BlockDoorwayOnTilemapLayer(decoration1Tilemap, doorway);
            }

            if (decoration2Tilemap != null) {
                BlockDoorwayOnTilemapLayer(decoration2Tilemap, doorway);
            }
            if (frontTilemap != null) {
                BlockDoorwayOnTilemapLayer(frontTilemap, doorway);
            }
        
        }
    }

    private void BlockDoorwayOnTilemapLayer(Tilemap tilemap, Doorway doorway)
    {
        switch (doorway.orientation) {
            case Orientation.north:
            case Orientation.south:
                BlockDoorwayHorizontally(tilemap, doorway);
                break;
            
            case Orientation.east:
            case Orientation.west:
                BlockDoorwayVertically(tilemap, doorway);
                break;

            case Orientation.none:
                break;
            
            default:
                break;

        }
    }


    //for east and west
    private void BlockDoorwayVertically(Tilemap tilemap, Doorway doorway)
    {
        Vector2Int startPosition = doorway.doorwayStartCopyPosition;

        for (int yPos = 0; yPos < doorway.doorwayCopyTileHeight; yPos++) {
            for (int xPos = 0; xPos < doorway.doorwayCopyTileWidth; xPos++) {

                // get rotation of tile being copied
                Matrix4x4 transformMatrix = tilemap.GetTransformMatrix(new Vector3Int(startPosition.x + xPos, startPosition.y - yPos, 0));

                //copy tile
                tilemap.SetTile(new Vector3Int(startPosition.x + xPos, startPosition.y - 1- yPos, 0), tilemap.GetTile(
                    new Vector3Int(startPosition.x + xPos, startPosition.y - yPos, 0)));

                // set rotation of tile being copied
                tilemap.SetTransformMatrix(new Vector3Int(startPosition.x + xPos, startPosition.y - 1 - yPos, 0), transformMatrix);

            }
        }
    }



    //for north and south
    private void BlockDoorwayHorizontally(Tilemap tilemap, Doorway doorway)
    {
        Vector2Int startPosition = doorway.doorwayStartCopyPosition;

        for (int xPos = 0; xPos < doorway.doorwayCopyTileWidth; xPos++) {
            for (int yPos = 0; yPos < doorway.doorwayCopyTileHeight; yPos++) {

                // get rotation of tile being copied
                Matrix4x4 transformMatrix = tilemap.GetTransformMatrix(new Vector3Int(startPosition.x + xPos, startPosition.y - yPos, 0));

                //copy tile
                tilemap.SetTile(new Vector3Int(startPosition.x + 1 + xPos, startPosition.y - yPos, 0), tilemap.GetTile(
                    new Vector3Int(startPosition.x + xPos, startPosition.y - yPos, 0)));

                // set rotation of tile being copied
                tilemap.SetTransformMatrix(new Vector3Int(startPosition.x + 1 + xPos, startPosition.y - yPos, 0), transformMatrix);

            }
        }
    }

    private void AddDoorsToRoom()
    {
        if (room.roomNodeType.isCorridorEW || room.roomNodeType.isCorridorNS) return;
        foreach (Doorway doorway in room.doorWayList) {
            if (doorway.doorPrefab != null && doorway.isConnected) {
                float tileDistance = Settings.tileSizePixel / Settings.pixelPerUnit;
                GameObject door = null;

                if (doorway.orientation == Orientation.north) {
                    door = Instantiate(doorway.doorPrefab, gameObject.transform);
                    door.transform.localPosition = new Vector3(doorway.position.x + tileDistance / 2f, doorway.position.y + tileDistance, 0);
                }
                else if (doorway.orientation == Orientation.south)
                {
                    door = Instantiate(doorway.doorPrefab, gameObject.transform);
                    door.transform.localPosition = new Vector3(doorway.position.x + tileDistance / 2f, doorway.position.y, 0);
                }
                else if ((doorway.orientation == Orientation.east))
                {
                    door = Instantiate(doorway.doorPrefab, gameObject.transform);
                    door.transform.localPosition = new Vector3(doorway.position.x + tileDistance, doorway.position.y + tileDistance * 1.25f, 0);
                }
                else if (doorway.orientation == Orientation.west)
                {
                    door = Instantiate(doorway.doorPrefab, gameObject.transform);
                    door.transform.localPosition = new Vector3 (doorway.position.x, doorway.position.y + tileDistance * 1.25f, 0);
                }

                if (room.roomNodeType.isBossRoom) {
                    Door doorComponent = door.GetComponent<Door>();
                    if (doorComponent != null) {
                        doorComponent.isBossRoomDoor = true;
                        doorComponent.LockDoor();

                        GameObject skullIcon = Instantiate(GameResources.Instance.minimapSkullPrefab, gameObject.transform);
                        skullIcon.transform.localPosition = door.transform.localPosition;
                    }
                }
            }
        }
    }

    private void AddObstaclesAndPreferredPaths() 
    {
        //this array will be populated with wall obstacles
        aStarMovementPenalty = new int[room.templateUpperBounds.x - room.templateLowerBounds.x + 1,
            room.templateUpperBounds.y - room.templateLowerBounds.y + 1];

        //loop through all the grid squares
        for (int x = 0; x < (room.templateUpperBounds.x - room.templateLowerBounds.x + 1); x++) 
        {
            for (int y = 0; y < (room.templateUpperBounds.y - room.templateLowerBounds.y + 1); y++)
            {
                aStarMovementPenalty[x,y] = Settings.defaultAStarMovementPenalty;

                TileBase tile = collisionTilemap.GetTile(new Vector3Int(x + room.templateLowerBounds.x, y + room.templateLowerBounds.y, 0));

                foreach (TileBase collisionTile in GameResources.Instance.enemyUnwalkableCollisionTilesArray) {
                    if (tile == collisionTile) {
                        aStarMovementPenalty[x,y] = 0;
                        break;
                    }
                }

                //add preferred paths for enemies(1 is the preffered path value, default value
                //for a grid location is specified int the settings
                

                if (tile == GameResources.Instance.preferedEnemyPathTile) {
                    aStarMovementPenalty[x,y] = Settings.preferredPathAStarMovementPenalty;
                }
            }
        }
    }

    public void LockDoors()
    {
        Door[] doors = GetComponentsInChildren<Door>();

        foreach (Door door in doors)
        {
            door.LockDoor();
        }
        DisableRoomCollider();
    }

    public void UnlockDoors(float doorUnlockDelay)
    {
        StartCoroutine(UnlockDoorsRoutine(doorUnlockDelay));
    }

    private IEnumerator UnlockDoorsRoutine(float doorUnlockDelay)
    {
        if (doorUnlockDelay > 0) {
            yield return new WaitForSeconds(doorUnlockDelay);
        }

        Door[] doors = GetComponentsInChildren<Door>();

        foreach (Door door in doors)
        {
            door.UnlockDoor();
        }
        EnableRoomCollider();
    }

    private void DisableRoomCollider()
    {
        boxCollider2D.enabled = false;
    }

    private void EnableRoomCollider()
    {
        boxCollider2D.enabled = true;
    }

    public void ActivateEnviromentGameObjects()
    {
        if (enviromentGameObject != null) {
            enviromentGameObject.SetActive(true);
        }
    }

    public void DeactivateEnviromentGameObjects()
    {
        if (enviromentGameObject != null) {
            enviromentGameObject.SetActive(false);
        }
    }

    private void CreateItemObstaclesArray()
    {
        // this array will be populated during gameplay with any moveable obstacles
        aStarItemObstacles = new int[room.templateUpperBounds.x - room.templateLowerBounds.x + 1, 
            room.templateUpperBounds.y - room.templateLowerBounds.y + 1];
    }

    public void UpdateMoveableObstacles()
    {
        InitializeItemObstaclesArray();

        foreach (MoveItem moveItem in moveableItemsList)
        {
            Vector3Int colliderBoundsMin = grid.WorldToCell(moveItem.boxCollider.bounds.min);
            Vector3Int colliderBoundsMax = grid.WorldToCell(moveItem.boxCollider.bounds.max);

            // Loop through and add moveable item collider bounds to obstacle array
            for (int i = colliderBoundsMin.x; i <= colliderBoundsMax.x; i++)
            {
                for (int j = colliderBoundsMin.y; j <= colliderBoundsMax.y; j++)
                {
                    aStarItemObstacles[i - room.templateLowerBounds.x, j - room.templateLowerBounds.y] = 0;
                }
            }
        }
    }

    private void InitializeItemObstaclesArray()
    {
        for (int x = 0; x < (room.templateUpperBounds.x - room.templateLowerBounds.x + 1); x++)
        {
            for (int y = 0; y < (room.templateUpperBounds.y - room.templateLowerBounds.y + 1); y++)
            {
                // Set default movement penalty for grid sqaures
                aStarItemObstacles[x, y] = Settings.defaultAStarMovementPenalty;
            }
        }
    }

    ///// <summary>
    ///// This is used for debugging - shows the position of the table obstacles. 
    ///// (MUST BE COMMENTED OUT BEFORE UPDATING ROOM PREFABS)
    ///// </summary>
    // private void OnDrawGizmos()
    // {

    //    for (int i = 0; i < (room.templateUpperBounds.x - room.templateLowerBounds.x + 1); i++)
    //    {
    //        for (int j = 0; j < (room.templateUpperBounds.y - room.templateLowerBounds.y + 1); j++)
    //        {
    //            if (aStarItemObstacles[i, j] == 0)
    //            {
    //                Vector3 worldCellPos = grid.CellToWorld(new Vector3Int(i + room.templateLowerBounds.x, j + room.templateLowerBounds.y, 0));

    //                Gizmos.DrawWireCube(new Vector3(worldCellPos.x + 0.5f, worldCellPos.y + 0.5f, 0), Vector3.one);
    //            }
    //        }
    //    }

    // }

}
