using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public int BaseSize = 10;
    public int RandomBlockScale = 4; // pick scale of random blocks to the board size. Ex: 4 means BaseSize * 4 = 40 random blocks
    private static int HeightSize;
    private static int RandomBlockCount;
    public GameObject[] Detail_Prefabs;
    public static GameObject[,,] BoardCube_Arr;
    public static GameObject[,,] BoardCubeDetail_Arr; // tracks detail gameobjects on blocks
    public static bool[,,] CanBuildOn_Arr; // marks block coordinates as buildable
    public static bool[,,] IsBuilt_Arr; // marks block coordinates as built

    // Start is called before the first frame update
    void Start()
    {
        HeightSize = BaseSize * 2;
        RandomBlockCount = BaseSize * RandomBlockScale;

        CanBuildOn_Arr = new bool[BaseSize, HeightSize, BaseSize];
        IsBuilt_Arr = new bool[BaseSize, HeightSize, BaseSize];
        BoardCube_Arr = new GameObject[BaseSize, HeightSize, BaseSize];
        BoardCubeDetail_Arr = new GameObject[BaseSize, HeightSize, BaseSize];

        bool[,,] isRandom = GenerateRandomGrid();
        Debug.Log(isRandom);

        // Fill CanBuildOn_Arr and IsBuilt_Arr with rule-based values
        for (int x = 0; x < BaseSize; x++)
        {
            for (int y = 0; y < HeightSize; y++)
            {
                for (int z = 0; z < BaseSize; z++)
                {
                    if (isRandom[x, y, z])
                    {
                        IsBuilt_Arr[x, y, z] = true;
                        CanBuildOn_Arr[x, y, z] = true;
                    }
                    else if (y == 0)
                    {
                        IsBuilt_Arr[x, y, z] = true;
                        CanBuildOn_Arr[x, y, z] = true;
                    }
                    else if (y >= 1)
                    {
                        IsBuilt_Arr[x, y, z] = false;
                        CanBuildOn_Arr[x, y, z] = false;
                    }
                    else
                    {
                        Debug.Log("MISSING RULE FOR COORD (" + x + "," + y + "," + z + ")");
                    }
                }
            }
        }

        // Place blocks
        for (int x = 0; x < BaseSize; x++)
        {
            for (int y = 0; y < HeightSize; y++)
            {
                for (int z = 0; z < BaseSize; z++)
                {
                    if(IsBuilt_Arr[x, y, z])
                    {
                        PlaceCube(x, y, z);
                    } else
                    {
                        PlaceCube(x, y, z, true);
                    }
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // need to update Isbuildable, isbuilt, game object asset of whatever changed.
    }

    /// <summary>
    /// Places a GameObject of cube at a specified coordinates. Treats levels of the cube differently for different Cube GameObjects
    /// </summary>
    /// <param name="prefab_type">Type of prefab to place</param>
    /// <param name="x">x coordinate</param>
    /// <param name="y">y coordinate</param>
    /// <param name="z">z coordinate</param>
    public void PlaceCube(int x, int y, int z, bool isInvisible = false)
    {
        if (!isInvisible)
        {
            BoardCube_Arr[x, y, z] = GameObject.Instantiate(GetBlock(x, y, z), new Vector3(x * 2.5f, y * 2.5f, z * 2.5f), transform.rotation);
            BoardCube_Arr[x, y, z].transform.SetParent(this.transform, false);
            BoardCube_Arr[x, y, z].name = (x + "," + y + "," + z);
            Debug.Log("Placed block at (" + x + "," + y + "," + z + ") marked as built.");

            System.Random rand = new System.Random();
            bool chanceForDetail = rand.Next(100) < 20;
            if (chanceForDetail)
            {
                PlaceDetail(BoardCube_Arr[x, y, z].transform, x, y, z);
            }
        }
    }

    public void PlaceDetail(Transform parentTransform, int x, int y, int z)
    {
        int topNeighborValue = GetBlockNeighborsBuiltValue(x, y, z) % 11;
        if (topNeighborValue != 0) // no top neighbor place detail
        {
            System.Random rand = new System.Random();
            int randDetailPrefabIdx = rand.Next(Detail_Prefabs.Length);
            int rotationAngle = rand.Next(20, 90); // rotate between 20 to 90 degrees

            BoardCubeDetail_Arr[x, y, z] = GameObject.Instantiate(Detail_Prefabs[randDetailPrefabIdx], parentTransform.position, Quaternion.AngleAxis(rotationAngle, Vector3.up), parentTransform);
            BoardCubeDetail_Arr[x, y, z].name = x + "," + y + "," + z + "-DetailChild";

            // TODO this seems like a hacky way to just add 2.5f to the object to make it appear on the top of the block.
            Vector3 topOfBlock = new Vector3(0, 2.5f, 0);
            BoardCubeDetail_Arr[x, y, z].transform.position += topOfBlock;

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
        Boolean[] neighbors = new Boolean[4];
        if (z + 1 < BaseSize)
        {
            if (IsBuilt_Arr[x, y, z + 1]) neighborValue *= 2; //Front Block
        }
        if (z - 1 >= 0) {
            if (IsBuilt_Arr[x, y, z - 1]) neighborValue *= 3; //Behind Block
        }
        if (x + 1 < BaseSize)
        {
            if (IsBuilt_Arr[x + 1, y, z]) neighborValue *= 5; //Left Block
        }
        if (x - 1 >= 0)
        {
            if (IsBuilt_Arr[x - 1, y, z]) neighborValue *= 7; //Right Block
        }
        if (y + 1 < HeightSize)
        {
            if (IsBuilt_Arr[x, y + 1, z]) neighborValue *= 11; //Above Block
        }
        //We dont need bottom block
        return neighborValue;
    }

    /// <summary>
    /// Returns the correct prefab to use for block at x, y, z coordinate
    /// </summary>
    /// <param name="x">x coordinate</param>
    /// <param name="y">y coordinate</param>
    /// <param name="z">z coordinate</param>
    public GameObject GetBlock(int x, int y, int z)
    {
        int neighborValue = GetBlockNeighborsBuiltValue(x, y, z);
        GameObject block;
        
        if(neighborValue % 11 == 0 && neighborValue != 0)
        {
            Debug.Log("STONE BLOCK AT: " + x + "," + y + "," + z + " WITH NEIGHBOR VALUE OF: " + neighborValue);
            block = GameAssets.i.stone_blocks_[neighborValue / 11];
        } else
        {
            Debug.Log("GRASS BLOCK AT: " + x + "," + y + "," + z + " WITH NEIGHBOR VALUE OF: " + neighborValue);
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

}
