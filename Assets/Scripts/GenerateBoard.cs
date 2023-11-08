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
    public static bool[,,] IsInvisibleArr; // tracks the blocks that are invisible for easier calculations

    // Start is called before the first frame update
    void Start()
    {
        IsInvisibleArr = new bool[XSize, YSize, ZSize];

        CubeArr = new GameObject[XSize, YSize, ZSize];
        for (int x = 0; x < XSize; x++)
        {
            for (int y = 0; y < YSize; y++)
            {
                for (int z = 0; z < ZSize; z++)
                {
                    PlaceCube(CubeArr, IsInvisibleArr, x, y, z);
                }
            }
        }
        GenerateRandom();
    }

    /// <summary>
    /// Places a GameObject of cube at a specified coordinates. Treats levels of the cube differently for different Cube GameObjects
    /// </summary>
    /// <param name="CubeArr">The manager of the cubearray in 3d array format</param>
    /// <param name="x">x coordinate</param>
    /// <param name="y">y coordinate</param>
    /// <param name="z">z coordinate</param>
    void PlaceCube(GameObject[,,] CubeArr, bool[,,] IsInvisibleArr, int x, int y, int z)
    {
        Debug.Log("X: " + x);
        Debug.Log("Y: " + y);
        Debug.Log("Z: " + z);

        if (y == 0)
        {
            Debug.Log("Placing Base Block");
            CubeArr[x, y, z] = (GameObject)Instantiate(BaseBlock, new Vector3(x * 2, y * 2, z * 2), transform.rotation);
            IsInvisibleArr[x, y, z] = false;
        }
        else if (y >= 1)
        {
            Debug.Log("Placing Invisible Block");
            CubeArr[x, y, z] = (GameObject)Instantiate(InvisibleBlock, new Vector3(x * 2, y * 2, z * 2), transform.rotation);
            IsInvisibleArr[x, y, z] = true;
        }
        else
        {
            Debug.Log("Placing Other Block");
            CubeArr[x, y, z] = (GameObject)Instantiate(TopBlock, new Vector3(x * 2, y * 2, z * 2), transform.rotation);
            IsInvisibleArr[x, y, z] = false;
        }
    }

    /// <summary>
    ///  Generates a random series of blocks on top of the ground level game board to help game get started.
    /// </summary>
    void GenerateRandom()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
