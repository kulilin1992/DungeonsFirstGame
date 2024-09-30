using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AStar
{
    public static Stack<Vector3> BuildPath(Room room, Vector3Int startGridPos, Vector3Int endGridPos)
    {
        startGridPos -= (Vector3Int)room.templateLowerBounds;
        endGridPos -= (Vector3Int) room.templateLowerBounds;

        List<Node> openNodeList = new List<Node>();
        HashSet<Node> closedNodeList = new HashSet<Node>();

        GridNodes gridNodes= new GridNodes(room.templateUpperBounds.x - room.templateLowerBounds.x + 1,
            room.templateUpperBounds.y - room.templateLowerBounds.y + 1);

        Node startNode = gridNodes.GetGridNode(startGridPos.x, startGridPos.y);
        Node targetNode = gridNodes.GetGridNode(endGridPos.x, endGridPos.y);

        Node endPathNode = FindShortestPath(startNode, targetNode, gridNodes, openNodeList, closedNodeList, room.instantiateRoom);

        if (endPathNode != null) {
            return CreatePathStack(endPathNode, room);
        }
        return null;
        
    }

    private static Node FindShortestPath(Node startNode, Node targetNode, GridNodes gridNodes, List<Node> openNodeList, HashSet<Node> closedHashNodeList, InstantiateRoom instantiateRoom)
    {
        openNodeList.Add(startNode);

        while (openNodeList.Count > 0)
        {
            openNodeList.Sort();

            //the node in the open list with the lowest f cost
            Node currentNode = openNodeList[0];
            openNodeList.RemoveAt(0);

            if (currentNode == targetNode)
            {
                return currentNode;
            }

            closedHashNodeList.Add(currentNode);

            EvaluteCurrentNodeNeighbours(currentNode, targetNode, gridNodes, openNodeList, closedHashNodeList, instantiateRoom);
        }
        return null;
    }

    private static void EvaluteCurrentNodeNeighbours(Node currentNode, Node targetNode, GridNodes gridNodes, List<Node> openNodeList, HashSet<Node> closedHashNodeList, InstantiateRoom instantiateRoom)
    {
        Vector2Int currentNodeGridPos = currentNode.gridPosition;
        Node validNeighbourNode;

        //loop through all directions
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0)
                {
                    continue;
                }

                validNeighbourNode = GetValidNodeNeighbour(currentNodeGridPos.x + i, currentNodeGridPos.y + j,
                    gridNodes, closedHashNodeList, instantiateRoom);
                    
                if (validNeighbourNode != null) {
                    //calculate the g cost for the neighbour node
                    int newCostToNeighbour;

                    //get the movement penalty
                    // unwalkable paths have a value of 0 default movement penalty
                    //settings and applies to other grid squares
                    int movementPenaltyForGridSpace = instantiateRoom.aStarMovementPenalty[validNeighbourNode.gridPosition.x,
                        validNeighbourNode.gridPosition.y];

                    newCostToNeighbour = currentNode.gCost + GetDistance(currentNode, validNeighbourNode) + movementPenaltyForGridSpace;

                    bool isValidNeighbourNodeInOpenList = openNodeList.Contains(validNeighbourNode);

                    if (newCostToNeighbour < validNeighbourNode.gCost || !isValidNeighbourNodeInOpenList)
                    {
                        validNeighbourNode.gCost = newCostToNeighbour;
                        validNeighbourNode.hCost = GetDistance(validNeighbourNode, targetNode);
                        validNeighbourNode.parentNode = currentNode;
                        
                        if (!isValidNeighbourNodeInOpenList)
                        {
                            openNodeList.Add(validNeighbourNode);
                        }
                    }   
                }
            }
        }
    }

    private static int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridPosition.x - nodeB.gridPosition.x);
        int dstY = Mathf.Abs(nodeA.gridPosition.y - nodeB.gridPosition.y);

        if (dstX > dstY) return 10 * (dstX - dstY) + 14 * dstY;
        return 10 * (dstY - dstX) + 14 * dstX;
    }

    private static Node GetValidNodeNeighbour(int neighbourNodeXPos, int neighbourNodeYPos, GridNodes gridNodes, HashSet<Node> closedHashNodeList, InstantiateRoom instantiateRoom)
    {
        //if neighbour node position is beyond the grid size, return null
        if (neighbourNodeXPos < 0 || neighbourNodeXPos >= instantiateRoom.room.templateUpperBounds.x - instantiateRoom.room.templateLowerBounds.x
        || neighbourNodeYPos < 0 || neighbourNodeYPos >= instantiateRoom.room.templateUpperBounds.y - instantiateRoom.room.templateLowerBounds.y)
        {
            return null;
        }

        Node neighbourNode = gridNodes.GetGridNode(neighbourNodeXPos, neighbourNodeYPos);

        int movementPenaltyForGridSpace = instantiateRoom.aStarMovementPenalty[neighbourNodeXPos, neighbourNodeYPos];

        // check for moveable obstacle at that position
        int itemObstacleForGridSpace = instantiateRoom.aStarItemObstacles[neighbourNodeXPos, neighbourNodeYPos];

        //if neighbour is in the closed list or is an obstacle
        if (movementPenaltyForGridSpace == 0 || itemObstacleForGridSpace == 0 || closedHashNodeList.Contains(neighbourNode))
        {
            return null;
        }
        else
        {
            return neighbourNode;
        }
        
    }

    private static Stack<Vector3> CreatePathStack(Node targetNode, Room room)
    {
        Stack<Vector3> pathStack = new Stack<Vector3>();

        Node nextNode = targetNode;

        //get mid point of cell
        Vector3 cellMidPoint = room.instantiateRoom.grid.cellSize * 0.5f;
        cellMidPoint.z = 0f;

        while (nextNode != null)
        {
            //convert grid position to world position
            Vector3 worldPos = room.instantiateRoom.grid.CellToWorld(new Vector3Int(
                nextNode.gridPosition.x + room.templateLowerBounds.x,
                nextNode.gridPosition.y + room.templateLowerBounds.y, 0
            ));

            //add mid point of cell to world position
            worldPos += cellMidPoint;
            pathStack.Push(worldPos);
            nextNode = nextNode.parentNode;
        }
        return pathStack;
    }
}
