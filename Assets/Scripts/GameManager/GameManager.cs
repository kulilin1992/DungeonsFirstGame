using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class GameManager : SingletonMonobehaviour<GameManager>
{

     #region Header DUNGEON LEVELS

    [Space(10)]
    [Header("DUNGEON LEVELS")]

    #endregion Header DUNGEON LEVELS

    #region Tooltip

    [Tooltip("Populate with the dungeon level scriptable objects")]

    #endregion Tooltip

    [SerializeField] private List<DungeonLevelSO> dungeonLevelList;

    #region Tooltip

    [Tooltip("Populate with the starting dungeon level for testing , first level = 0")]

    #endregion Tooltip

    [SerializeField] private int currentDungeonLevelListIndex = 0;
    [HideInInspector] public GameState gameState;

    private Room currentRoom;
    private Room previousRoom;
    private PlayerDetailsSO playerDetails;
    private Player player;

    protected override void Awake()
    {
        base.Awake();
        playerDetails = GameResources.Instance.currentPlayer.playerDetails;

        InstantiatePlayer();
    }

    // Start is called before the first frame update
    private void Start()
    {
        gameState = GameState.gameStarted;
    }

    // Update is called once per frame
    private void Update()
    {
        HandleGameState();

        //for testing
        // if (Input.GetKeyDown(KeyCode.R)) {
        //     gameState = GameState.gameStarted;
        // }
    }

    #region Validation

#if UNITY_EDITOR

    private void OnValidate()
    {
        //HelperUtilities.ValidateCheckNullValue(this, nameof(pauseMenu), pauseMenu);
        //HelperUtilities.ValidateCheckNullValue(this, nameof(messageTextTMP), messageTextTMP);
        //HelperUtilities.ValidateCheckNullValue(this, nameof(canvasGroup), canvasGroup);
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(dungeonLevelList), dungeonLevelList);
    }

#endif

    #endregion Validation

    private void OnEnable ()
    {
        StaticEventHandler.OnRoomChanged += OnRoomChanged;
    }

    void OnDisable ()
    {
        StaticEventHandler.OnRoomChanged -= OnRoomChanged;
    }

    private void OnRoomChanged(RoomChangedEventArgs args)
    {
        SetCurrentRoom(args.room);
    }

    private void InstantiatePlayer()
    {
        GameObject playerGameObject = Instantiate(playerDetails.playerPrefab);

        player = playerGameObject.GetComponent<Player>();

        player.Initialize(playerDetails);
    }

    private void HandleGameState()
    {
        // Handle game state
        switch (gameState)
        {
            case GameState.gameStarted:

                // Play first level
                PlayDungeonLevel(currentDungeonLevelListIndex);

                gameState = GameState.playingLevel;

                // Trigger room enemies defeated since we start in the entrance where there are no enemies (just in case you have a level with just a boss room!)
                //RoomEnemiesDefeated();

                break;

            // While playing the level handle the tab key for the dungeon overview map.
            // case GameState.playingLevel:

            //     if (Input.GetKeyDown(KeyCode.Escape))
            //     {
            //         PauseGameMenu();
            //     }

            //     if (Input.GetKeyDown(KeyCode.Tab))
            //     {
            //         DisplayDungeonOverviewMap();
            //     }

            //     break;

            // // While engaging enemies handle the escape key for the pause menu
            // case GameState.engagingEnemies:

            //     if (Input.GetKeyDown(KeyCode.Escape))
            //     {
            //         PauseGameMenu();
            //     }

            //     break;


            // // if in the dungeon overview map handle the release of the tab key to clear the map
            // case GameState.dungeonOverviewMap:

            //     // Key released
            //     if (Input.GetKeyUp(KeyCode.Tab))
            //     {
            //         // Clear dungeonOverviewMap
            //         DungeonMap.Instance.ClearDungeonOverViewMap();
            //     }

            //     break;

            // // While playing the level and before the boss is engaged, handle the tab key for the dungeon overview map.
            // case GameState.bossStage:

            //     if (Input.GetKeyDown(KeyCode.Escape))
            //     {
            //         PauseGameMenu();
            //     }

            //     if (Input.GetKeyDown(KeyCode.Tab))
            //     {
            //         DisplayDungeonOverviewMap();
            //     }

            //     break;

            // // While engaging the boss handle the escape key for the pause menu
            // case GameState.engagingBoss:

            //     if (Input.GetKeyDown(KeyCode.Escape))
            //     {
            //         PauseGameMenu();
            //     }

            //     break;

            // // handle the level being completed
            // case GameState.levelCompleted:

            //     // Display level completed text
            //     StartCoroutine(LevelCompleted());

            //     break;

            // // handle the game being won (only trigger this once - test the previous game state to do this)
            // case GameState.gameWon:

            //     if (previousGameState != GameState.gameWon)
            //         StartCoroutine(GameWon());

            //     break;

            // // handle the game being lost (only trigger this once - test the previous game state to do this)
            // case GameState.gameLost:

            //     if (previousGameState != GameState.gameLost)
            //     {
            //         StopAllCoroutines(); // Prevent messages if you clear the level just as you get killed
            //         StartCoroutine(GameLost());
            //     }

            //     break;

            // // restart the game
            // case GameState.restartGame:

            //     RestartGame();

            //     break;

            // // if the game is paused and the pause menu showing, then pressing escape again will clear the pause menu
            // case GameState.gamePaused:
            //     if (Input.GetKeyDown(KeyCode.Escape))
            //     {
            //         PauseGameMenu();
            //     }
            //     break;
        }
    }
     private void PlayDungeonLevel(int dungeonLevelListIndex)
    {
        // Build dungeon for level
        bool dungeonBuiltSucessfully = DungeonBuilder.Instance.GenerateDungeon(dungeonLevelList[dungeonLevelListIndex]);

        if (!dungeonBuiltSucessfully)
        {
            Debug.LogError("Couldn't build dungeon from specified rooms and node graphs");
        }

        // Call static event that room has changed.
        StaticEventHandler.CallRoomChangedEvent(currentRoom);

        // Set player roughly mid-room
        player.gameObject.transform.position = new Vector3((currentRoom.lowerBounds.x + currentRoom.upperBounds.x) / 2f,
            (currentRoom.lowerBounds.y + currentRoom.upperBounds.y) / 2f, 0f);

        // Get nearest spawn point in room nearest to player
        player.gameObject.transform.position = HelperUtilities.GetSpawnPositionNearestToPlayer(player.gameObject.transform.position);

        // // Display Dungeon Level Text
        // StartCoroutine(DisplayDungeonLevelText());

        //// ** Demo code
        //RoomEnemiesDefeated();
    }

    public void SetCurrentRoom(Room room)
    {
        previousRoom = currentRoom;
        currentRoom = room;
    }

    public Room GetCurrentRoom()
    {
        return currentRoom;
    }

    public Player GetPlayer() {
        return player;
    }

    public Sprite GetPlayerMiniMapIcon()
    {
        return playerDetails.playerMiniMapIcon;
    }
}
