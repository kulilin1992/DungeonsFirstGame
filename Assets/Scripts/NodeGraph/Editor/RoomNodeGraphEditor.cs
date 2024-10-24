using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public class RoomNodeGraphEditor : EditorWindow
{

    //定义房间节点样式
    private GUIStyle roomNodeStyle;
    //定义房间节点选中样式
    private GUIStyle roomNodeSelectedStyle;
    //定义当前房间节点图
    private static RoomNodeGraphSO currentRoomNodeGraph;

    //定义房间节点图偏移量
    private Vector2 graphOffset;
    //定义房间节点图拖动量
    private Vector2 graphDrag;
    //定义房间节点类型列表
    private RoomNodeTypeListSO roomNodeTypeList;

    //定义当前房间节点
    private RoomNodeSO currentRoomNode = null;

    //Node layout values
    private const float nodeWidth = 160f;
    private const float nodeHeight = 75f;
    private const int nodePadding = 25;
    private const int nodeBorder = 12;

    private const float connectingLineWidth = 3f;
    private const float connectingLineArrowSize = 6f;


    //grid spacing
    private const float gridLarge = 100f;
    private const float gridSmall = 25f;

    [MenuItem("Room Node Graph Editor", menuItem = "Window/Dungeon Editor/Room Node Graph Editor")]
    public static void OpenWindow()
    {
        GetWindow<RoomNodeGraphEditor>("Room Node Graph Editor");
    }

    private void OnEnable() {

        // 添加Selection.selectionChanged事件监听器
        Selection.selectionChanged += InspectorSelectionChanged;
        //aa
        // 创建roomNodeStyle样式
        roomNodeStyle = new GUIStyle();
        // 设置roomNodeStyle的背景图片
        roomNodeStyle.normal.background = EditorGUIUtility.Load("node1") as Texture2D;
        // 设置roomNodeStyle的文字颜色
        roomNodeStyle.normal.textColor = Color.white;
        // 设置roomNodeStyle的内边距
        roomNodeStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
        // 设置roomNodeStyle的边框
        roomNodeStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);
    
        // 创建roomNodeSelectedStyle样式
        roomNodeSelectedStyle = new GUIStyle();
        // 设置roomNodeSelectedStyle的背景图片
        roomNodeSelectedStyle.normal.background = EditorGUIUtility.Load("node1 on") as Texture2D;
        // 设置roomNodeSelectedStyle的文字颜色
        roomNodeSelectedStyle.normal.textColor = Color.white;
        // 设置roomNodeSelectedStyle的内边距
        roomNodeSelectedStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
        // 设置roomNodeSelectedStyle的边框
        roomNodeSelectedStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);
    
        // 获取GameResources中的roomNodeTypeList
        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    private void OnDisable() {
        Selection.selectionChanged -= InspectorSelectionChanged;
    }

    [OnOpenAsset(0)]
    public static bool OnDoubleClickAsset(int instanceID, int line) {
        // 根据instanceID获取RoomNodeGraphSO对象
        RoomNodeGraphSO roomNodeGraph = EditorUtility.InstanceIDToObject(instanceID) as RoomNodeGraphSO;
        // 如果获取成功
        if (roomNodeGraph != null) {
            // 打开窗口
            OpenWindow();
            // 将当前RoomNodeGraphSO对象赋值给currentRoomNodeGraph
            currentRoomNodeGraph = roomNodeGraph;
            return true;
        }
        return false;
    }

    private void OnGUI() {
        //Debug.Log("OnGUI has been called");
        // GUILayout.BeginArea(new Rect(new Vector2(100f, 100f), new Vector2(nodeWidth, nodeHeight)), roomNodeStyle);
        // EditorGUILayout.LabelField("Node 1");
        // GUILayout.EndArea();

        // GUILayout.BeginArea(new Rect(new Vector2(300f, 300f), new Vector2(nodeWidth, nodeHeight)), roomNodeStyle);
        // EditorGUILayout.LabelField("Node 2");
        // GUILayout.EndArea();

        if (currentRoomNodeGraph != null) {

            //draw grid
            DrawBackgroundGrid(gridSmall, 0.2f, Color.gray);
            DrawBackgroundGrid(gridLarge, 0.3f, Color.gray);

            DrawDraggedLine();

            ProcessEvents(Event.current);

            DrawRoomConnections();

            DrawRoomNodes();
        }

        if (GUI.changed) {
            Repaint();
        }
    }

    private void DrawBackgroundGrid(float gridSize, float gridOpacity, Color gridColor) {
        // 计算垂直线数量
        int verticalLineCount = Mathf.CeilToInt((position.width + gridSize) / gridSize);   //垂直
        // 计算水平线数量
        int horizontalLineCount = Mathf.CeilToInt((position.height + gridSize) / gridSize); // 水平

        // 设置网格颜色和透明度
        Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);
        // 更新网格偏移量
        graphOffset += graphDrag * 0.5f;

        // 计算网格偏移量
        Vector3 gridOffset = new Vector3(graphOffset.x % gridSize, graphOffset.y % gridSize, 0);

        // 绘制垂直线
        for (int i = 0; i < verticalLineCount; i++) {
            Handles.DrawLine(new Vector3(gridSize * i, -gridSize, 0) + gridOffset, new Vector3(gridSize * i, position.height + gridSize, 0f) +
                gridOffset);
        }

        // 绘制水平线
        for (int j = 0; j< horizontalLineCount; j++) {
            Handles.DrawLine(new Vector3(-gridSize, gridSize * j, 0) + gridOffset, new Vector3(position.width + gridSize, gridSize * j, 0f) +
                gridOffset);
        }
        // 恢复网格颜色
        Handles.color = Color.white;
    }

    private void DrawDraggedLine() {
// 如果当前房间节点图的线条位置不为零
        if (currentRoomNodeGraph.linePosition != Vector2.zero) {
    // 绘制贝塞尔曲线，起点为当前房间节点图的房间节点中心，终点为线条位置，控制点为当前房间节点图的房间节点中心，线条颜色为白色，线条宽度为连接线宽度
            Handles.DrawBezier(currentRoomNodeGraph.roomNodeToDrawLineFrom.rect.center, currentRoomNodeGraph.linePosition,
                currentRoomNodeGraph.roomNodeToDrawLineFrom.rect.center, currentRoomNodeGraph.linePosition, Color.white, null, connectingLineWidth);
        }
    }

    private void ProcessEvents(Event currentEvent) 
    {

        graphDrag = Vector2.zero;
        //currentRoomNode = IsMouseOverRoomNode(currentEvent);

        if (currentRoomNode == null || currentRoomNode.isLeftClickDragging == false) {
            currentRoomNode = IsMouseOverRoomNode(currentEvent);
        }


        // if (currentRoomNode == null) {
        //     ProcessRoomNodeGraphEvents(currentEvent);
        // }
        if (currentRoomNode == null || currentRoomNodeGraph.roomNodeToDrawLineFrom != null) {
            ProcessRoomNodeGraphEvents(currentEvent);
        }
        else {
            currentRoomNode.ProcessEvents(currentEvent);
        }
    }


    private void ProcessRoomNodeGraphEvents(Event currentEvent)
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

         // 如果当前事件是鼠标右键点击
        if (currentEvent.button == 1) {
            // 显示右键菜单
            ShowContextMenu(currentEvent.mousePosition);
        } else if (currentEvent.button == 0) {
            // 清除线拖拽
            ClearLineDrag();
            // 清除所有选中的房间节点
            ClearAllSelectedRoomNodes();
        }
    }

    private void ProcessMouseUpEvent(Event currentEvent)
    {
        if (currentEvent.button == 1 && currentRoomNodeGraph.roomNodeToDrawLineFrom != null) {
            
            RoomNodeSO roomNode = IsMouseOverRoomNode(currentEvent);

            if (roomNode != null) {
                if (currentRoomNodeGraph.roomNodeToDrawLineFrom.AddChildRoomNodeIDToRoomNode(roomNode.id)) {
                    roomNode.AddParentRoomNodeIDToRoomNode(currentRoomNodeGraph.roomNodeToDrawLineFrom.id);
                }
            }

            ClearLineDrag();
        }
    }
    private void ProcessMouseDragEvent(Event currentEvent)
    {
        if (currentEvent.button == 1) {
            ProcessRightMouseDragEvent(currentEvent);
        }
        else if (currentEvent.button == 0) {
            ProcessLeftMouseDragEvent(currentEvent.delta);
        }
    }

    private void ShowContextMenu(Vector2 mousePosition)
    {
        // 创建一个GenericMenu对象
        GenericMenu menu = new GenericMenu();
        // 添加一个菜单项，点击后调用CreateRoomNode方法，并传入mousePosition参数
        menu.AddItem(new GUIContent("Create Room Node"), false, CreateRoomNode, mousePosition);
        // 添加一个分隔符
        menu.AddSeparator("");
        // 添加一个菜单项，点击后调用SelectAllRoomNodes方法
        menu.AddItem(new GUIContent("Select All Room Nodes"), false, SelectAllRoomNodes);
        // 添加一个分隔符
        menu.AddSeparator("");
        // 添加一个菜单项，点击后调用DeleteSelectedRoomNodeLinks方法
        menu.AddItem(new GUIContent("Delete Selected Room Node Links"), false, DeleteSelectedRoomNodeLinks);
        // 添加一个菜单项，点击后调用DeleteSelectedRoomNodes方法
        menu.AddItem(new GUIContent("Delete Selected Room Nodes"), false, DeleteSelectedRoomNodes);

        // 显示菜单
        menu.ShowAsContext();
    }

    private void CreateRoomNode(object mousePositionObj)
    {
        if (currentRoomNodeGraph.roomNodeList.Count == 0) {
            CreateRoomNode(new Vector2(200f, 200f), roomNodeTypeList.list.Find(x => x.isEntrance));
        }

        CreateRoomNode(mousePositionObj, roomNodeTypeList.list.Find(x => x.isNone));
    }

    private void CreateRoomNode(object mousePositionObj, RoomNodeTypeSO roomNodeType)
    {
        //获取鼠标位置
        Vector2 mousePosition = (Vector2)mousePositionObj;

        //创建一个RoomNodeSO实例
        RoomNodeSO roomNode = ScriptableObject.CreateInstance<RoomNodeSO>();

        //将roomNode添加到currentRoomNodeGraph的roomNodeList中
        currentRoomNodeGraph.roomNodeList.Add(roomNode);

        //初始化roomNode，设置位置、当前RoomNodeGraph和节点类型
        roomNode.Initialize(new Rect(mousePosition, new Vector2(nodeWidth, nodeHeight)), currentRoomNodeGraph, roomNodeType);

        //将roomNode添加到AssetDatabase中
        AssetDatabase.AddObjectToAsset(roomNode, currentRoomNodeGraph);
        //保存AssetDatabase中的更改
        AssetDatabase.SaveAssets();


        //refresh graph node dictionary
        currentRoomNodeGraph.OnValidate();
    }

    private void DeleteSelectedRoomNodes() {
        Queue<RoomNodeSO> roomNodesToDelete = new Queue<RoomNodeSO>();

        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList) {
            if (roomNode.isSelected && !roomNode.roomNodeType.isEntrance) {
                roomNodesToDelete.Enqueue(roomNode);

                foreach (string childRoomNodeID in roomNode.childRoomNodeIDList) {
                    RoomNodeSO childRoomNode = currentRoomNodeGraph.GetRoomNode(childRoomNodeID);
                    if (childRoomNode != null) {
                        childRoomNode.RemoveParentRoomNodeIDFromRoomNode(roomNode.id);
                    }
                }

                foreach (string parentRoomNodeID in roomNode.parentRoomNodeIDList) {
                    RoomNodeSO parentRoomNode = currentRoomNodeGraph.GetRoomNode(parentRoomNodeID);
                    if (parentRoomNode != null) {
                        parentRoomNode.RemoveChildRoomNodeIDFromRoomNode(roomNode.id);
                    }
                }
            }
        }

        while (roomNodesToDelete.Count > 0) {
            RoomNodeSO roomNodeToDelete = roomNodesToDelete.Dequeue();
            currentRoomNodeGraph.roomNodeDictionary.Remove(roomNodeToDelete.id);
            currentRoomNodeGraph.roomNodeList.Remove(roomNodeToDelete);
            DestroyImmediate(roomNodeToDelete, true);
            AssetDatabase.SaveAssets();
        }
    }

    private void DeleteSelectedRoomNodeLinks() {
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList) {
            if (roomNode.isSelected && roomNode.childRoomNodeIDList.Count > 0) {
                for (int i = roomNode.childRoomNodeIDList.Count - 1; i >= 0; i--) {
                    //get the child node
                    RoomNodeSO childRoomNode = currentRoomNodeGraph.GetRoomNode(roomNode.childRoomNodeIDList[i]);
                    //if the child room node is selected
                    if (childRoomNode != null && childRoomNode.isSelected) {
                        //remove childid from parent room node
                        roomNode.RemoveChildRoomNodeIDFromRoomNode(childRoomNode.id);
                        //remove parentid from child room node
                        childRoomNode.RemoveParentRoomNodeIDFromRoomNode(roomNode.id);

                    }
                }
            }
        }

        ClearAllSelectedRoomNodes();
    }

    private void DrawRoomNodes() {
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList) {

            if (roomNode.isSelected) {
                roomNode.Draw(roomNodeSelectedStyle);
            }
            else {
                roomNode.Draw(roomNodeStyle);
            }
        }
        GUI.changed = true;
    }

    private RoomNodeSO IsMouseOverRoomNode(Event currentEvent) {
        for (int i = currentRoomNodeGraph.roomNodeList.Count - 1; i >= 0; i--) {
            if (currentRoomNodeGraph.roomNodeList[i].rect.Contains(currentEvent.mousePosition)) {
                return currentRoomNodeGraph.roomNodeList[i];
            }
        }
        return null;
    }


    private void ProcessRightMouseDragEvent(Event currentEvent) {
        if (currentRoomNodeGraph.roomNodeToDrawLineFrom != null) {
            DrawConnectingLine(currentEvent.delta);
            GUI.changed = true;
        }
    }

    private void ProcessLeftMouseDragEvent(Vector2 dragDelta) {
        graphDrag = dragDelta;
        for (int i = 0; i < currentRoomNodeGraph.roomNodeList.Count; i++) {
            currentRoomNodeGraph.roomNodeList[i].DragNode(dragDelta);
        }
        GUI.changed = true;
    }

    public void DrawConnectingLine(Vector2 delta) {
        currentRoomNodeGraph.linePosition += delta;
    }

    private void ClearLineDrag() {
        currentRoomNodeGraph.roomNodeToDrawLineFrom = null;
        currentRoomNodeGraph.linePosition = Vector2.zero;
        GUI.changed = true;
    }

    private void ClearAllSelectedRoomNodes() {
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList) {
            if (roomNode.isSelected) {
                roomNode.isSelected = false;
                GUI.changed = true;
            }
        }
    }

    private void SelectAllRoomNodes() {
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList) {
            roomNode.isSelected = true;
        }
        GUI.changed = true;
    }

    private void DrawRoomConnections() {
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList) {
            if (roomNode.childRoomNodeIDList.Count > 0) {
                foreach (string childRoomNodeID in roomNode.childRoomNodeIDList) {
                    if (currentRoomNodeGraph.roomNodeDictionary.ContainsKey(childRoomNodeID)) {
                        DrawConnectionLine(roomNode, currentRoomNodeGraph.roomNodeDictionary[childRoomNodeID]);
                        GUI.changed = true;
                    }
                }

            }
        }
    }

    private void DrawConnectionLine(RoomNodeSO parentRoomNode, RoomNodeSO childRoomNode) {
        Vector2 startPostion = parentRoomNode.rect.center;
        Vector2 endPosition = childRoomNode.rect.center;

        Vector2 midPosition = (startPostion + endPosition) / 2f;

        Vector2 direction = endPosition - startPostion;

        Vector2 arrowTailPoint1 = midPosition - new Vector2(-direction.y, direction.x).normalized * connectingLineArrowSize;
        Vector2 arrowTailPoint2 = midPosition + new Vector2(-direction.y, direction.x).normalized * connectingLineArrowSize;

        Vector2 arrowHeadPoint = midPosition + direction.normalized * connectingLineArrowSize;

        Handles.DrawBezier(arrowHeadPoint, arrowTailPoint1, arrowHeadPoint, arrowTailPoint1, Color.white, null, connectingLineWidth);
        Handles.DrawBezier(arrowHeadPoint, arrowTailPoint2, arrowHeadPoint, arrowTailPoint2, Color.white, null, connectingLineWidth);

        Handles.DrawBezier(startPostion, endPosition, startPostion, endPosition, Color.white, null, connectingLineWidth);
        GUI.changed = true;
    }

    private void InspectorSelectionChanged() {
        RoomNodeGraphSO roomNodeGraph = Selection.activeObject as RoomNodeGraphSO;

        if (roomNodeGraph != null) {
            currentRoomNodeGraph = roomNodeGraph;
            GUI.changed = true;
        }
    }
}
