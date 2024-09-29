
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
    [HideInInspector] public GameState previousGameState;

    private Room currentRoom;
    private Room previousRoom;
    private PlayerDetailsSO playerDetails;
    private Player player;

    private long gameScore;
    private int scoreMultiplier;
    private InstantiateRoom bossRoom;

    //ui
    [SerializeField] private TextMeshProUGUI messageTextTMP;
    [SerializeField] private CanvasGroup canvasGroup;


    //dungeon map
    private bool isFading = false;

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
        previousGameState = GameState.gameStarted;

        gameScore = 0;

        scoreMultiplier = 1;

        StartCoroutine(Fade(0f, 1f, 0f, Color.black));
    }

    public IEnumerator Fade(float startFadeAlpha, float targetFadeAlpha, float fadeSeconds, Color bgColor)
    {

        isFading = true;

        Image image = canvasGroup.GetComponent<Image>();
        image.color = bgColor;

        float time = 0;
        while (time <= fadeSeconds) {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startFadeAlpha, targetFadeAlpha, time / fadeSeconds);
            yield return null;
        }

        isFading = false;
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
        StaticEventHandler.OnPointsScored += OnPointsScored;
        StaticEventHandler.OnMultiplierChanged += OnMultiplierChanged;

        StaticEventHandler.OnRoomEnemiesDefeated += OnRoomEnemiesDefeated;
        player.destoryedEvent.OnDestroyed += OnPlayerDestroyed;
    }

    void OnDisable ()
    {
        StaticEventHandler.OnRoomChanged -= OnRoomChanged;
        StaticEventHandler.OnPointsScored -= OnPointsScored;
        StaticEventHandler.OnMultiplierChanged -= OnMultiplierChanged;

        StaticEventHandler.OnRoomEnemiesDefeated -= OnRoomEnemiesDefeated;
        player.destoryedEvent.OnDestroyed -= OnPlayerDestroyed;
    }

    private void OnPlayerDestroyed(DestoryedEvent destoryedEvent, DestoryedEventArgs args)
    {
        previousGameState = gameState;

        gameState = GameState.gameLost;
    }

    private void OnMultiplierChanged(multiplierArgs args)
    {
        if (args.multiplier) {
            scoreMultiplier++;
        }
        else
        {
            scoreMultiplier--;
        }
        scoreMultiplier = Mathf.Clamp(scoreMultiplier, 1 , 30);

        StaticEventHandler.CallScoreChangedEvent(gameScore, scoreMultiplier);
    }

    private void OnPointsScored(PointsScoredArgs args)
    {
        gameScore += args.points * scoreMultiplier;
        StaticEventHandler.CallScoreChangedEvent(gameScore, scoreMultiplier);
    }

    private void OnRoomChanged(RoomChangedEventArgs args)
    {
        SetCurrentRoom(args.room);
    }

    private void OnRoomEnemiesDefeated(RoomEnemiesDefeatedArgs args)
    {
        RoomEnemiesDefeated();
    }

    private void InstantiatePlayer()
    {
        GameObject playerGameObject = Instantiate(playerDetails.playerPrefab);

        player = playerGameObject.GetComponent<Player>();

        player.Initialize(playerDetails);
    }

    private void HandleGameState()
    {
        Debug.Log("HandleGameState: " + gameState);
        // Handle game state
        switch (gameState)
        {
            case GameState.gameStarted:

                // Play first level
                PlayDungeonLevel(currentDungeonLevelListIndex);

                gameState = GameState.playingLevel;

                // Trigger room enemies defeated since we start in the entrance where there are no enemies (just in case you have a level with just a boss room!)
                RoomEnemiesDefeated();

                break;

            //While playing the level handle the tab key for the dungeon overview map.
            case GameState.playingLevel:

                // if (Input.GetKeyDown(KeyCode.Escape))
                // {
                //     PauseGameMenu();
                // }

                if (Input.GetKeyDown(KeyCode.Tab))
                {
                    DisplayDungeonOverviewMap();
                }

                break;

            // // While engaging enemies handle the escape key for the pause menu
            // case GameState.engagingEnemies:

            //     if (Input.GetKeyDown(KeyCode.Escape))
            //     {
            //         PauseGameMenu();
            //     }

            //     break;


             // if in the dungeon overview map handle the release of the tab key to clear the map
             case GameState.dungeonOverviewMap:

                // Key released
                if (Input.GetKeyUp(KeyCode.Tab))
                {
                    // Clear dungeonOverviewMap
                    DungeonMap.Instance.ClearDungeonOverViewMap();
                }

                break;

            // While playing the level and before the boss is engaged, handle the tab key for the dungeon overview map.
            case GameState.bossStage:

                // if (Input.GetKeyDown(KeyCode.Escape))
                // {
                //     PauseGameMenu();
                // }

                if (Input.GetKeyDown(KeyCode.Tab))
                {
                    DisplayDungeonOverviewMap();
                }

                break;

            // // While engaging the boss handle the escape key for the pause menu
            // case GameState.engagingBoss:

            //     if (Input.GetKeyDown(KeyCode.Escape))
            //     {
            //         PauseGameMenu();
            //     }

            //     break;

            // handle the level being completed
            case GameState.levelCompleted:

                // Display level completed text
                StartCoroutine(LevelCompleted());

                break;

            // handle the game being won (only trigger this once - test the previous game state to do this)
            case GameState.gameWon:

                if (previousGameState != GameState.gameWon)
                    StartCoroutine(GameWon());

                break;

            // handle the game being lost (only trigger this once - test the previous game state to do this)
            case GameState.gameLost:

                if (previousGameState != GameState.gameLost)
                {
                    StopAllCoroutines(); // Prevent messages if you clear the level just as you get killed
                    StartCoroutine(GameLost());
                }

                break;

            // restart the game
            case GameState.restartGame:

                RestartGame();

                break;

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
        StartCoroutine(DisplayDungeonLevelText());

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

    public DungeonLevelSO GetCurrentDungeonLevel()
    {
        return dungeonLevelList[currentDungeonLevelListIndex];
    }

    public void RoomEnemiesDefeated()
    {
        bool isDungeonClearOfRegularEnemies = true;
        bossRoom = null;

        foreach (KeyValuePair<string, Room> keyValuePair in DungeonBuilder.Instance.dungeonBuilderRoomDictionary) 
        {
            if (keyValuePair.Value.roomNodeType.isBossRoom) {
                bossRoom = keyValuePair.Value.instantiateRoom;
                continue;
            }

            if (!keyValuePair.Value.isClearedOfEnemies) {
                isDungeonClearOfRegularEnemies = false;
                break;
            }
        }

        if ((isDungeonClearOfRegularEnemies && bossRoom == null) || (isDungeonClearOfRegularEnemies && bossRoom.room.isClearedOfEnemies)) {
            
            if (currentDungeonLevelListIndex < dungeonLevelList.Count - 1) {
                gameState = GameState.levelCompleted;
            }
            else {
                gameState = GameState.gameWon;
            }

        }
        else if (isDungeonClearOfRegularEnemies) {
            gameState = GameState.bossStage;
            StartCoroutine(StartBossStage());
        }
    }
    private IEnumerator StartBossStage() {
        bossRoom.gameObject.SetActive(true);
        bossRoom.UnlockDoors(0f);
        yield return new WaitForSeconds(2f);

        //fade in
        yield return StartCoroutine(Fade(0f, 1f, 2f, new Color(0f, 0f, 0f, 0.4f)));

        //display boss message
        yield return StartCoroutine(DisplayMessageRoutine("WELL DONE " + GameResources.Instance.currentPlayer.playerName + 
        "! YOU'VE SURVIVED ....SO FAR\n\nNOW FIND AND DEFEAT THE BOSS....GOOD LUCK!", Color.white, 5f));

        //fade out
        yield return StartCoroutine(Fade(1f, 0f, 2f, new Color(0f, 0f, 0f, 0.4f)));
    }

    private IEnumerator LevelCompleted()
    {
        gameState = GameState.playingLevel;
        yield return new WaitForSeconds(2f);

        //fade in
        yield return StartCoroutine(Fade(0f, 1f, 2f, new Color(0f, 0f, 0f, 0.4f)));

        //display level completed
        yield return StartCoroutine(DisplayMessageRoutine("WELL DONE " + GameResources.Instance.currentPlayer.playerName + 
        "! YOU'VE SURVIVED THIS DUNGEON LEVEL", Color.white, 5f));

        yield return StartCoroutine(DisplayMessageRoutine("COLLECT ANY LOOT...THEN PRESS RETURN\n\nTO DESCEND FURTHER INTO THE DUNGEON", Color.white, 5f));

        //fade out
        yield return StartCoroutine(Fade(1f, 0f, 2f, new Color(0f, 0f, 0f, 0.4f)));

        while (!Input.GetKeyDown(KeyCode.Return)) {
            yield return null;
        }

        yield return null;
        currentDungeonLevelListIndex++;

        PlayDungeonLevel(currentDungeonLevelListIndex);
    }

    private IEnumerator GameWon()
    {
        previousGameState = GameState.gameWon;

        GetPlayer().playerController.DisablePlayer();

        //fade out
        yield return StartCoroutine(Fade(0f, 1f, 2f, Color.black));

        //display game won
        yield return StartCoroutine(DisplayMessageRoutine("WELL DONE " + GameResources.Instance.currentPlayer.playerName + 
        "! YOU'VE DEFEATED THE DUNGEON", Color.white, 3f));

        yield return StartCoroutine(DisplayMessageRoutine("YOUR SCORE " + gameScore.ToString("###,###0"), Color.white, 5f));
        yield return StartCoroutine(DisplayMessageRoutine("PRESS RETURN TO RESTART THE GAME", Color.white, 0f));


        gameState = GameState.restartGame;
    }

    private IEnumerator GameLost()
    {
        previousGameState = GameState.gameLost;

        GetPlayer().playerController.DisablePlayer();

        yield return new WaitForSeconds(1f);

        //fade out
        yield return StartCoroutine(Fade(0f, 1f, 2f, Color.black));

        Enemy[] enemies = GameObject.FindObjectsOfType<Enemy>();
        foreach (Enemy enemy in enemies) {
            enemy.gameObject.SetActive(false);
        }

        //display game lost
        yield return StartCoroutine(DisplayMessageRoutine("BAD LUCK " + GameResources.Instance.currentPlayer.playerName + 
        "! YOU'VE SUCCUMBED TO THE DUNGEON", Color.white, 3f));

        yield return StartCoroutine(DisplayMessageRoutine("YOUR SCORE " + gameScore.ToString("###,###0"), Color.white, 5f));
        yield return StartCoroutine(DisplayMessageRoutine("PRESS RETURN TO RESTART THE GAME", Color.white, 0f));

        gameState = GameState.restartGame;
    }

    private void RestartGame()
    {
        SceneManager.LoadScene("MainGameScene");
    }

    private IEnumerator DisplayDungeonLevelText()
    {
        StartCoroutine(Fade(0f, 1f, 0f, Color.black));

        GetPlayer().playerController.DisablePlayer();

        string messageText = "LEVEL " + (currentDungeonLevelListIndex + 1).ToString() + "\n\n" + dungeonLevelList
            [currentDungeonLevelListIndex].levelName.ToUpper();
        yield return StartCoroutine(DisplayMessageRoutine(messageText, Color.white, 2f));

        GetPlayer().playerController.EnablePlayer();

        yield return StartCoroutine(Fade(1f, 0f, 2f, Color.black));
    }

    private IEnumerator DisplayMessageRoutine(string messageText, Color textColor, float displayTime)
    {
        messageTextTMP.SetText(messageText);
        messageTextTMP.color = textColor;

        if (displayTime > 0f) {
            float timer = displayTime;

            while (timer > 0f && !Input.GetKeyDown(KeyCode.Return)) {
                timer -= Time.deltaTime;
                yield return null;
            }
        }
        else
        {
            while (!Input.GetKeyDown(KeyCode.Return)) {
                yield return null;
            }
        }

        yield return null;

        messageTextTMP.SetText("");
    }

    private void DisplayDungeonOverviewMap()
    {
        // if (isFading)
        //     return;
        
        DungeonMap.Instance.DisplayDungeonOverViewMap();
    }
}
