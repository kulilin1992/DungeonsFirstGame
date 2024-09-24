using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

[DisallowMultipleComponent]
public class DungeonBuilder : SingletonMonobehaviour<DungeonBuilder>
{
    public Dictionary<string, Room> dungeonBuilderRoomDictionary = new Dictionary<string, Room>();
    private Dictionary<string, RoomTemplateSO> roomTemplateDictionary = new Dictionary<string, RoomTemplateSO>();
    private List<RoomTemplateSO> roomTemplateList = null;
    private RoomNodeTypeListSO roomNodeTypeList;
    private bool dungeonBuildSuccessful;

    protected override void Awake()
    {
        base.Awake();

        LoadRoomNodeTypeList();

        //GameResources.Instance.dimmedMaterial.SetFloat("Alpha_Slider", 1f);
    }

    private void OnEnable()
    {
        GameResources.Instance.dimmedMaterial.SetFloat("Alpha_Slider", 0f);
    }

    private void OnDisable()
    {
        GameResources.Instance.dimmedMaterial.SetFloat("Alpha_Slider", 1f);
    }

    private void LoadRoomNodeTypeList()
    {
        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    public bool GenerateDungeon(DungeonLevelSO currentDungeonLevel) {
        roomTemplateList = currentDungeonLevel.roomTemplateList;
        LoadRoomTemplatesIntoDictionary();

        dungeonBuildSuccessful = false;
        int dungeonBuildAttempts = 0;

        while (!dungeonBuildSuccessful && dungeonBuildAttempts < Settings.maxDungeonBuildAttempts)
        {
            dungeonBuildAttempts++;

            RoomNodeGraphSO roomNodeGraph = SelectRandomRoomNodeGraph(currentDungeonLevel.roomNodeGraphList);

            int dungeonRebuildAttemptsForNodeGraph = 0;
            dungeonBuildSuccessful = false;

            while (!dungeonBuildSuccessful && dungeonRebuildAttemptsForNodeGraph <= Settings.maxDungeonRebuildAttemptsForRoomGraph)
            {
                ClearDungeon();
                dungeonRebuildAttemptsForNodeGraph++;
                dungeonBuildSuccessful = AttemptToBuildRandomDungeon(roomNodeGraph);

            }
            if (dungeonBuildSuccessful) {
                InstantiateRoomObjects();
            }
        }
        return dungeonBuildSuccessful;
    }

    private bool AttemptToBuildRandomDungeon(RoomNodeGraphSO roomNodeGraph)
    {
        Queue<RoomNodeSO> openRoomNodeQueue = new Queue<RoomNodeSO>();

        RoomNodeSO entranceNode = roomNodeGraph.GetRoomNode(roomNodeTypeList.list.Find(x => x.isEntrance));

        if (entranceNode != null)
        {
            openRoomNodeQueue.Enqueue(entranceNode);
        }
        else
        {
            Debug.Log("No Entrance Node Found");
            return false;
        }
        bool noRoomOverlaps = true;

        noRoomOverlaps = ProcessRoomsInOpenRoomNodeQueue(roomNodeGraph, openRoomNodeQueue, noRoomOverlaps);

        if (openRoomNodeQueue.Count == 0 && noRoomOverlaps) {
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool ProcessRoomsInOpenRoomNodeQueue(RoomNodeGraphSO roomNodeGraph, Queue<RoomNodeSO> openRoomNodeQueue, bool noRoomOverlaps)
    {
        while (openRoomNodeQueue.Count > 0 && noRoomOverlaps == true)
        {
            RoomNodeSO roomNode = openRoomNodeQueue.Dequeue();

            foreach (RoomNodeSO childRoomNode in roomNodeGraph.GetChildRoomNodes(roomNode)) {
                openRoomNodeQueue.Enqueue(childRoomNode);
            }

            if (roomNode.roomNodeType.isEntrance) {
                RoomTemplateSO roomTemplate = GetRandomRoomTemplate(roomNode.roomNodeType);
                Room room = CreateRoomFromRoomTemplate(roomTemplate, roomNode);

                room.isPositioned = true;
                dungeonBuilderRoomDictionary.Add(room.id, room);
            }
            else
            {
                Room parentRoom = dungeonBuilderRoomDictionary[roomNode.parentRoomNodeIDList[0]];
                noRoomOverlaps = CanPlaceRoomWithNoOverLaps(roomNode, parentRoom);
            }
        }
        return noRoomOverlaps;
    }

    private bool CanPlaceRoomWithNoOverLaps(RoomNodeSO roomNode, Room parentRoom)
    {
        bool roomOverLaps = true;

        while (roomOverLaps) {
            List<Doorway> unconnectedAvailableParentDoorways = GetUnconnectedAvailableDoorways(parentRoom.doorWayList).ToList();

            if (unconnectedAvailableParentDoorways.Count == 0) {
                return false;
            }

            Doorway parentDoorway = unconnectedAvailableParentDoorways[UnityEngine.Random.Range(0, unconnectedAvailableParentDoorways.Count)];

            RoomTemplateSO roomTemplate = GetRandomTemplateForRoomConsistentWithParent(roomNode, parentDoorway);

            Room room = CreateRoomFromRoomTemplate(roomTemplate, roomNode);

            if (PlaceTheRoom(parentRoom, parentDoorway, room)) {
                roomOverLaps = false;

                room.isPositioned = true;

                dungeonBuilderRoomDictionary.Add(room.id, room);
            }
            else {
                roomOverLaps = true;
            }
        }
        return true; // no room overlaps
    }

    private void LoadRoomTemplatesIntoDictionary()
    {
        roomTemplateDictionary.Clear();

        foreach (RoomTemplateSO roomTemplate in roomTemplateList)
        {
            if (!roomTemplateDictionary.ContainsKey(roomTemplate.guid))
            {
                roomTemplateDictionary.Add(roomTemplate.guid, roomTemplate);
            }
            else
            {
                Debug.Log("Duplicate Room Template Key in Dictionary: " + roomTemplateList);
            }
        }
    }

    private RoomNodeGraphSO SelectRandomRoomNodeGraph(List<RoomNodeGraphSO> roomNodeGraphList)
    {
        if (roomNodeGraphList.Count > 0) {
            return roomNodeGraphList[UnityEngine.Random.Range(0, roomNodeGraphList.Count)];
        }
        else
        {
            Debug.Log("No Room Node Graphs in List");
            return null;
        }
    }

    private void ClearDungeon()
    {
        if (dungeonBuilderRoomDictionary.Count > 0) {
            foreach (KeyValuePair<string, Room> keyValuePair in dungeonBuilderRoomDictionary) {
                Room room = keyValuePair.Value;
                if (room.instantiateRoom != null) {
                    Destroy(room.instantiateRoom.gameObject);
                }
            }
            dungeonBuilderRoomDictionary.Clear();
        }
    }

    private RoomTemplateSO GetRandomRoomTemplate(RoomNodeTypeSO roomNodeType)
    {
        List<RoomTemplateSO> matchingRoomTemplateList = new List<RoomTemplateSO>();

        foreach (RoomTemplateSO roomTemplate in roomTemplateList) {
            if (roomTemplate.roomNodeType == roomNodeType) {
                matchingRoomTemplateList.Add(roomTemplate);
            }
        }

        if (matchingRoomTemplateList.Count == 0) {
            return null;
        }

        return matchingRoomTemplateList[UnityEngine.Random.Range(0, matchingRoomTemplateList.Count)];
    }
    

    private Room CreateRoomFromRoomTemplate(RoomTemplateSO roomTemplate, RoomNodeSO roomNode)
    {
        Room room = new Room();

        room.templateId = roomTemplate.guid;
        room.id = roomNode.id;
        room.prefab = roomTemplate.prefab;
        room.roomNodeType = roomTemplate.roomNodeType;
        room.lowerBounds = roomTemplate.lowerBounds;
        room.upperBounds = roomTemplate.upperBounds;
        room.spawnPositionArray = roomTemplate.spawnPositionArray;
        room.templateLowerBounds = roomTemplate.lowerBounds;
        room.templateUpperBounds = roomTemplate.upperBounds;

        room.childRoomIdList = CopyStringList(roomNode.childRoomNodeIDList);
        room.doorWayList = CopyDoorWayList(roomTemplate.doorwayList);

        //Entrance
        if (roomNode.parentRoomNodeIDList.Count == 0) {
            room.parentRoomId = "";
            room.isPreviouslyVisited = true;

            GameManager.Instance.SetCurrentRoom(room);
        }
        else
        {
            room.parentRoomId = roomNode.parentRoomNodeIDList[0];
        }

        return room;

    }

    private IEnumerable<Doorway> GetUnconnectedAvailableDoorways(List<Doorway> doorWayList)
    {
        foreach (Doorway doorway in doorWayList) {
            if (!doorway.isConnected && !doorway.isUnavailable) {
                yield return doorway;
            }
        }
    }

    private RoomTemplateSO GetRandomTemplateForRoomConsistentWithParent(RoomNodeSO roomNode, Doorway parentDoorway)
    {
        RoomTemplateSO roomTemplate = null;
        if (roomNode.roomNodeType.isCorridor) {
            switch (parentDoorway.orientation) {
                case Orientation.north:
                case Orientation.south:
                    roomTemplate = GetRandomRoomTemplate(roomNodeTypeList.list.Find(x => x.isCorridorNS));
                    break;
                case Orientation.east:
                case Orientation.west:
                    roomTemplate = GetRandomRoomTemplate(roomNodeTypeList.list.Find(x => x.isCorridorEW));
                    break;
                case Orientation.none:
                    break;
                default:
                    break;
            }
        }
        else
        {
            roomTemplate = GetRandomRoomTemplate(roomNode.roomNodeType);
        }
        return roomTemplate;
    }

    private List<Doorway> CopyDoorWayList(List<Doorway> oldDoorwayList)
    {
        List<Doorway> copiedDoorwayList = new List<Doorway>();
        foreach (Doorway oldDoorway in oldDoorwayList)
        {
            Doorway newDoorway = new Doorway();
            newDoorway.position = oldDoorway.position;
            newDoorway.orientation = oldDoorway.orientation;
            newDoorway.doorPrefab = oldDoorway.doorPrefab;
            newDoorway.isConnected = oldDoorway.isConnected;
            newDoorway.isUnavailable = oldDoorway.isUnavailable;
            newDoorway.doorwayStartCopyPosition = oldDoorway.doorwayStartCopyPosition;
            newDoorway.doorwayCopyTileWidth = oldDoorway.doorwayCopyTileWidth;
            newDoorway.doorwayCopyTileHeight = oldDoorway.doorwayCopyTileHeight;
            copiedDoorwayList.Add(newDoorway);
        }
        return copiedDoorwayList;
    }

    private List<string> CopyStringList(List<string> oldStringList)
    {
        List<string> copiedList = new List<string>();
        foreach (string oldStringValue in oldStringList)
        {
            copiedList.Add(oldStringValue);
        }
        return copiedList;
    }


    // if the room doesn't overlap, return true
    private bool PlaceTheRoom(Room parentRoom, Doorway parentDoorway, Room room)
    {
        Doorway doorway = GetOppositeDoorway(parentDoorway, room.doorWayList);

        if (doorway == null) {
            parentDoorway.isUnavailable = true;
            return false;
        }

        //calcaulate world grid parent doorway position
        Vector2Int parentDoorwayPosition = parentRoom.lowerBounds + parentDoorway.position - parentRoom.templateLowerBounds;
    
        Vector2Int adjustment = Vector2Int.zero;

        switch (doorway.orientation) {
            case Orientation.north:
               adjustment = new Vector2Int(0, -1);
               break;
            case Orientation.south:
                adjustment = new Vector2Int(0, 1);
                break;
            case Orientation.east:
                adjustment = new Vector2Int(-1, 0);
                break;
            case Orientation.west:
                adjustment = new Vector2Int(1, 0);
                break;
            case Orientation.none:
                break;
            default:
                break;
        }

        room.lowerBounds = parentDoorwayPosition + adjustment + room.templateLowerBounds - doorway.position;
        room.upperBounds = room.lowerBounds + room.templateUpperBounds - room.templateLowerBounds;

        Room overlappingRoom = CheckForRoomOverlap(room);

        if (overlappingRoom == null) {
            parentDoorway.isConnected = true;
            parentDoorway.isUnavailable = true;
            doorway.isConnected = true;
            doorway.isUnavailable = true;
            return true;
        }
        else {
            parentDoorway.isUnavailable = true;
            return false;
        }
    }

    private Doorway GetOppositeDoorway(Doorway parentDoorway, List<Doorway> doorWayList)
    {
        foreach (Doorway doorwayToCheck in doorWayList) {
            if (parentDoorway.orientation == Orientation.east && doorwayToCheck.orientation == Orientation.west) {
                return doorwayToCheck;
            } else if (parentDoorway.orientation == Orientation.west && doorwayToCheck.orientation == Orientation.east) {
                return doorwayToCheck;
            } else if (parentDoorway.orientation == Orientation.north && doorwayToCheck.orientation == Orientation.south) {
                return doorwayToCheck;
            } else if (parentDoorway.orientation == Orientation.south && doorwayToCheck.orientation == Orientation.north) {
                return doorwayToCheck;
            }
        }
        return null;
    }

    private Room CheckForRoomOverlap(Room roomParam)
    {
        foreach (KeyValuePair<string, Room> keyValuePair in dungeonBuilderRoomDictionary) {
            Room room = keyValuePair.Value;
            if (room.id == roomParam.id || !room.isPositioned) {
                continue;
            }

            if (IsOverLappingRoom(roomParam, room)) {
                return room;
            }
        }

        return null;
    }

    private bool IsOverLappingRoom(Room roomParam, Room room)
    {
        bool isOverLappingX = IsOverLappingInterval(roomParam.lowerBounds.x, roomParam.upperBounds.x, room.lowerBounds.x, room.upperBounds.x);
        bool isOverLappingY = IsOverLappingInterval(roomParam.lowerBounds.y, roomParam.upperBounds.y, room.lowerBounds.y, room.upperBounds.y);
        if (isOverLappingX && isOverLappingY) {
            return true;
        }
        else {
            return false;
        }
    }

    private bool IsOverLappingInterval(int imin1, int imax1, int imin2, int imax2)
    {
        if (Mathf.Max(imin1, imin2) <= Mathf.Min(imax1, imax2)) {
            return true;
        }
        else {
            return false;
        }
    }

    private void InstantiateRoomObjects() {
        foreach (KeyValuePair<string, Room> keyValuePair in dungeonBuilderRoomDictionary) {
            Room room = keyValuePair.Value;

            Vector3 roomPosition = new Vector3(room.lowerBounds.x - room.templateLowerBounds.x, 
                room.lowerBounds.y - room.templateLowerBounds.y, 0f);
            
            GameObject roomObject = Instantiate(room.prefab, roomPosition, Quaternion.identity, transform);
            
            InstantiateRoom instantiateRoom = roomObject.GetComponentInChildren<InstantiateRoom>();
            instantiateRoom.room = room;

            instantiateRoom.Initialise(roomObject);

            //save gameobject reference
            room.instantiateRoom = instantiateRoom;
        }
    }

    public RoomTemplateSO GetRoomTemplate(string roomTemplateId)
    {
        if (roomTemplateDictionary.TryGetValue(roomTemplateId, out RoomTemplateSO roomTemplate)) {
            return roomTemplate;
        }
        else {
            return null;

        }
    }

    public Room GetRoomByRoomId(string roomId)
    {
        if (dungeonBuilderRoomDictionary.TryGetValue(roomId, out Room room)) {
            return room;
        }
        else {
            return null;
        }
    }
}
