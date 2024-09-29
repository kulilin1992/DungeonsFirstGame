using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Settings
{
    //build dungeon
    public const int maxDungeonRebuildAttemptsForRoomGraph = 1000;
    public const int maxDungeonBuildAttempts = 10;
    public const int maxChildCorridors = 3;
    public const float fadeInTime = 0.5f;

    #region ANIMATOR PARAMETERS
    public static int aimUp = Animator.StringToHash("aimUp");
    public static int aimDown = Animator.StringToHash("aimDown");
    public static int aimLeft = Animator.StringToHash("aimLeft");
    public static int aimRight = Animator.StringToHash("aimRight");
    public static int aimUpLeft = Animator.StringToHash("aimUpLeft");
    public static int aimUpRight = Animator.StringToHash("aimUpRight");
    public static int isIdle = Animator.StringToHash("isIdle");
    public static int isMoving = Animator.StringToHash("isMoving");
    public static int rollUp = Animator.StringToHash("rollUp");
    public static int rollRight = Animator.StringToHash("rollRight");
    public static int rollLeft = Animator.StringToHash("rollLeft");
    public static int rollDown = Animator.StringToHash("rollDown");

    public static float baseSpeedForPlayerAnimations = 8f;

    public static float baseSpeedForEnemyAnimations = 3f;
    #endregion

    //door
    public const float pixelPerUnit = 16f;
    public const float tileSizePixel = 16f;
    public static int open = Animator.StringToHash("open");

    public const float doorUnlockDelay = 1f;

    //gameobject tags
    public const string playerTag = "Player";
    public const string playerWeapon = "playerWeapon";

    //fire
    public const float useAimAngleDistance = 3.5f;

    //ui
    public const float uiAmmoIconSpacing = 4f;
    public const float uiHeartSpacing = 16f;

    //Astar
    public const int defaultAStarMovementPenalty = 40;
    public const int preferredPathAStarMovementPenalty = 1;
    public const float playerMoveDistanceToRebuildPath = 3f;
    public const float enemyPathRebuildCooldown = 2f;

    public const int targetFrameRateToSpreadPathFindingOver = 60;

    //enemy parameters
    public const int defaultEnemyHealth = 20;


    //contact damage
    public const float contactDamageCollisionResetDelay = 0.5f;

    //enviroment item
    public static int destory = Animator.StringToHash("destroy");
    public static string stateDestoryed = "Destroyed";

    //chest animator
    public static int use = Animator.StringToHash("use");

    //game music
    public const float musicFadeInTime = 0.5f;
    public const float musicFadeOutTime = 0.5f;
}
