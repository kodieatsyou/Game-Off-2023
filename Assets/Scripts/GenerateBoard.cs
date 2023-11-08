using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateBoard : MonoBehaviour
{
    public int XSize = 10;
    public int YSize = 10;
    public int ZSize = 10;
    public GameObject TopBlock;
    public GameObject BottomBlock;
    public static GameObject[,,] CubeArray;

    // Start is called before the first frame update
    void Start()
    {
        CubeArray = new GameObject[XSize, YSize, ZSize];
        for (int x = 0; x < XSize; x++)
        {
            for (int y = 0; y < YSize; y++)
            {
                for (int z = 0; z < ZSize; z++)
                {
                    Debug.Log("X: " + x);
                    Debug.Log("Y: " + y);
                    Debug.Log("Z: " + z);
                    CubeArray[x, y, z] = (GameObject)Instantiate(TopBlock, new Vector3(x * 2, y * 2, z * 2), transform.rotation);
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
