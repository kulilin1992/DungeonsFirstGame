
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Enemy))]
[DisallowMultipleComponent]
public class EnemyMovementAI : MonoBehaviour
{
    [HideInInspector] public float moveSpeed;

    [HideInInspector] public int updateFrameNumber= 1;
    [SerializeField] private MovementDetailsSO movementDetails;
    
    private Enemy enemy;
    private Stack<Vector3> movementSteps = new Stack<Vector3>();
    private Vector3 playerReferencePosition;
    private Coroutine moveEnemyRoutine;
    private float currentEnemyPathRebuildCooldown;
    private WaitForFixedUpdate waitForFixedUpdate;
    private bool chasePlayer = false;

    //move item
    private List<Vector2Int> surroundingPositionList = new List<Vector2Int>();

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
        moveSpeed = movementDetails.GetMoveSpeed();
    }

    private void Start()
    {
        waitForFixedUpdate = new WaitForFixedUpdate();
        playerReferencePosition = GameManager.Instance.GetPlayer().GetPlayerPosition();
    }

    private void Update()
    {
        EnemyMove();
    }

    private void EnemyMove()
    {
        currentEnemyPathRebuildCooldown -= Time.deltaTime;

        if (!chasePlayer && Vector3.Distance(transform.position, GameManager.Instance.GetPlayer().GetPlayerPosition()) <
            enemy.enemyDetails.chaseDistance) {
            chasePlayer = true;
        }

        if (!chasePlayer) {
            return;
        }

        //optimize
        if (Time.frameCount % Settings.targetFrameRateToSpreadPathFindingOver != updateFrameNumber) return;

        if (currentEnemyPathRebuildCooldown <= 0f || (Vector3.Distance(playerReferencePosition, GameManager.Instance.GetPlayer().GetPlayerPosition()) >
            Settings.playerMoveDistanceToRebuildPath)) {
            // Rebuild path
            currentEnemyPathRebuildCooldown = Settings.enemyPathRebuildCooldown;

            playerReferencePosition = GameManager.Instance.GetPlayer().GetPlayerPosition();

            CreatePath();

            if (movementSteps != null) {
                if (moveEnemyRoutine != null) {

                    enemy.idleEvent.CallIdleEvent();
                    StopCoroutine(moveEnemyRoutine);
                }

                moveEnemyRoutine = StartCoroutine(MoveEnemyRoutine(movementSteps));
            }
        }
    }

    private IEnumerator MoveEnemyRoutine(Stack<Vector3> movementSteps)
    {
        while (movementSteps.Count > 0) {
            Vector3 nextPosition = movementSteps.Pop();
            
            //while not very close continue to move
            while (Vector3.Distance(transform.position, nextPosition) > 0.2f) {
                enemy.movementToPositionEvent.CallMovementToPositionEvent(nextPosition, transform.position, moveSpeed, 
                    (nextPosition - transform.position).normalized);

                yield return waitForFixedUpdate;
            }
            yield return waitForFixedUpdate;
            
        }
        enemy.idleEvent.CallIdleEvent();
    }

    private void CreatePath()
    {
        Room currentRoom = GameManager.Instance.GetCurrentRoom();

        Grid grid = currentRoom.instantiateRoom.grid;

        Vector3Int playerGridPosition = GetNearestNonObstaclePlayerPosition(currentRoom);

        Vector3Int enemyGridPosition = grid.WorldToCell(transform.position);

        movementSteps = AStar.BuildPath(currentRoom, enemyGridPosition, playerGridPosition);

        if (movementSteps != null) {
            movementSteps.Pop();
        }
        else
        {
            Debug.Log("No path found");
            enemy.idleEvent.CallIdleEvent();
        }
    }

    private Vector3Int GetNearestNonObstaclePlayerPosition(Room currentRoom)
    {
        Vector3 playerPosition = GameManager.Instance.GetPlayer().GetPlayerPosition();

        Vector3Int playerCellPosition = currentRoom.instantiateRoom.grid.WorldToCell(playerPosition);

        Vector2Int adjustedPlayerCellPosition = new Vector2Int(playerCellPosition.x - currentRoom.templateLowerBounds.x, 
            playerCellPosition.y - currentRoom.templateLowerBounds.y);

        
        //int obstacle = currentRoom.instantiateRoom.aStarMovementPenalty[adjustedPlayerCellPosition.x, adjustedPlayerCellPosition.y];
        int obstacle = Mathf.Min(currentRoom.instantiateRoom.aStarMovementPenalty[adjustedPlayerCellPosition.x, adjustedPlayerCellPosition.y],
            currentRoom.instantiateRoom.aStarItemObstacles[adjustedPlayerCellPosition.x, adjustedPlayerCellPosition.y]);

        //if player is not on a cell square
        if (obstacle != 0) {
            return playerCellPosition;
        }
        else
        {
            // Empty surrounding position list
            surroundingPositionList.Clear();

            // Populate surrounding position list - this will hold the 8 possible vector locations surrounding a (0,0) grid square
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (j == 0 && i == 0) continue;

                    surroundingPositionList.Add(new Vector2Int(i, j));
                }
            }

             // Loop through all positions
            for (int l = 0; l < 8; l++)
            {
                // Generate a random index for the list
                int index = Random.Range(0, surroundingPositionList.Count);

                // See if there is an obstacle in the selected surrounding position
                try
                {
                    obstacle = Mathf.Min(currentRoom.instantiateRoom.aStarMovementPenalty[adjustedPlayerCellPosition.x + surroundingPositionList[index].x,
                        adjustedPlayerCellPosition.y + surroundingPositionList[index].y], currentRoom.instantiateRoom.aStarItemObstacles[adjustedPlayerCellPosition.x + surroundingPositionList[index].x,
                        adjustedPlayerCellPosition.y + surroundingPositionList[index].y]);

                    // If no obstacle return the cell position to navigate to
                    if (obstacle != 0)
                    {
                        return new Vector3Int(playerCellPosition.x + surroundingPositionList[index].x, playerCellPosition.y + surroundingPositionList[index].y, 0);
                    }

                }
                // Catch errors where the surrounding positon is outside the grid
                catch
                {

                }

                // Remove the surrounding position with the obstacle so we can try again
                surroundingPositionList.RemoveAt(index);
            }

            // If no non-obstacle cells found surrounding the player - send the enemy in the direction of an enemy spawn position
            return (Vector3Int)currentRoom.spawnPositionArray[Random.Range(0, currentRoom.spawnPositionArray.Length)];
            // for (int x = -1; x <= 1; x++) {
            //     for (int y = -1; y <= 1; y++) {
            //         if (x == 0 && y == 0) continue;
            //         try {
            //             obstacle = currentRoom.instantiateRoom.aStarMovementPenalty[adjustedPlayerCellPosition.x + x, adjustedPlayerCellPosition.y + y];
            //             if (obstacle != 0) return new Vector3Int(playerCellPosition.x + x, playerCellPosition.y + y, 0);
            //         } catch {
            //             continue;
            //         }
            //     }       
            // }
        }
        
        //return playerCellPosition;
    }

    //optimize
    public void SetUpdateFrameNumber(int updateFrameNumber)
    {
        this.updateFrameNumber = updateFrameNumber;
    }
}
