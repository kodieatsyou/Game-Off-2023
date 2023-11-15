using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class BoardSpace
{
    private Vector3 boardPos;
    private Vector3 worldPos;
    private float worldSpaceScalingFactor;
    private bool isBuilt;
    private bool isBuildable;
    private int neighborValue;
    private GameObject spaceObj;
    private GameObject detailObj;
    private bool playerInSpace;

    public BoardSpace(Vector3 boardPos, float worldSpaceScalingFactor, bool isBuilt, bool isBuildable)
    {
        this.boardPos = boardPos;
        this.worldPos = new Vector3(boardPos.x * worldSpaceScalingFactor, boardPos.y * worldSpaceScalingFactor, boardPos.z * worldSpaceScalingFactor);
        this.worldSpaceScalingFactor = worldSpaceScalingFactor;
        this.isBuilt = isBuilt;
        this.isBuildable = isBuildable;
        this.neighborValue = 0;
        this.spaceObj = null;
        this.detailObj = null;
        this.playerInSpace = false;
    }

    public void SetBoardPosition(Vector3 newPos) 
    { 
        this.boardPos = newPos;
        this.worldPos = new Vector3(this.boardPos.x * this.worldSpaceScalingFactor, this.boardPos.y * this.worldSpaceScalingFactor, this.boardPos.z * this.worldSpaceScalingFactor);
    }

    public Vector3 GetBoardPosition() { return this.boardPos; }

    public Vector3 GetWorldPosition() {  return this.worldPos; }

    public Vector3 GetWorldPositionOfTop() { return new Vector3((boardPos.x) * worldSpaceScalingFactor, (boardPos.y + 1) * worldSpaceScalingFactor, (boardPos.z) * worldSpaceScalingFactor); }

    public void SetWorldPosition(Vector3 newPos)
    {
        this.worldPos = new Vector3(newPos.x * this.worldSpaceScalingFactor, newPos.y * this.worldSpaceScalingFactor, newPos.z * this.worldSpaceScalingFactor);
    }

    public void SetWorldSpaceScalingFactor(float worlspaceScalingFactor)
    {
        this.worldSpaceScalingFactor = worlspaceScalingFactor;
        this.worldPos = new Vector3(this.boardPos.x * this.worldSpaceScalingFactor, this.boardPos.y * this.worldSpaceScalingFactor, this.boardPos.z * this.worldSpaceScalingFactor);
    }

    public float GetWorldSpaceScalingFactor() { return this.worldSpaceScalingFactor; }

    public void SetNeighborValue(int neighborValue) { 
        this.neighborValue = neighborValue; 
    }

    public int GetNeighborValue() { return this.neighborValue; }

    public void SetIsBuildable(bool isBuildable) { this.isBuildable = isBuildable; }

    public bool GetIsBuildable() { return this.isBuildable; }

    public void SetIsBuilt(bool isBuilt) { this.isBuilt = isBuilt; }

    public bool GetIsBuilt() { return this.isBuilt; }

    public bool SetSpaceObj(Transform parent = null, string name = "")
    {
        try
        {
            spaceObj = GameObject.Instantiate(GetBlock(), worldPos, Quaternion.identity);
            if(name == "")
            {
                spaceObj.name = (boardPos.x + "," + boardPos.y + "," + boardPos.z);
            } else {
                spaceObj.name = name;
            }

            if(parent != null)
            {
                spaceObj.transform.parent = parent;
            }
            spaceObj.AddComponent<Block>().SetSpaceObj(this);
            return true;
        }
        catch
        {
            Debug.Log("Failed creating object for space at: (" + boardPos.x + "," + boardPos.y + "," + boardPos.z + ")!");
            return false;
        }
    }

    public GameObject GetSpaceObj() { return spaceObj; }

    public bool SetDetailObj()
    {
        try
        {
            if(isBuilt)
            {
                System.Random rand = new System.Random();
                int randDetailPrefabIdx = rand.Next(GameAssets.i.block_details_.Length);
                int rotationAngle = rand.Next(20, 90); // rotate between 20 to 90 degrees

                detailObj = GameObject.Instantiate(
                    GameAssets.i.block_details_[randDetailPrefabIdx], 
                    new Vector3((boardPos.x) * worldSpaceScalingFactor, (boardPos.y + 1) * worldSpaceScalingFactor, (boardPos.z) * worldSpaceScalingFactor), 
                    Quaternion.AngleAxis(rotationAngle, Vector3.up));
                detailObj.name = spaceObj.name + " DETAIL";
                detailObj.transform.parent = spaceObj.transform;
                return true;
            } else
            {
                return false;
            }
            
        }
        catch
        {
            Debug.Log("Failed creating detail object for space at: (" + boardPos.x + "," + boardPos.y + "," + boardPos.z + ")!");
            return false;
        }
    }

    public void SetPlayerInSpace(bool playerInSpace) { this.playerInSpace = playerInSpace; }

    public bool GetPlayerInSpace() {  return this.playerInSpace; }

    private GameObject GetBlock()
    {
        GameObject block = null;
        if(isBuilt)
        {
            if (neighborValue % 11 == 0 && neighborValue != 0)
            {
                //Debug.Log("STONE BLOCK AT: " + x + "," + y + "," + z + " WITH NEIGHBOR VALUE OF: " + neighborValue);
                block = GameAssets.i.stone_blocks_[neighborValue / 11];
            }
            else
            {
                //Debug.Log("GRASS BLOCK AT: " + x + "," + y + "," + z + " WITH NEIGHBOR VALUE OF: " + neighborValue);
                block = GameAssets.i.grass_blocks_[neighborValue];
            }
        } else
        {
            if(isBuildable)
            {
                block = GameAssets.i.buildable_block_;
            }
        }
        return block;
    }

}

