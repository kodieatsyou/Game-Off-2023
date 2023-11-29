using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BoardManager : MonoBehaviourPunCallbacks
{
    public static BoardManager Instance;

    public PhotonView BMPhotonView;
    public int baseSize;
    public int heightSize;
    public int randomBlockScale = 4;
    private static int randomBlockCount;
    public bool[,,] board;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            PhotonNetwork.Destroy(gameObject);
        }
        BMPhotonView = GetComponent<PhotonView>();
    }

    private void Start()
    {
        board = new bool[baseSize, heightSize, baseSize];
        //Allow master client to make initial board
        if (PhotonNetwork.IsMasterClient)
        {
            randomBlockCount = baseSize * randomBlockScale;
            bool[,,] isRandom = GenerateRandomGrid();
            for (int x = 0; x < baseSize; x++)
            {
                for (int y = 0; y < heightSize; y++)
                {
                    for (int z = 0; z < baseSize; z++)
                    {
                        if (isRandom[x, y, z])
                        {
                            board[x, y, z] = true;
                        }
                        else if (y == 0)
                        {
                            board[x, y, z] = true;
                        }
                        else
                        {
                            board[x, y, z] = false;
                        }
                    }
                }
            }
            BMPhotonView.RPC("RPCBoardManagerInitializeClientBoards", RpcTarget.AllBuffered, FlattenBoardArray(board), baseSize, heightSize);
        }
        StartCoroutine(SpawnPlayer());
    }

    bool[] FlattenBoardArray(bool[,,] inflatedArray)
    {
        bool[] flattenedArray = new bool[baseSize * heightSize * baseSize];
        int index = 0;

        for (int i = 0; i < baseSize; i++)
        {
            for (int j = 0; j < heightSize; j++)
            {
                for (int k = 0; k < baseSize; k++)
                {
                    flattenedArray[index++] = inflatedArray[i, j, k];
                }
            }
        }

        return flattenedArray;
    }

    bool[,,] InflateBoardArray(bool[] flattenedArray)
    {
        bool[,,] inflatedArray = new bool[baseSize, heightSize, baseSize];

        for (int i = 0; i < baseSize; i++)
        {
            for (int j = 0; j < heightSize; j++)
            {
                for (int k = 0; k < baseSize; k++)
                {
                    inflatedArray[i, j, k] = flattenedArray[i * heightSize * baseSize + j * baseSize + k];
                }
            }
        }

        return inflatedArray;
    }

    bool[,,] GenerateRandomGrid()
    {
        bool[,,] randomCoords = new bool[baseSize, heightSize, baseSize];
        for (int x = 0; x < baseSize; x++) // preload randomCoords with false
        {
            for (int y = 0; y < heightSize; y++)
            {
                for (int z = 0; z < baseSize; z++)
                {
                    randomCoords[x, y, z] = false;
                }
            }
        }

        System.Random rand = new System.Random();
        int count = 0;
        while (count < randomBlockCount) // create randomBlockCount random x and z set of coordinates
        {
            int randX = rand.Next(0, baseSize);
            int randZ = rand.Next(0, baseSize);
            while (randomCoords[randX, 1, randZ]) // while no collision
            {
                randX = rand.Next(0, baseSize);
                randZ = rand.Next(0, baseSize);
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

    IEnumerator SpawnPlayer()
    {
        yield return new WaitUntil(() => Board.Instance.boardArray != null);

        System.Random random = new System.Random();
        BoardSpace currentPos = Board.Instance.boardArray[0, 0, 0];
        bool foundSpawn = false;
        while (!foundSpawn)
        {
            currentPos = Board.Instance.boardArray[random.Next(0, baseSize - 1), 0, random.Next(0, baseSize - 1)];

            if(currentPos.GetPlayerOnSpace() == null && !Board.Instance.boardArray[(int)currentPos.GetPosInBoard().x, 1, (int)currentPos.GetPosInBoard().z].GetIsBuilt())
            {
                foundSpawn = true;
            }
            yield return null;
        }

        Vector3 pos = currentPos.GetWorldPositionOfTopOfSpace();
        GameObject player = PhotonNetwork.Instantiate("NetworkObjects/Player", pos, Quaternion.identity);
        BMPhotonView.RPC("RPCBoardManagerPlacePlayerOnSpace", RpcTarget.All, PhotonNetwork.LocalPlayer, currentPos.GetPosInBoard());
    }

    [PunRPC]
    void RPCBoardManagerPlacePlayerOnSpace(Player player, Vector3 posOfSpaceToPutOn)
    {
        if(player == PhotonNetwork.LocalPlayer)
        {
            PlayerController.Instance.currentSpace = Board.Instance.boardArray[(int)posOfSpaceToPutOn.x, (int)posOfSpaceToPutOn.y, (int)posOfSpaceToPutOn.z];
        }
        Board.Instance.boardArray[(int)posOfSpaceToPutOn.x, (int)posOfSpaceToPutOn.y, (int)posOfSpaceToPutOn.z].PlacePlayerOnSpace(player);
    }

    [PunRPC]
    void RPCBoardManagerInitializeClientBoards(bool[] initialFlattenedBoard, int initialHeightSize, int initialBaseSize)
    {
        Debug.Log("Instantiating");
        baseSize = initialBaseSize;
        heightSize = initialHeightSize;
        board = InflateBoardArray(initialFlattenedBoard);
        Instantiate(GameAssets.i.board_, Vector3.zero, Quaternion.identity).GetComponent<Board>().InitializeBoard(board, baseSize, heightSize);
    }

    [PunRPC]
    public void BoardManagerSetSpaceIsBuilt(Vector3 pos, bool newValue)
    {
        board[(int)pos.x, (int)pos.y, (int)pos.z] = newValue;
        Board.Instance.boardArray[(int)pos.x, (int)pos.y, (int)pos.z].SetIsBuilt(newValue);
    }

    [PunRPC]
    public void RPCBoardManagerDoWind(WindDir dir)
    {
        Debug.Log("MAKING WIND PARTICLES");
        Instantiate(GameAssets.i.wind_particle_.GetComponent<WindParticle>().InitializeWindParticles(3f, dir));
        PlayerController.Instance.GetPushedByWind(dir);
    }
}
