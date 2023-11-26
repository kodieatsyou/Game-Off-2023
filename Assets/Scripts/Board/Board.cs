using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SelectionMode
{
    Build,
    Move,
    Player,
    None
}

public class Board : MonoBehaviour
{
    public static Board Instance;
    public BoardSpace[,,] boardArray;
    public int baseSize;
    public int heightSize;
    public int yOfCurrentHeighestBuiltBlock = 0;
    public SelectionMode selectionMode;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        
    }

    public void InitializeBoard(bool[,,] initialBoard, int baseSize, int heightSize)
    {
        selectionMode = SelectionMode.None;
        boardArray = new BoardSpace[baseSize, heightSize, baseSize];
        this.baseSize = baseSize;
        this.heightSize = heightSize;
        for (int x = 0; x < baseSize; x++)
        {
            for (int y = 0; y < heightSize; y++)
            {
                for (int z = 0; z < baseSize; z++)
                {
                    boardArray[x, y, z] = Instantiate(GameAssets.i.board_space_object_, transform).GetComponent<BoardSpace>().InitializeSpace(new Vector3(x, y, z), 2.5f, initialBoard[x, y, z]);
                }
            }
        }

        CameraController.Instance.InitializeCamera();
    }

    public Vector3 GetBoardMiddlePosAtYLevel(int yLevel)
    {
        if (Mathf.Floor(baseSize / 2.0f) == baseSize / 2.0f) //Check if the middle of the grid is a float or not. If its a whole number the middle is between 2 blocks else the middle is a whole block. 
        {
            return new Vector3(((baseSize / 2.0f) * 2.5f) - 1, yLevel * 2.5f, ((baseSize / 2.0f) * 2.5f) - 1); // Subtract one to get the location between the two "middle" blocks

        }
        else
        {
            return new Vector3((baseSize / 2.0f) * 2.5f, yLevel * 2.5f, (baseSize / 2.0f) * 2.5f); // Get the location of the center of the middle block
        }
    }
}
