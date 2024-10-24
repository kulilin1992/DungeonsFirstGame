using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class RoomNodeSO : ScriptableObject
{

    //  public string id;
    //  public List<string> parentRoomNodeIDList = new List<string>();
    //  public List<string> childRoomNodeIDList = new List<string>();

     [HideInInspector] public string id;
     [HideInInspector] public List<string> parentRoomNodeIDList = new List<string>();
     [HideInInspector] public List<string> childRoomNodeIDList = new List<string>();
    [HideInInspector] public RoomNodeGraphSO roomNodeGraph;
    public RoomNodeTypeSO roomNodeType;
    [HideInInspector] public RoomNodeTypeListSO roomNodeTypeList;

    #region Editor Code
#if UNITY_EDITOR

    [HideInInspector] public Rect rect;
    [HideInInspector] public bool isLeftClickDragging = false;
    [HideInInspector] public bool isSelected = false;

    // 初始化节点
    public void Initialize(Rect rect, RoomNodeGraphSO nodeGraph, RoomNodeTypeSO roomNodeType)
    {
        this.rect = rect;
        this.id = Guid.NewGuid().ToString();
        this.name = "RoomNode";
        this.roomNodeGraph = nodeGraph;
        this.roomNodeType = roomNodeType;

        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    // 绘制节点
    public void Draw(GUIStyle nodeStyle)
    {
        GUILayout.BeginArea(rect, nodeStyle);

        EditorGUI.BeginChangeCheck();

        // 如果parentRoomNodeIDList的长度大于0或者roomNodeType是入口节点，则显示roomNodeType的名称
        if (parentRoomNodeIDList.Count > 0 || roomNodeType.isEntrance) {
            EditorGUILayout.LabelField(roomNodeType.roomNodeTypeName);
        }
        // 否则，显示roomNodeTypeList的列表，并获取选中的索引
        else {
            // 在roomNodeTypeList列表中查找与roomNodeType相等的元素的索引
            int selected = roomNodeTypeList.list.FindIndex(x => x == roomNodeType);
            // 在GetRoomNodeTypesToDisplay()方法中获取要显示的房间节点类型，并在编辑器中显示一个下拉列表，初始选择为selected
            int selection = EditorGUILayout.Popup("", selected, GetRoomNodeTypesToDisplay());

            // 将选中的roomNodeType赋值给roomNodeType
            roomNodeType = roomNodeTypeList.list[selection];

            // 如果选中的roomNodeType是走廊节点，而原来的roomNodeType不是走廊节点，或者选中的roomNodeType不是走廊节点，而原来的roomNodeType是走廊节点，或者选中的roomNodeType是boss房间，而原来的roomNodeType不是boss房间，则执行以下操作
            if (roomNodeTypeList.list[selected].isCorridor && !roomNodeTypeList.list[selection].isCorridor ||
                !roomNodeTypeList.list[selected].isCorridor && roomNodeTypeList.list[selection].isCorridor ||
                !roomNodeTypeList.list[selected].isBossRoom && roomNodeTypeList.list[selection].isBossRoom) {
                // 如果childRoomNodeIDList的长度大于0，则执行以下操作
                if (childRoomNodeIDList.Count > 0) {
                    // 从childRoomNodeIDList的最后一个元素开始循环
                    for (int i = childRoomNodeIDList.Count - 1; i>= 0; i--) {
                        // 获取childRoomNodeIDList中的元素对应的RoomNodeSO
                        RoomNodeSO childRoomNode = roomNodeGraph.GetRoomNode(childRoomNodeIDList[i]);

                        // 如果childRoomNode不为空，则执行以下操作
                        if (childRoomNode != null) {
                            // 从RoomNodeSO中移除childRoomNodeIDList中的元素
                            RemoveChildRoomNodeIDFromRoomNode(childRoomNode.id);
                            // 从RoomNodeSO中移除parentRoomNodeIDList中的元素
                            childRoomNode.RemoveParentRoomNodeIDFromRoomNode(id);
                        }
                    }

                }
            }
        }

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(this);    
        }

        GUILayout.EndArea();
    }

    // 获取可显示的节点类型
    public string[] GetRoomNodeTypesToDisplay()
    {
        string[] roomArray = new string[roomNodeTypeList.list.Count];

        for (int i = 0; i < roomNodeTypeList.list.Count; i++)
        {
            if (roomNodeTypeList.list[i].displayInNodeGraphEditor) {
                roomArray[i] = roomNodeTypeList.list[i].roomNodeTypeName;
            }
        }
        return roomArray;
    }

    // 处理事件
    public void ProcessEvents(Event currentEvent)
    {
        switch (currentEvent.type)
        {
            case EventType.MouseDown:
                ProcessMouseDownEvent(currentEvent);
                break;
            case EventType.MouseUp:
                ProcessMouseUpEvent(currentEvent);
                break;
            case EventType.MouseDrag:
                ProcessMouseDragEvent(currentEvent);
                break;
            default:
                break;
        }
    }

    // 处理鼠标按下事件
    private void ProcessMouseDownEvent(Event currentEvent)
    {
        if (currentEvent.button == 0) {
            ProcessLeftClickDownEvent();
        }
        else if (currentEvent.button == 1) {
            ProcessRightClickDownEvent(currentEvent);
        }
    }

    // 处理左键按下事件
    private void ProcessLeftClickDownEvent()
    {
        Selection.activeObject = this;
        //isSelected = !isSelected;
        if (isSelected) {
            isSelected = false;
        }
        else {
            isSelected = true;
        }
    }

    // 处理鼠标抬起事件
    private void ProcessMouseUpEvent(Event currentEvent)
    {
        if (currentEvent.button == 0) {
            ProcessLeftClickUpEvent();
        }
    }

    // 处理左键抬起事件
    private void ProcessLeftClickUpEvent()
    {
        if (isLeftClickDragging) {
            isLeftClickDragging = false;
        }
    }

    // 处理鼠标拖动事件
    private void ProcessMouseDragEvent(Event currentEvent)
    {
        if (currentEvent.button == 0) {
            ProcessLeftMouseDragEvent(currentEvent);
        }
    }

    // 处理左键拖动事件
    private void ProcessLeftMouseDragEvent(Event currentEvent)
    {
        isLeftClickDragging = true;
        DragNode(currentEvent.delta);
        GUI.changed = true;
    }

    // 拖动节点
    public void DragNode(Vector2 delta)
    {
        rect.position += delta;
        EditorUtility.SetDirty(this);
    }

    // 处理右键按下事件
    private void ProcessRightClickDownEvent(Event currentEvent)
    {
        roomNodeGraph.SetNodeToDrawConnetionLineFrom(this, currentEvent.mousePosition);
    }

    // 添加子节点ID
    public bool AddChildRoomNodeIDToRoomNode(string childId) {

        if (IsChildRoomValid(childId)) {
            childRoomNodeIDList.Add(childId);
            return true;
        }
        return false;
    }

    // 判断子节点是否有效
    public bool IsChildRoomValid(string childId) {
        bool isConnectedBossNodeAlready = false;
        foreach (RoomNodeSO roomNode in roomNodeGraph.roomNodeList) {
            if (roomNode.roomNodeType.isBossRoom && roomNode.parentRoomNodeIDList.Count > 0) {
                isConnectedBossNodeAlready = true;
            }
        }
        //if the child node is a boss node and there is already a boss node connected
        if (roomNodeGraph.GetRoomNode(childId).roomNodeType.isBossRoom && isConnectedBossNodeAlready)
            return false;
        
        //if the child node has a type of none
        if (roomNodeGraph.GetRoomNode(childId).roomNodeType.isNone)
            return false;

        //if the node already has a child with this child id
        if (childRoomNodeIDList.Contains(childId))
            return false;

        //if this node id and the child id are the same
        if (id == childId)
            return false;

        // if this childid is already in the parentid list
        if (parentRoomNodeIDList.Contains(childId))
            return false;
        
        //if the cild node already has a parent return false
        if (roomNodeGraph.GetRoomNode(childId).parentRoomNodeIDList.Count > 0)
            return false;

        //if the child node is a corridor and this node is a corridor
        if (roomNodeGraph.GetRoomNode(childId).roomNodeType.isCorridor && roomNodeType.isCorridor)
            return false;

        // if the child node is a corridor and this node is not a corridor
        if (!roomNodeGraph.GetRoomNode(childId).roomNodeType.isCorridor && !roomNodeType.isCorridor)
            return false;

        if (roomNodeGraph.GetRoomNode(childId).roomNodeType.isCorridor && childRoomNodeIDList.Count > Settings.maxChildCorridors)
            return false;
        if (roomNodeGraph.GetRoomNode(childId).roomNodeType.isEntrance)
            return false;
        
        //if adding a room to a corridor check that this corridor node doesn't already has a room add
        if (!roomNodeGraph.GetRoomNode(childId).roomNodeType.isCorridor && childRoomNodeIDList.Count > 0)
            return false;

        return true;
    }

    // 添加父节点ID
    public bool AddParentRoomNodeIDToRoomNode(string parentId) {
        
        parentRoomNodeIDList.Add(parentId);
        return true;
    }

    // 移除子节点ID
    public bool RemoveChildRoomNodeIDFromRoomNode(string childId) {
        if (childRoomNodeIDList.Contains(childId)) {
            childRoomNodeIDList.Remove(childId);
            return true;
        }
        return false;
    }

    // 移除父节点ID
    public bool RemoveParentRoomNodeIDFromRoomNode(string parentId) {
        if (parentRoomNodeIDList.Contains(parentId)) {
            parentRoomNodeIDList.Remove(parentId);
            return true;
        }
        return false;
    }

#endif
    #endregion Editor Code
}
