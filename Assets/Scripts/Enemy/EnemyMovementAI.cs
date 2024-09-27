using System;
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

        
        int obstacle = currentRoom.instantiateRoom.aStarMovementPenalty[adjustedPlayerCellPosition.x, adjustedPlayerCellPosition.y];

        //if player is not on a cell square
        if (obstacle != 0) {
            return playerCellPosition;
        }
        else
        {
            for (int x = -1; x <= 1; x++) {
                for (int y = -1; y <= 1; y++) {
                    if (x == 0 && y == 0) continue;
                    try {
                        obstacle = currentRoom.instantiateRoom.aStarMovementPenalty[adjustedPlayerCellPosition.x + x, adjustedPlayerCellPosition.y + y];
                        if (obstacle != 0) return new Vector3Int(playerCellPosition.x + x, playerCellPosition.y + y, 0);
                    } catch {
                        continue;
                    }
                }       
            }
        }
        
        return playerCellPosition;
    }

    //optimize
    public void SetUpdateFrameNumber(int updateFrameNumber)
    {
        this.updateFrameNumber = updateFrameNumber;
    }
}
