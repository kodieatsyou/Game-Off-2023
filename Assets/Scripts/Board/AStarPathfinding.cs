using System.Collections.Generic;
using UnityEngine;

public class AStarPathfinding
{
    private BoardSpace start;
    private BoardSpace end;
    private Vector3 gridSize;
    private Node[,,] nodeGrid;

    public AStarPathfinding(BoardSpace start, BoardSpace end)
    {
        Debug.Log("Setting board space end to: " + end.ToString());
        this.start = start;
        this.end = end;
        gridSize = new Vector3(Board.Instance.baseSize, Board.Instance.heightSize, Board.Instance.baseSize);
        nodeGrid = new Node[Board.Instance.baseSize, Board.Instance.heightSize, Board.Instance.baseSize];
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                for (int z = 0; z < gridSize.z; z++)
                {
                    bool isWalkable = false;
                    if(Board.Instance.boardArray[x, y, z].GetIsBuilt())
                    {
                        if(y + 1 >= Board.Instance.heightSize || (!Board.Instance.boardArray[x, y + 1, z].GetIsBuilt() && Board.Instance.boardArray[x, y, z].GetPlayerOnSpace() == null))
                        isWalkable = true;
                    }
                    nodeGrid[x, y, z] = new Node(isWalkable, x, y, z);
                }
            }
        }
    }

    public List<BoardSpace> FindPath()
    {
        Node startNode = nodeGrid[(int)start.GetPosInBoard().x, (int)start.GetPosInBoard().y, (int)start.GetPosInBoard().z];
        Node targetNode = nodeGrid[(int)end.GetPosInBoard().x, (int)end.GetPosInBoard().y, (int)end.GetPosInBoard().z];

        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node currentNode = GetLowestFCostNode(openSet);
            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
            {
                // Path found, reconstruct and return the path
                return RetracePath(startNode, targetNode);
            }

            foreach (Node neighbor in GetNeighbors(currentNode))
            {
                if (!neighbor.isWalkable || closedSet.Contains(neighbor))
                {
                    continue;
                }

                int newMovementCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);
                if (newMovementCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = newMovementCostToNeighbor;
                    neighbor.hCost = GetDistance(neighbor, targetNode);
                    neighbor.parent = currentNode;

                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                }
            }
        }

        // No path found
        return null;
    }

    private List<BoardSpace> RetracePath(Node startNode, Node endNode)
    {
        List<BoardSpace> path = new List<BoardSpace>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(Board.Instance.boardArray[currentNode.gridX, currentNode.gridY, currentNode.gridZ]);
            currentNode = currentNode.parent;
        }

        path.Reverse();
        return path;
    }

    private List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    // Exclude diagonal movement horizontally
                    if (x != 0 && z != 0)
                        continue;

                    int checkX = node.gridX + x;
                    int checkY = node.gridY + y;
                    int checkZ = node.gridZ + z;

                    // Debug statement to check indices
                    Debug.Log($"Checking indices: ({checkX}, {checkY}, {checkZ})");

                    // Check if the position is within the grid boundaries
                    if (checkX >= 0 && checkX < gridSize.x && checkY >= 0 && checkY < gridSize.y && checkZ >= 0 && checkZ < gridSize.z)
                    {
                        // Debug statement to check walkability
                        Debug.Log($"Walkable: {nodeGrid[checkX, checkY, checkZ].isWalkable}");

                        // Check if the node at the position is walkable
                        if (nodeGrid[checkX, checkY, checkZ].isWalkable)
                        {
                            // Check for clear vertical path
                            if (Mathf.Abs(node.gridY - checkY) <= 1 && Mathf.Abs(node.gridZ - checkZ) <= 1)
                            {
                                neighbors.Add(nodeGrid[checkX, checkY, checkZ]);
                            }
                        }
                    }
                }
            }
        }

        return neighbors;
    }

    private int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);
        int dstZ = Mathf.Abs(nodeA.gridZ - nodeB.gridZ);
        return dstX + dstY + dstZ;
    }

    private Node GetLowestFCostNode(List<Node> nodeList)
    {
        Node lowestFCostNode = nodeList[0];
        foreach (Node node in nodeList)
        {
            if (node.fCost < lowestFCostNode.fCost)
            {
                lowestFCostNode = node;
            }
        }
        return lowestFCostNode;
    }

    private class Node
    {
        public bool isWalkable;
        public int gridX, gridY, gridZ;
        public int gCost, hCost;
        public Node parent;

        public Node(bool isWalkable, int gridX, int gridY, int gridZ)
        {
            this.isWalkable = isWalkable;
            this.gridX = gridX;
            this.gridY = gridY;
            this.gridZ = gridZ;
        }

        public int fCost
        {
            get { return gCost + hCost; }
        }
    }
}