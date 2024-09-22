using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;


[CreateAssetMenu(fileName = "RoomNodeGraph", menuName = "Scriptable Objects/Dungeon/Room Node Graph")]
public class RoomNodeGraphSO : ScriptableObject
{
    [HideInInspector] public RoomNodeTypeListSO roomNodeTypeList;
    [HideInInspector] public List<RoomNodeSO> roomNodeList = new List<RoomNodeSO>();
    [HideInInspector] public Dictionary<string, RoomNodeSO> roomNodeDictionary = new Dictionary<string, RoomNodeSO>();


    private void Awake()
    {
        LoadRoomNodeDictionary();
    }

    private void LoadRoomNodeDictionary()
    {
        roomNodeDictionary.Clear();

        foreach (RoomNodeSO roomNode in roomNodeList)
        {
            roomNodeDictionary[roomNode.id] = roomNode;
        }
    }

    public RoomNodeSO GetRoomNode(RoomNodeTypeSO roomNodeType){
        foreach (RoomNodeSO roomNode in roomNodeList)
        {
            if (roomNode.roomNodeType == roomNodeType)
            {
                return roomNode;
            }
        }
        return null;
    }

    public RoomNodeSO GetRoomNode(string roomNodeId) {
        if (roomNodeDictionary.TryGetValue(roomNodeId, out RoomNodeSO roomNode))
        {
            return roomNode;
        }
        //Debug.LogError("Room Node not found in dictionary!");
        return null;
    }

    public IEnumerable<RoomNodeSO> GetChildRoomNodes(RoomNodeSO parentRoomNode)
    {
        foreach (string childNodeId in parentRoomNode.childRoomNodeIDList) {
            yield return GetRoomNode(childNodeId);
        }
    }

    #region Editor Code

#if UNITY_EDITOR

    [HideInInspector] public RoomNodeSO roomNodeToDrawLineFrom = null;
    [HideInInspector] public Vector2 linePosition;

    public void OnValidate()
    {
        LoadRoomNodeDictionary();
    }

    public void SetNodeToDrawConnetionLineFrom(RoomNodeSO node, Vector2 linePos)
    {
        roomNodeToDrawLineFrom = node;
        linePosition = linePos;
    }

#endif
    #endregion Editor Code
}