public class BoardManager : MonoBehaviour
{
    public int BaseSize = 10;
    public int RandomBlockScale = 4; // pick scale of random blocks to the board size. Ex: 4 means BaseSize * 4 = 40 random blocks
    private static int HeightSize;
    private static int RandomBlockCount;
    public static BoardSpace[,,] BoardSpace_Arr;

    public Vector3 start;
    public Vector3 end;

    public int currentBuiltHeight = 0; // marks the current highest built y position

    // Start is called before the first frame update
    void Start()
    {
        HeightSize = BaseSize * 2;
        RandomBlockCount = BaseSize * RandomBlockScale;

        BoardSpace_Arr = new BoardSpace[BaseSize, HeightSize, BaseSize];

        bool[,,] isRandom = GenerateRandomGrid();
        //Debug.Log(isRandom);

        // Fill CanBuildOn_Arr and IsBuilt_Arr with rule-based values
        for (int x = 0; x < BaseSize; x++)
        {
            for (int y = 0; y < HeightSize; y++)
            {
                for (int z = 0; z < BaseSize; z++)
                {
                    if (isRandom[x, y, z])
                    {
                        BoardSpace_Arr[x, y, z] = new BoardSpace(new Vector3(x, y, z), 2.5f, true, false);
                    }
                    else if (y == 0)
                    {
                        BoardSpace_Arr[x, y, z] = new BoardSpace(new Vector3(x, y, z), 2.5f, true, false);
                    }
                    else if (y >= 1)
                    {
                        BoardSpace_Arr[x, y, z] = new BoardSpace(new Vector3(x, y, z), 2.5f, false, false);
                    }
                    else
                    {
                        //Debug.Log("MISSING RULE FOR COORD (" + x + "," + y + "," + z + ")");
                    }
                }
            }
        }

        // Update blocks
        for (int x = 0; x < BaseSize; x++)
        {
            for (int y = 0; y < HeightSize; y++)
            {
                for (int z = 0; z < BaseSize; z++)
                {
                    UpdateSpace(x, y, z);
                }
            }
        }
        PlacePlayerOnRandomBlock();
        EventManager.TriggerEvent("OnBoardDoneInitializing", this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        // need to update Isbuildable, isbuilt, game object asset of whatever changed.
    }

    public void UpdateSpace(int x, int y, int z)
    {
        int neighborValue = GetBlockNeighborsBuiltValue(x, y, z);
        BoardSpace_Arr[x, y, z].SetNeighborValue(neighborValue);
        BoardSpace_Arr[x, y, z].SetSpaceObj(this.transform);

        //Check if there are any built blocks above this block
        if (neighborValue % 11 != 0)
        {
            //Check if a detail should be added to the top of the block
            System.Random rand = new System.Random();
            bool chanceForDetail = rand.Next(100) < 40;
            if (chanceForDetail)
            {
                BoardSpace_Arr[x, y, z].SetDetailObj();
            }
            if(y + 1 < HeightSize && BoardSpace_Arr[x, y, z].GetIsBuilt())
            {
                BoardSpace_Arr[x, y + 1, z].SetIsBuildable(true);
            }
        }
    }

    /// <summary>
    /// Gets an int representing the value for the "is built" status of the blocks to the left right front back and above of a specified coordinate
    /// If the value returned % 11 == 0 then there is a block above
    /// </summary>
    /// <param name="x">x coordinate</param>
    /// <param name="y">y coordinate</param>
    /// <param name="z">z coordinate</param>
    public int GetBlockNeighborsBuiltValue(int x, int y, int z)
    {
        int neighborValue = 1;
        if (z + 1 < BaseSize)
        {
            if (BoardSpace_Arr[x, y, z + 1].GetIsBuilt()) neighborValue *= 2; //Front Block
        }
        if (z - 1 >= 0) {
            if (BoardSpace_Arr[x, y, z - 1].GetIsBuilt()) neighborValue *= 3; //Behind Block
        }
        if (x + 1 < BaseSize)
        {
            if (BoardSpace_Arr[x + 1, y, z].GetIsBuilt()) neighborValue *= 5; //Left Block
        }
        if (x - 1 >= 0)
        {
            if (BoardSpace_Arr[x - 1, y, z].GetIsBuilt()) neighborValue *= 7; //Right Block
        }
        if (y + 1 < HeightSize)
        {
            if (BoardSpace_Arr[x, y + 1, z].GetIsBuilt()) neighborValue *= 11; //Above Block
        }
        return neighborValue;
    }

    /// <summary>
    ///  Generates a bool 3d array of coordinates in the field that determine if a block should be random or not.
    ///  Max height of a random coord is 2 currently
    /// </summary>
    bool[,,] GenerateRandomGrid()
    {
        bool[,,] randomCoords = new bool[BaseSize, HeightSize, BaseSize];
        for (int x = 0; x < BaseSize; x++) // preload randomCoords with false
        {
            for (int y = 0; y < HeightSize; y++)
            {
                for (int z = 0; z < BaseSize; z++)
                {
                    randomCoords[x, y, z] = false;
                }
            }
        }

        System.Random rand = new System.Random();
        int count = 0;
        while (count < RandomBlockCount) // create RandomBlockCount random x and z set of coordinates
        {
            int randX = rand.Next(0, BaseSize);
            int randZ = rand.Next(0, BaseSize);
            while (randomCoords[randX, 1, randZ]) // while no collision
            {
                randX = rand.Next(0, BaseSize);
                randZ = rand.Next(0, BaseSize);
            }
            randomCoords[randX, 1, randZ] = true;

            bool buildTwo = rand.Next(100) < 20; // 20% chance for one coord to be 2 height
            if (buildTwo)
            {
                randomCoords[randX, 2, randZ] = true;
            }

            count++;
        }
        return randomCoords;
    }

    /// <summary>
    ///  Returns the coordinates for the exact middle of the grid with the y set to the highest built level.
    /// </summary>
    public Vector3 GetBoardMiddlePos()
    {
        if(Math.Floor(BaseSize / 2.0f) == BaseSize / 2.0f) //Check if the middle of the grid is a float or not. If its a whole number the middle is between 2 blocks else the middle is a whole block. 
        {
            return new Vector3(((BaseSize / 2.0f) * 2.5f) - 1, currentBuiltHeight * 2.5f, ((BaseSize / 2.0f) * 2.5f) - 1); // Subtract one to get the location between the two "middle" blocks
            
        } else
        {
            return new Vector3((BaseSize / 2.0f) * 2.5f, currentBuiltHeight * 2.5f, (BaseSize / 2.0f) * 2.5f); // Get the location of the center of the middle block
        }
        
    }

    public void PlacePlayerOnRandomBlock()
    {
        List<BoardSpace> availableBlocks = new List<BoardSpace> ();
        for (int x = 0; x < BaseSize; x++)
        {
            for (int y = 0; y < 1; y++)
            {
                for (int z = 0; z < BaseSize; z++)
                {
                    if (BoardSpace_Arr[x, y, z].GetIsBuilt() && BoardSpace_Arr[x, y, z].GetNeighborValue() % 11 != 0)
                    {
                        availableBlocks.Add(BoardSpace_Arr[x, y, z]);
                    }
                }
            }
        }
        System.Random rand = new System.Random();
        BoardSpace spaceToSpawn = availableBlocks[rand.Next(availableBlocks.Count)];
        spaceToSpawn.SetPlayerInSpace(true);
        Debug.Log("Placing player on block: " + spaceToSpawn.GetSpaceObj().name);
        Instantiate(GameAssets.i.player_object_, spaceToSpawn.GetWorldPositionOfTop(), Quaternion.identity);
    }

    public List<BoardSpace> FindPathInGrid()
    {
        BoardSpace from = BoardSpace_Arr[(int)start.x, (int)start.y, (int)start.z];
        BoardSpace to = BoardSpace_Arr[(int)end.x, (int)end.y, (int)end.z];
        List<BoardSpace> path = new AStarPathfinding(BoardSpace_Arr, from, to).FindPath();
        if(path == null)
        {
            Debug.Log("Could not find path!");
        }
        return path;
    }

}





/*
 * 
 * /// <summary>
    /// Returns the correct prefab to use for block at x, y, z coordinate
    /// </summary>
    /// <param name="neighborValue">Value of the blocks neighbors based on if they have been built on or not.</param>
    public GameObject GetBlock(int neighborValue)
    {
        GameObject block;
        
        if(neighborValue % 11 == 0 && neighborValue != 0)
        {
            //Debug.Log("STONE BLOCK AT: " + x + "," + y + "," + z + " WITH NEIGHBOR VALUE OF: " + neighborValue);
            block = GameAssets.i.stone_blocks_[neighborValue / 11];
        } else
        {
            //Debug.Log("GRASS BLOCK AT: " + x + "," + y + "," + z + " WITH NEIGHBOR VALUE OF: " + neighborValue);
            block = GameAssets.i.grass_blocks_[neighborValue];
        }

        return block;
    }

    /// <summary>
    /// Uses the current CanBuildOn_Arr status and given coordinates to determine if the block at said coordinates can be built.
    /// Only condition where buildable is when the block below has been built, there is no player on the current block, and the coordinates are valid
    /// </summary>
    /// <param name="x">X coordinate value</param>
    /// <param name="y">y coordinate value</param>
    /// <param name="z">z coordinate value</param>
    /// <returns>Boolean representing if the block is buildable or not. True for the player can build, false for not.</returns>
    public bool IsBuildable(int x, int y, int z)
    {
        if (IsBuilt_Arr[x, y, z]) //already built blocks are never buildable
        {
            return false;
        }
        else if ((!IsBuilt_Arr[x, y - 1, z]) && (y - 1 > 0) && (y < HeightSize))
        {
            return true;
        }
        else
        {
            return false; // default return false
        }
    }

    /// <summary>
    /// Places a GameObject of cube at a specified coordinates. Treats levels of the cube differently for different Cube GameObjects
    /// </summary>
    /// <param name="prefab_type">Type of prefab to place</param>
    /// <param name="x">x coordinate</param>
    /// <param name="y">y coordinate</param>
    /// <param name="z">z coordinate</param>
    public void PlaceCube(int x, int y, int z)
    {
        if (y + 1 > currentBuiltHeight) currentBuiltHeight = y + 1; //Set the current built height to this blocks y position if the y is greater.

        int neighborValue = GetBlockNeighborsBuiltValue(x, y, z);

        BoardCube_Arr[x, y, z] = GameObject.Instantiate(GetBlock(neighborValue), new Vector3(x * 2.5f, y * 2.5f, z * 2.5f), transform.rotation);
        BoardCube_Arr[x, y, z].transform.SetParent(this.transform, false);
        BoardCube_Arr[x, y, z].name = (x + "," + y + "," + z);

        //Check if there are any built blocks above this block
        if(neighborValue % 11 != 0)
        {
            //Check if a detail should be added to the top of the block
            System.Random rand = new System.Random();
            bool chanceForDetail = rand.Next(100) < 40;
            if (chanceForDetail)
            {
                PlaceDetail(BoardCube_Arr[x, y, z].transform, x, y, z);
            }

            //Mark the block above as can be built
            CanBuildOn_Arr[x, y + 1, z] = true;
            //Instantiate the "buildable" block prefab on block above this one
            BoardCube_Arr[x, y + 1, z] = GameObject.Instantiate(buildableBlock, new Vector3(x * 2.5f, (y + 1) * 2.5f, z * 2.5f), transform.rotation);
            BoardCube_Arr[x, y + 1, z].transform.SetParent(this.transform, false);
            BoardCube_Arr[x, y + 1, z].name = (x + "," + y + "," + z + " BUILDABLE");
        }
    }

    /// <summary>
    /// Places a detail prefab on top of a block at the specified coordinate
    /// </summary>
    /// <param name="parentTransform">Parent GameObject, should be the block which is getting the detail added to it.</param>
    /// <param name="x">x coordinate of parent</param>
    /// <param name="y">y coordinate of parent</param>
    /// <param name="z">z coordinate of parent</param>
    public void PlaceDetail(Transform parentTransform, int x, int y, int z)
    {
        System.Random rand = new System.Random();
        int randDetailPrefabIdx = rand.Next(GameAssets.i.block_details_.Length);
        int rotationAngle = rand.Next(20, 90); // rotate between 20 to 90 degrees

        BoardCubeDetail_Arr[x, y, z] = GameObject.Instantiate(GameAssets.i.block_details_[randDetailPrefabIdx], parentTransform.position, Quaternion.AngleAxis(rotationAngle, Vector3.up), parentTransform);
        BoardCubeDetail_Arr[x, y, z].name = x + "," + y + "," + z + "-DetailChild";

        // TODO this seems like a hacky way to just add 2.5f to the object to make it appear on the top of the block.
        Vector3 topOfBlock = new Vector3(0, 2.5f, 0);
        BoardCubeDetail_Arr[x, y, z].transform.position += topOfBlock;
    }
*/
