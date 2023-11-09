using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static int BaseSize = 10;
    private static int HeightSize = BaseSize * 2;
    public static int randomBlockCount = 10;
    public GameObject TopBlock_Prefab;
    public GameObject BaseBlock_Prefab;
    public GameObject InvisibleBlock_Prefab;
    public static GameObject[,,] BoardCube_Arr;
    public static bool[,,] CanBuildOn_Arr; // marks block coordinates as buildable
    public static bool[,,] IsBuilt_Arr; // marks block coordinates as built

    // Start is called before the first frame update
    void Start()
    {
        CanBuildOn_Arr = new bool[BaseSize, BaseSize, HeightSize];
        IsBuilt_Arr = new bool[BaseSize, BaseSize, HeightSize];
        BoardCube_Arr = new GameObject[BaseSize, BaseSize, HeightSize];

        bool[,,] isRandom = GenerateRandom();

        for (int b = 0; b < BaseSize; b++) // base(b) counts both x and z
        {
            for (int y = 0; y < HeightSize; y++)
            {
                if (isRandom[b, y, b])
                {
                    Debug.Log("Placing Randomized Block");
                    PlaceCube(BaseBlock_Prefab, b, y, b);
                    CanBuildOn_Arr[b, y, b] = true;

                }
                else if (y == 0)
                {
                    Debug.Log("Placing Base Block");
                    PlaceCube(BaseBlock_Prefab, b, y, b);
                    CanBuildOn_Arr[b, y, b] = true;
                }
                else if (y >= 1)
                {
                    Debug.Log("Placing Invisible Block");
                    placeCube(InvisibleBlock_Prefab, b, y, b, true);
                    CanBuildOn_Arr[b, y, b] = false;
                }
                else
                {
                    Debug.Log("Placing Other Block");
                    placeCube(TopBlock_Prefab, b, y, b);
                    CanBuildOn_Arr[b, y, b] = false;
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
        BoardCube_Arr[x, y, z] = (GameObject)Instantiate(prefab_type, new Vector3(x * 2, y * 2, z * 2), transform.rotation);
        if (isInvisible)
        {
            Debug.Log("Placed block at (" + x + "," + y + "," + z + ")");
            IsBuilt_Arr[x, y, z] = false;
        }
        else
        {
            Debug.Log("Placed block at (" + x + "," + y + "," + z + ") marked as built");
            IsBuilt_Arr[x, y, z] = true;
        }
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
        else if ((!IsBuilt_Arr[x, y - 1, z]) && (y - 1 > 0) && (y < YSize))
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
    /// </summary>
    bool[,,] GenerateRandom()
    {
        System.Random rand = new System.Random();

        bool[,,] randomCoords = new bool[,,];

        int count = 0;
        while (count < randomBlockCount) // create randomBlockCount random x and z set of coordinates
        {
            int randX = rand.Next(0, XSize);
            int randZ = rand.Next(0, ZSize);

            randomCoords[count] = true;

            count++;
        }

        return randomCoords;

    }

}
