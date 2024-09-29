public enum Orientation
{
    north,
    east,
    south,
    west,
    none
}
public enum GameState
{
    gameStarted,
    playingLevel,
    engagingEnemies,
    bossStage,
    engagingBoss,
    levelCompleted,
    gameWon,
    gameLost,
    gamePaused,
    dungeonOverviewMap,
    restartGame
}

public enum AimDirection
{
    Up,
    UpRight,
    UpLeft,
    Down,
    Left,
    Right
}

public enum ChestSpawnEvent
{
    onRoomEntry,
    onEnemiesDefeated
}

public enum ChestSpawnPosition
{
    atSpawnerPosition,
    atPlayerPosition
}

public enum ChestState
{
    closed,
    healthItem,
    ammoItem,
    weaponItem,
    empty
}
