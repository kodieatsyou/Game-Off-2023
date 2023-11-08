using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateBoard : MonoBehaviour
{
    public int XSize = 10;
    public int YSize = 10;
    public int ZSize = 10;
    public GameObject TopBlock;
    public GameObject BaseBlock;
    public GameObject InvisibleBlock;
    public static GameObject[,,] CubeArr;
    public static bool[,,] IsBuildableArr; // marks block coordinates as buildable
    public static bool[,,] IsBuiltArr; // marks block coordinates as built

    // Start is called before the first frame update
    void Start()
    {
        IsBuildableArr = new bool[XSize, YSize, ZSize];
        IsBuiltArr = new bool[XSize, YSize, ZSize];
        CubeArr = new GameObject[XSize, YSize, ZSize];

        for (int x = 0; x < XSize; x++)
        {
            for (int y = 0; y < YSize; y++)
            {
                for (int z = 0; z < ZSize; z++)
                {
                    PlaceCube(x, y, z);
                }
            }
        }
        GenerateRandom();
    }
    // Update is called once per frame
    void Update()
    {
        // need to update isbuildable, isbuilt, game object asset of whatever changed.
    }

    /// <summary>
    /// Places a GameObject of cube at a specified coordinates. Treats levels of the cube differently for different Cube GameObjects
    /// </summary>
    /// <param name="x">x coordinate</param>
    /// <param name="y">y coordinate</param>
    /// <param name="z">z coordinate</param>
    void PlaceCube(int x, int y, int z)
    {
        Debug.Log("X: " + x);
        Debug.Log("Y: " + y);
        Debug.Log("Z: " + z);

        if (y == 0)
        {
            Debug.Log("Placing Base Block");
            CubeArr[x, y, z] = (GameObject)Instantiate(BaseBlock, new Vector3(x * 2, y * 2, z * 2), transform.rotation);
            IsBuildableArr[x, y, z] = true;
            IsBuiltArr[x, y, z] = true;
        }
        else if (y >= 1)
        {
            Debug.Log("Placing Invisible Block");
            CubeArr[x, y, z] = (GameObject)Instantiate(InvisibleBlock, new Vector3(x * 2, y * 2, z * 2), transform.rotation);
            IsBuildableArr[x, y, z] = false;
            IsBuiltArr[x, y, z] = false;
        }
        else
        {
            Debug.Log("Placing Other Block");
            CubeArr[x, y, z] = (GameObject)Instantiate(TopBlock, new Vector3(x * 2, y * 2, z * 2), transform.rotation);
            IsBuildableArr[x, y, z] = false;
            IsBuiltArr[x, y, z] = false;
        }
    }

    /// <summary>
    /// Uses the current IsBuildableArr status and given coordinates to determine if the block at said coordinates can be built.
    /// Only condition where buildable is when the block below has been built, there is no player on the current block, and the coordinates are valid
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <returns>Boolean representing if the block is buildable or not. True for the player can build, false for not.</returns>
    public bool isBuildable(int x, int y, int z)
    {
        if (IsBuiltArr[x, y, z]) //already built blocks are never buildable
        {
            return false;
        }
        else if ((!IsBuiltArr[x, y-1, z]) && (y-1 > 0) && (y < YSize))
        {
            return true;
        }
        else
        {
            return false; // default return false
        }
    }
    /// <summary>
    ///  Generates a random series of blocks on top of the ground level game board to help game get started.
    /// </summary>
    void GenerateRandom()
    {
        
    }

}
