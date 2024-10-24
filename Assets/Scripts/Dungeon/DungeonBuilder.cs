using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

[DisallowMultipleComponent]
public class DungeonBuilder : SingletonMonobehaviour<DungeonBuilder>
{
    // 定义一个字典，用于存储房间
    public Dictionary<string, Room> dungeonBuilderRoomDictionary = new Dictionary<string, Room>();
    // 定义一个字典，用于存储房间模板
    private Dictionary<string, RoomTemplateSO> roomTemplateDictionary = new Dictionary<string, RoomTemplateSO>();
    // 定义一个列表，用于存储房间模板
    private List<RoomTemplateSO> roomTemplateList = null;
    // 定义一个房间节点类型列表
    private RoomNodeTypeListSO roomNodeTypeList;
    // 定义一个布尔值，用于判断是否成功构建了地牢
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
        // 将当前地牢级别的房间模板列表赋值给roomTemplateList
        roomTemplateList = currentDungeonLevel.roomTemplateList;
        // 将房间模板列表加载到字典中
        LoadRoomTemplatesIntoDictionary();

        // 地牢构建是否成功
        dungeonBuildSuccessful = false;
        // 地牢构建尝试次数
        int dungeonBuildAttempts = 0;

        // 当地牢构建不成功且尝试次数小于最大地牢构建尝试次数时，继续尝试构建地牢
        while (!dungeonBuildSuccessful && dungeonBuildAttempts < Settings.maxDungeonBuildAttempts)
        {
            dungeonBuildAttempts++;

            // 从当前地牢级别的房间节点图列表中随机选择一个房间节点图
            RoomNodeGraphSO roomNodeGraph = SelectRandomRoomNodeGraph(currentDungeonLevel.roomNodeGraphList);

            // 为房间节点图进行地牢重建尝试次数
            int dungeonRebuildAttemptsForNodeGraph = 0;
            // 地牢构建是否成功
            dungeonBuildSuccessful = false;

            // 当地牢构建不成功且地牢重建尝试次数小于最大地牢重建尝试次数时，继续尝试构建地牢
            while (!dungeonBuildSuccessful && dungeonRebuildAttemptsForNodeGraph <= Settings.maxDungeonRebuildAttemptsForRoomGraph)
            {
                // 清空地牢
                ClearDungeon();
                dungeonRebuildAttemptsForNodeGraph++;
                // 尝试构建随机地牢
                dungeonBuildSuccessful = AttemptToBuildRandomDungeon(roomNodeGraph);

            }
            // 如果地牢构建成功，则实例化房间对象
            if (dungeonBuildSuccessful) {
                InstantiateRoomObjects();
            }
        }
        // 返回地牢构建是否成功
        return dungeonBuildSuccessful;
    }

// 尝试构建随机地牢
    private bool AttemptToBuildRandomDungeon(RoomNodeGraphSO roomNodeGraph)
    {
        // 创建一个队列，用于存储待处理的房间节点
        Queue<RoomNodeSO> openRoomNodeQueue = new Queue<RoomNodeSO>();

        // 获取地牢图中的入口节点
        RoomNodeSO entranceNode = roomNodeGraph.GetRoomNode(roomNodeTypeList.list.Find(x => x.isEntrance));

        // 如果入口节点不为空，则将其加入队列
        if (entranceNode != null)
        {
            openRoomNodeQueue.Enqueue(entranceNode);
        }
        else
        {
            // 如果没有找到入口节点，则输出错误信息并返回false
            Debug.Log("No Entrance Node Found");
            return false;
        }
        // 标记是否没有房间重叠
        bool noRoomOverlaps = true;

        // 处理队列中的房间节点
        noRoomOverlaps = ProcessRoomsInOpenRoomNodeQueue(roomNodeGraph, openRoomNodeQueue, noRoomOverlaps);

        // 如果队列中的房间节点处理完毕且没有房间重叠，则返回true
        if (openRoomNodeQueue.Count == 0 && noRoomOverlaps) {
            return true;
        }
        else
        {
            // 否则返回false
            return false;
        }
    }

