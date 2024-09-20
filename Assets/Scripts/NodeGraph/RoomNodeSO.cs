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

    public void Initialize(Rect rect, RoomNodeGraphSO nodeGraph, RoomNodeTypeSO roomNodeType)
    {
        this.rect = rect;
        this.id = Guid.NewGuid().ToString();
        this.name = "RoomNode";
        this.roomNodeGraph = nodeGraph;
        this.roomNodeType = roomNodeType;

        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    public void Draw(GUIStyle nodeStyle)
    {
        GUILayout.BeginArea(rect, nodeStyle);

        EditorGUI.BeginChangeCheck();

        if (parentRoomNodeIDList.Count > 0 || roomNodeType.isEntrance) {
            EditorGUILayout.LabelField(roomNodeType.roomNodeTypeName);
        }
        else {
            int selected = roomNodeTypeList.list.FindIndex(x => x == roomNodeType);
            int selection = EditorGUILayout.Popup("", selected, GetRoomNodeTypesToDisplay());

            roomNodeType = roomNodeTypeList.list[selection];

            if (roomNodeTypeList.list[selected].isCorridor && !roomNodeTypeList.list[selection].isCorridor ||
                !roomNodeTypeList.list[selected].isCorridor && roomNodeTypeList.list[selection].isCorridor ||
                !roomNodeTypeList.list[selected].isBossRoom && roomNodeTypeList.list[selection].isBossRoom) {
                if (childRoomNodeIDList.Count > 0) {
                    for (int i = childRoomNodeIDList.Count - 1; i>= 0; i--) {
                        RoomNodeSO childRoomNode = roomNodeGraph.GetRoomNode(childRoomNodeIDList[i]);

                        if (childRoomNode != null) {
                            RemoveChildRoomNodeIDFromRoomNode(childRoomNode.id);
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

    private void ProcessMouseDownEvent(Event currentEvent)
    {
        if (currentEvent.button == 0) {
            ProcessLeftClickDownEvent();
        }
        else if (currentEvent.button == 1) {
            ProcessRightClickDownEvent(currentEvent);
        }
    }

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

    private void ProcessMouseUpEvent(Event currentEvent)
    {
        if (currentEvent.button == 0) {
            ProcessLeftClickUpEvent();
        }
    }

    private void ProcessLeftClickUpEvent()
    {
        if (isLeftClickDragging) {
            isLeftClickDragging = false;
        }
    }

    private void ProcessMouseDragEvent(Event currentEvent)
    {
        if (currentEvent.button == 0) {
            ProcessLeftMouseDragEvent(currentEvent);
        }
    }

    private void ProcessLeftMouseDragEvent(Event currentEvent)
    {
        isLeftClickDragging = true;
        DragNode(currentEvent.delta);
        GUI.changed = true;
    }

    public void DragNode(Vector2 delta)
    {
        rect.position += delta;
        EditorUtility.SetDirty(this);
    }

    private void ProcessRightClickDownEvent(Event currentEvent)
    {
        roomNodeGraph.SetNodeToDrawConnetionLineFrom(this, currentEvent.mousePosition);
    }

    public bool AddChildRoomNodeIDToRoomNode(string childId) {

        if (IsChildRoomValid(childId)) {
            childRoomNodeIDList.Add(childId);
            return true;
        }
        return false;
    }

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

    public bool AddParentRoomNodeIDToRoomNode(string parentId) {
        
        parentRoomNodeIDList.Add(parentId);
        return true;
    }

    public bool RemoveChildRoomNodeIDFromRoomNode(string childId) {
        if (childRoomNodeIDList.Contains(childId)) {
            childRoomNodeIDList.Remove(childId);
            return true;
        }
        return false;
    }

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
