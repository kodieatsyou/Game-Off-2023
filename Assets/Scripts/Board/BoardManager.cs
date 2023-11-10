using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
<<<<<<< HEAD
    public int BaseSize = 10;
    private static int HeightSize;
    private static int RandomBlockCount;
=======
    public static int BaseSize = 10;
    private static int HeightSize = BaseSize * 2;
    public static int randomBlockCount = 40;
>>>>>>> origin/board-generation-beautifying
    public GameObject MiddleBlock_Prefab;
    public GameObject BaseBlock_Prefab;
    public GameObject InvisibleBlock_Prefab;
    public GameObject[] Detail_Prefabs;
    public static GameObject[,,] BoardCube_Arr;
    public static bool[,,] CanBuildOn_Arr; // marks block coordinates as buildable
    public static bool[,,] IsBuilt_Arr; // marks block coordinates as built

    // Start is called before the first frame update
    void Start()
    {
        HeightSize = BaseSize * 2;
        RandomBlockCount = BaseSize * 4;

        CanBuildOn_Arr = new bool[BaseSize, HeightSize, BaseSize];
        IsBuilt_Arr = new bool[BaseSize, HeightSize, BaseSize];
        BoardCube_Arr = new GameObject[BaseSize, HeightSize, BaseSize];

        bool[,,] isRandom = GenerateRandomGrid();
        Debug.Log(isRandom);

        for (int x = 0; x < BaseSize; x++)
        {
            for (int y = 0; y < HeightSize; y++)
            {
                for (int z = 0; z < BaseSize; z++)
                {
                    if (isRandom[x, y, z])
                    {
                        Debug.Log("Placing Randomized Block");
                        PlaceCube(MiddleBlock_Prefab, x, y, z);
                        CanBuildOn_Arr[x, y, z] = true;
                    }
                    else if (y == 0)
                    {
                        Debug.Log("Placing Base Block");
                        PlaceCube(BaseBlock_Prefab, x, y, z);
                        CanBuildOn_Arr[x, y, z] = true;
                    }
                    else if (y >= 1)
                    {
                        Debug.Log("Placing Invisible Block");
                        PlaceCube(InvisibleBlock_Prefab, x, y, z, true);
                        CanBuildOn_Arr[x, y, z] = false;
                    }
                    else
                    {
                        Debug.Log("MISSING RULE FOR COORD (" + x + "," + y + "," + z + ")");
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
    public void PlaceCube(GameObject prefab_type, int x, int y, int z, bool isInvisible = false)
    {
<<<<<<< HEAD
        BoardCube_Arr[x, y, z] = (GameObject)Instantiate(prefab_type, new Vector3(x, y, z), transform.rotation);
        BoardCube_Arr[x, y, z].name = "Block(" + x + "," + y + "," + z + ")";
=======
        BoardCube_Arr[x, y, z] = (GameObject)Instantiate(prefab_type, new Vector3(x * 2.5f, y * 2.5f, z * 2.5f), transform.rotation);
        BoardCube_Arr[x, y, z].transform.SetParent(this.transform, false);
        BoardCube_Arr[x, y, z].name = (x + "," + y + "," + z);
>>>>>>> origin/board-generation-beautifying
        if (isInvisible)
        {
            Debug.Log("Placed block at (" + x + "," + y + "," + z + ") marked as NOT built.");
            IsBuilt_Arr[x, y, z] = false;
        }
        else
        {
            Debug.Log("Placed block at (" + x + "," + y + "," + z + ") marked as built.");
            IsBuilt_Arr[x, y, z] = true;
        }
    }

    /// <summary>
    /// Gets an int representing the value for the is built status of the blocks to the left right front back and above of a specified coordinate
    /// </summary>
    /// <param name="x">x coordinate</param>
    /// <param name="y">y coordinate</param>
    /// <param name="z">z coordinate</param>
    public int GetBlockNeighborsBuiltValue(int x, int y, int z)
    {
        int neighborValue = 0;
        Boolean[] neighbors = new Boolean[4];
        if (z + 1 < BaseSize)
        {
            if (IsBuilt_Arr[x, y, z + 1]) neighborValue += 1; //Front Block value of 1
        }
        if (z - 1 >= 0) {
            if (IsBuilt_Arr[x, y, z - 1]) neighborValue += 2; //Behind Block value of 2
        }
        if (x + 1 < BaseSize)
        {
            if (IsBuilt_Arr[x + 1, y, z]) neighborValue += 3; //Left Block value of 7
        }
        if (x - 1 >= 0)
        {
            if (IsBuilt_Arr[x - 1, y, z]) neighborValue += 4; //Right Block value of 10
        }
        if (y + 1 < HeightSize)
        {
            if (IsBuilt_Arr[x, y + 1, z]) neighborValue += 5; //Above Block value of 13
        }
        //We dont need bottom block
        return neighborValue;
    }

    /// <summary>
    /// Updates a block depending on its neighbors
    /// </summary>
    /// <param name="x">x coordinate</param>
    /// <param name="y">y coordinate</param>
    /// <param name="z">z coordinate</param>
    public void UpdateBlock(int x, int y, int z)
    {
        return;
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
