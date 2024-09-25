using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HelperUtilities
{

    public static Camera mainCamera;

    public static Vector3 GetMouseWorldPosition() {
        if (mainCamera == null) mainCamera = Camera.main;

        Vector3 mouseScreenPosition = Input.mousePosition;

        //clamp mouse position to screen size
        mouseScreenPosition.x = Mathf.Clamp(mouseScreenPosition.x, 0, Screen.width);
        mouseScreenPosition.y = Mathf.Clamp(mouseScreenPosition.y, 0, Screen.height);

        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(mouseScreenPosition);
        worldPosition.z = 0f;
        return worldPosition;
    }

    public static float GetAngleFromVector(Vector3 vector) {
        float radians = Mathf.Atan2(vector.y, vector.x);
        float degrees = radians * Mathf.Rad2Deg;
        return degrees;
    }

    public static Vector3 GetDirectionVectorFromAngle(float angle) {
        Vector3 directionVector = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0f);
        return directionVector;
    }

    public static AimDirection GetAimDirection(float angleDegress) {
        AimDirection aimDirection;
        if (angleDegress >= 22f && angleDegress <= 67f) {
            aimDirection = AimDirection.UpRight;
        }
        else if (angleDegress > 67f && angleDegress <= 112f)
        {
            aimDirection = AimDirection.Up;
        }
        else if (angleDegress > 112f && angleDegress <= 158f)
        {
            aimDirection = AimDirection.UpLeft;
        }
        else if ((angleDegress <= 180f && angleDegress > 158f) || (angleDegress >= -180f && angleDegress < -135f))
        {
            aimDirection = AimDirection.Left;
        }
        else if (angleDegress > -135f && angleDegress <= -45f)
        {
            aimDirection = AimDirection.Down;
        }
        else if ((angleDegress > -45f && angleDegress <= 0f) || (angleDegress > 0 && angleDegress < 22f))
        {
            aimDirection = AimDirection.Right;
        }
        else 
        {
            aimDirection = AimDirection.Right;
        }
        return aimDirection;
    }

    public static bool ValidateCheckEmptyString(Object thisObject, string fieldName, string stringToCheck) {
        if (stringToCheck == "") {
            Debug.Log(fieldName + " is empty and must contain a value in object" + thisObject.name.ToString());
            return true;
        }
        return false;
    }


    public static bool ValidateCheckEnumerableValues(Object thisObject, string fieldName, IEnumerable enumerableObjectToCheck) {
        bool error = false;
        int count = 0;

        if (enumerableObjectToCheck == null) {
            Debug.Log(fieldName + " is null in object" + thisObject.name.ToString());
            return true;
        }

        foreach (var item in enumerableObjectToCheck) {
            if (item == null) {
                Debug.Log(fieldName + " has null values in object" + thisObject.name.ToString());
                error = true;
            }
            else {
                count++;
            }
        }

        if (count == 0) {
            Debug.Log(fieldName + " has no values in object" + thisObject.name.ToString());
            error = true;

        }
        return error;
    }

    public static bool ValidateCheckNullValue(Object thisObject, string fieldName, Object objectToCheck) {
        if (objectToCheck == null) {
            Debug.Log(fieldName + " is null in object" + thisObject.name.ToString());
            return true;
        }
        return false;
    }

    public static bool ValidateCheckPositiveValue(Object thisObject, string fieldName, int valueToCheck, bool isZeroAllowed) {
        bool error = false;
        if (isZeroAllowed) {
            if (valueToCheck < 0) {
                Debug.Log(fieldName + " must contain a positive value or zero in object" + thisObject.name.ToString());
                error = true;
            }
        }
        else {
            if (valueToCheck <= 0) {
                Debug.Log(fieldName + " must contain a positive value in object" + thisObject.name.ToString());
                error = true;
            }
        }
        return error;
    }

    public static bool ValidateCheckPositiveValue(Object thisObject, string fieldName, float valueToCheck, bool isZeroAllowed) {
        bool error = false;
        if (isZeroAllowed) {
            if (valueToCheck < 0) {
                Debug.Log(fieldName + " must contain a positive value or zero in object" + thisObject.name.ToString());
                error = true;
            }
        }
        else {
            if (valueToCheck <= 0) {
                Debug.Log(fieldName + " must contain a positive value in object" + thisObject.name.ToString());
                error = true;
            }
        }
        return error;
    }

    public static bool ValidateCheckPositiveRange(Object thisObject, string fieldNameMinimum, float valueToCheckMinimum,
        string fieldNameMaximum, float valueToCheckMaximum, bool isZeroAllowed) {
        

        bool error = false;
        if (valueToCheckMinimum > valueToCheckMaximum) {
            Debug.Log(fieldNameMinimum + " must be less than " + fieldNameMaximum + " in object" + thisObject.name.ToString());
            error = true;
        }
        if (ValidateCheckPositiveValue(thisObject, fieldNameMinimum, valueToCheckMinimum, isZeroAllowed)) error = true;
        if (ValidateCheckPositiveValue(thisObject, fieldNameMaximum, valueToCheckMaximum, isZeroAllowed)) error = true;
        return error;
    }

    public static Vector3 GetSpawnPositionNearestToPlayer(Vector3 playerPosition)
    {
        Room currectRoom = GameManager.Instance.GetCurrentRoom();

        Grid grid = currectRoom.instantiateRoom.grid;

        Vector3 nearestSpawnPosition = new Vector3(10000f, 10000f, 0);

        foreach (Vector2Int spawnPositionGrid in currectRoom.spawnPositionArray)
        {
            Vector3 spawnPositionWorld = grid.CellToWorld((Vector3Int)spawnPositionGrid);
            if (Vector3.Distance(spawnPositionWorld, playerPosition) < Vector3.Distance(nearestSpawnPosition, playerPosition))
            {
                nearestSpawnPosition = spawnPositionWorld;
            }
        }

        return nearestSpawnPosition;
    }
    
    //sound
    public static float LinearToDicibels(int linear) {
        float linearScaleRange = 20f;
        return Mathf.Log10((float)linear / linearScaleRange) * 20f;
    }
}