// 处理开放房间节点队列中的房间
    private bool ProcessRoomsInOpenRoomNodeQueue(RoomNodeGraphSO roomNodeGraph, Queue<RoomNodeSO> openRoomNodeQueue, bool noRoomOverlaps)
    {
        // 当开放房间节点队列不为空且没有房间重叠时，继续循环
        while (openRoomNodeQueue.Count > 0 && noRoomOverlaps == true)
        {
            // 从开放房间节点队列中取出一个房间节点
            RoomNodeSO roomNode = openRoomNodeQueue.Dequeue();

            // 遍历房间节点的子房间节点，将子房间节点加入开放房间节点队列
            foreach (RoomNodeSO childRoomNode in roomNodeGraph.GetChildRoomNodes(roomNode)) {
                openRoomNodeQueue.Enqueue(childRoomNode);
            }

            // 如果房间节点是入口节点
            if (roomNode.roomNodeType.isEntrance) {
                // 获取随机房间模板
                RoomTemplateSO roomTemplate = GetRandomRoomTemplate(roomNode.roomNodeType);
                // 根据房间模板创建房间
                Room room = CreateRoomFromRoomTemplate(roomTemplate, roomNode);

                // 设置房间已定位
                room.isPositioned = true;
                // 将房间加入 dungeonBuilderRoomDictionary 字典中
                dungeonBuilderRoomDictionary.Add(room.id, room);
            }
            else
            {
                // 获取父房间
                Room parentRoom = dungeonBuilderRoomDictionary[roomNode.parentRoomNodeIDList[0]];
                // 判断房间是否可以放置且没有重叠
                noRoomOverlaps = CanPlaceRoomWithNoOverLaps(roomNode, parentRoom);
            }
        }
        // 返回是否有房间重叠
        return noRoomOverlaps;
    }

    private bool CanPlaceRoomWithNoOverLaps(RoomNodeSO roomNode, Room parentRoom)
    {
// 定义一个布尔变量，表示房间是否重叠
        bool roomOverLaps = true;

// 当房间重叠时，执行以下循环
        while (roomOverLaps) {
    // 获取父房间中未连接的可用门道
            List<Doorway> unconnectedAvailableParentDoorways = GetUnconnectedAvailableDoorways(parentRoom.doorWayList).ToList();

    // 如果父房间中没有未连接的可用门道，则返回false
            if (unconnectedAvailableParentDoorways.Count == 0) {
                return false;
            }

    // 从未连接的可用门道中随机选择一个门道
            Doorway parentDoorway = unconnectedAvailableParentDoorways[UnityEngine.Random.Range(0, unconnectedAvailableParentDoorways.Count)];

    // 根据父房间和门道，获取一个随机的房间模板
            RoomTemplateSO roomTemplate = GetRandomTemplateForRoomConsistentWithParent(roomNode, parentDoorway);

    // 根据房间模板创建一个房间
            Room room = CreateRoomFromRoomTemplate(roomTemplate, roomNode);

    // 将房间放置在父房间中
            if (PlaceTheRoom(parentRoom, parentDoorway, room)) {
        // 如果房间放置成功，则将房间标记为已放置，并将房间添加到字典中
                roomOverLaps = false;

                room.isPositioned = true;

                dungeonBuilderRoomDictionary.Add(room.id, room);
            }
            else {
        // 如果房间放置失败，则将房间重叠标记为true，继续循环
                roomOverLaps = true;
            }
        }
        return true; // no room overlaps
    }

    private void LoadRoomTemplatesIntoDictionary()
    {
        // 清空roomTemplateDictionary
        roomTemplateDictionary.Clear();

        // 遍历roomTemplateList中的每一个RoomTemplateSO
        foreach (RoomTemplateSO roomTemplate in roomTemplateList)
        {
            // 如果roomTemplateDictionary中不包含roomTemplate的guid
            if (!roomTemplateDictionary.ContainsKey(roomTemplate.guid))
            {
                // 将roomTemplate添加到roomTemplateDictionary中
                roomTemplateDictionary.Add(roomTemplate.guid, roomTemplate);
            }
            else
            {
                // 如果roomTemplateDictionary中已经包含roomTemplate的guid，则输出错误信息
                Debug.Log("Duplicate Room Template Key in Dictionary: " + roomTemplateList);
            }
        }
    }

    private RoomNodeGraphSO SelectRandomRoomNodeGraph(List<RoomNodeGraphSO> roomNodeGraphList)
    {
        // 如果roomNodeGraphList不为空
        if (roomNodeGraphList.Count > 0) {
            // 从roomNodeGraphList中随机选择一个RoomNodeGraphSO并返回
            return roomNodeGraphList[UnityEngine.Random.Range(0, roomNodeGraphList.Count)];
        }
        else
        {
            // 如果roomNodeGraphList为空，则输出错误信息并返回null
            Debug.Log("No Room Node Graphs in List");
            return null;
        }
    }

    private void ClearDungeon()
    {
        // 如果dungeonBuilderRoomDictionary不为空
        if (dungeonBuilderRoomDictionary.Count > 0) {
            // 遍历dungeonBuilderRoomDictionary中的每一个KeyValuePair
            foreach (KeyValuePair<string, Room> keyValuePair in dungeonBuilderRoomDictionary) {
                // 获取KeyValuePair中的Room对象
                Room room = keyValuePair.Value;
                // 如果Room对象的instantiateRoom不为空
                if (room.instantiateRoom != null) {
                    // 销毁Room对象的instantiateRoom
                    Destroy(room.instantiateRoom.gameObject);
                }
            }
            // 清空dungeonBuilderRoomDictionary
            dungeonBuilderRoomDictionary.Clear();
        }
    }

    private RoomTemplateSO GetRandomRoomTemplate(RoomNodeTypeSO roomNodeType)
    {
        // 创建一个空的RoomTemplateSO列表
        List<RoomTemplateSO> matchingRoomTemplateList = new List<RoomTemplateSO>();

        // 遍历roomTemplateList中的每一个RoomTemplateSO
        foreach (RoomTemplateSO roomTemplate in roomTemplateList) {
        // 如果roomTemplate的roomNodeType与传入的roomNodeType相等
            if (roomTemplate.roomNodeType == roomNodeType) {
        // 将roomTemplate添加到matchingRoomTemplateList中
                matchingRoomTemplateList.Add(roomTemplate);
            }
        }

        // 如果matchingRoomTemplateList为空
        if (matchingRoomTemplateList.Count == 0) {
        // 返回null
            return null;
        }

        // 从matchingRoomTemplateList中随机选择一个RoomTemplateSO并返回
        return matchingRoomTemplateList[UnityEngine.Random.Range(0, matchingRoomTemplateList.Count)];
    }
    

    private Room CreateRoomFromRoomTemplate(RoomTemplateSO roomTemplate, RoomNodeSO roomNode)
    {
        // 创建一个Room对象
        Room room = new Room();

        // 设置Room对象的属性
        room.templateId = roomTemplate.guid;
        room.id = roomNode.id;
        room.prefab = roomTemplate.prefab;
        room.roomNodeType = roomTemplate.roomNodeType;
        room.lowerBounds = roomTemplate.lowerBounds;
        room.upperBounds = roomTemplate.upperBounds;
        room.spawnPositionArray = roomTemplate.spawnPositionArray;
        room.templateLowerBounds = roomTemplate.lowerBounds;
        room.templateUpperBounds = roomTemplate.upperBounds;

        // 复制子房间ID列表和门列表
        room.childRoomIdList = CopyStringList(roomNode.childRoomNodeIDList);
        room.doorWayList = CopyDoorWayList(roomTemplate.doorwayList);

        // 复制敌人列表和敌人生成参数列表
        room.enemiesByLevelList = roomTemplate.enemiesByLevelList;
        room.roomLevelEnemySpawnParamatersList = roomTemplate.roomEnemySpawnParamaterList;

        //game music
        room.battleMusic = roomTemplate.battleMusic;
        room.ambientMusic = roomTemplate.ambientMusic;

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

        if (room.GetNumberOfEnemiesToSpawn(GameManager.Instance.GetCurrentDungeonLevel()) == 0) {
            room.isClearedOfEnemies = true;
        }

        return room;

    }

    private IEnumerable<Doorway> GetUnconnectedAvailableDoorways(List<Doorway> doorWayList)
    {
        // 遍历doorWayList中的每一个Doorway
        foreach (Doorway doorway in doorWayList) {
            // 如果Doorway对象未连接且可用
            if (!doorway.isConnected && !doorway.isUnavailable) {
                // 返回Doorway对象
                yield return doorway;
            }
        }
    }

    private RoomTemplateSO GetRandomTemplateForRoomConsistentWithParent(RoomNodeSO roomNode, Doorway parentDoorway)
    {
        RoomTemplateSO roomTemplate = null;
        // 如果roomNode的roomNodeType是走廊
        if (roomNode.roomNodeType.isCorridor) {
            // 根据parentDoorway的方向选择一个RoomTemplateSO
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
            // 如果roomNode的roomNodeType不是走廊，则选择一个与roomNode的roomNodeType相匹配的RoomTemplateSO
            roomTemplate = GetRandomRoomTemplate(roomNode.roomNodeType);
        }
        return roomTemplate;
    }

    private List<Doorway> CopyDoorWayList(List<Doorway> oldDoorwayList)
    {
        // 创建一个Doorway列表
        List<Doorway> copiedDoorwayList = new List<Doorway>();
        // 遍历oldDoorwayList中的每一个Doorway
        foreach (Doorway oldDoorway in oldDoorwayList)
        {
            // 创建一个新的Doorway对象
            Doorway newDoorway = new Doorway();
            // 设置新Doorway对象的属性
            newDoorway.position = oldDoorway.position;
            newDoorway.orientation = oldDoorway.orientation;
            newDoorway.doorPrefab = oldDoorway.doorPrefab;
            newDoorway.isConnected = oldDoorway.isConnected;
            newDoorway.isUnavailable = oldDoorway.isUnavailable;
            newDoorway.doorwayStartCopyPosition = oldDoorway.doorwayStartCopyPosition;
            newDoorway.doorwayCopyTileWidth = oldDoorway.doorwayCopyTileWidth;
            newDoorway.doorwayCopyTileHeight = oldDoorway.doorwayCopyTileHeight;
            // 将新Doorway对象添加到copiedDoorwayList中
            copiedDoorwayList.Add(newDoorway);
        }
        // 返回copiedDoorwayList
        return copiedDoorwayList;
    }

    private List<string> CopyStringList(List<string> oldStringList)
    {
        // 创建一个字符串列表
        List<string> copiedList = new List<string>();
        // 遍历oldStringList中的每一个字符串
        foreach (string oldStringValue in oldStringList)
        {
            // 将字符串添加到copiedList中
            copiedList.Add(oldStringValue);
        }
        // 返回copiedList
        return copiedList;
    }


    // if the room doesn't overlap, return true
    private bool PlaceTheRoom(Room parentRoom, Doorway parentDoorway, Room room)
    {
        // 获取room中与parentDoorway方向相反的Doorway
        Doorway doorway = GetOppositeDoorway(parentDoorway, room.doorWayList);

        // 如果doorway为空
        if (doorway == null) {
            // 将parentDoorway设置为不可用
            parentDoorway.isUnavailable = true;
            // 返回false
            return false;
        }

        //calcaulate world grid parent doorway position
        // 计算parentDoorway在世界网格中的位置
        Vector2Int parentDoorwayPosition = parentRoom.lowerBounds + parentDoorway.position - parentRoom.templateLowerBounds;
    
        // 根据doorway的方向计算调整值
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

        // 设置room的lowerBounds和upperBounds
        room.lowerBounds = parentDoorwayPosition + adjustment + room.templateLowerBounds - doorway.position;
        room.upperBounds = room.lowerBounds + room.templateUpperBounds - room.templateLowerBounds;

        // 检查room是否与其他room重叠
        Room overlappingRoom = CheckForRoomOverlap(room);

        // 如果room不重叠
        if (overlappingRoom == null) {
            // 将parentDoorway和doorway设置为已连接
            parentDoorway.isConnected = true;
            parentDoorway.isUnavailable = true;
            doorway.isConnected = true;
            doorway.isUnavailable = true;
            // 返回true
            return true;
        }
        else {
            // 如果room重叠，将parentDoorway设置为不可用
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
