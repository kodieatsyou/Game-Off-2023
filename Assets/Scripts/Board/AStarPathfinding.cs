using System.Collections.Generic;
using UnityEngine;

public class AStarPathfinding
{
    private BoardSpace[,,] boardSpaceGrid;
    private BoardSpace start;
    private BoardSpace end;
    private Vector3 gridSize;
    private Node[,,] nodeGrid;

    public AStarPathfinding(BoardSpace[,,] boardSpaceGrid, BoardSpace start, BoardSpace end)
    {
        this.start = start;
        this.end = end;
        this.gridSize = new Vector3(boardSpaceGrid.GetLength(0), boardSpaceGrid.GetLength(1), boardSpaceGrid.GetLength(0));
        this.boardSpaceGrid = boardSpaceGrid;
        nodeGrid = new Node[(int)gridSize.x, (int)gridSize.y, (int)gridSize.z];

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                for (int z = 0; z < gridSize.z; z++)
                {
                    bool isWalkable = false;
                    if(boardSpaceGrid[x, y, z].GetIsBuilt() && boardSpaceGrid[x, y, z].GetNeighborValue() % 11 != 0)
                    {
                        isWalkable = true;
                    }
                    nodeGrid[x, y, z] = new Node(isWalkable, x, y, z);
                }
            }
        }
    }

    public List<BoardSpace> FindPath()
    {
        Debug.Log(nodeGrid.GetLength(1));
        Node startNode = nodeGrid[(int)start.GetBoardPosition().x, (int)start.GetBoardPosition().y, (int)start.GetBoardPosition().z];
        Node targetNode = nodeGrid[(int)end.GetBoardPosition().x, (int)end.GetBoardPosition().y, (int)end.GetBoardPosition().z];

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
            path.Add(boardSpaceGrid[currentNode.gridX, currentNode.gridY, currentNode.gridZ]);
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
                    if (x == 0 && y == 0 && z == 0)
                    {
                        continue;
                    }

                    int checkX = node.gridX + x;
                    int checkY = node.gridY + y;
                    int checkZ = node.gridZ + z;

                    if (checkX >= 0 && checkX < gridSize.x && checkY >= 0 && checkY < gridSize.y && checkZ >= 0 && checkZ < gridSize.z)
                    {
                        neighbors.Add(nodeGrid[checkX, checkY, checkZ]);
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

        // Use Manhattan distance for simplicity (can be replaced with Euclidean or other distance measures)
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