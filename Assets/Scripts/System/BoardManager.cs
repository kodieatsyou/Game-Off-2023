using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
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
    }

    private void Start()
    {
        BMPhotonView = GetComponent<PhotonView>();
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
            SpawnPlayers();

        }
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

    void SpawnPlayers()
    {
        bool[,] spawns = new bool[baseSize, baseSize];
        for (int x = 0; x < baseSize; x++) // preload randomCoords with false
        {
            for (int z = 0; z < baseSize; z++)
            {
                if (board[x, 0, z] && !board[x, 1, z])
                {
                    spawns[x, z] = true;
                } else
                {
                    spawns[x, z] = false;
                }
            }
        }

        StartCoroutine(FindSpawn(spawns));
    }

    IEnumerator FindSpawn(bool[,] viableSpawningLocations)
    {
        Vector2[] spawns = new Vector2[PhotonNetwork.CurrentRoom.PlayerCount];

        for(int i = 0; i < PhotonNetwork.CurrentRoom.PlayerCount; i++)
        {
            Vector3 pos = new Vector3(0, 0, 0);
            Vector2 currentPos = new Vector2(0, 0);
            System.Random random = new System.Random();
            while (!viableSpawningLocations[(int)currentPos.x, (int)currentPos.y])
            {
                currentPos = new Vector2(random.Next(0, baseSize - 1), random.Next(0, baseSize - 1));
                yield return null;
            }
            spawns[i] = currentPos;
        }
        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable
        {
            { "spawns", spawns }
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if(propertiesThatChanged.ContainsKey("spawns"))
        {
            //Get spawns from room props
            Vector2[] allSpawns = (Vector2[])PhotonNetwork.CurrentRoom.CustomProperties["spawns"];
            //Get this players spawn location
            Vector2 spawnLocation = allSpawns[PhotonNetwork.LocalPlayer.ActorNumber - 1];

            //Spawn the player
            Vector3 pos = Board.Instance.boardArray[(int)spawnLocation.x, 0, (int)spawnLocation.y].GetWorldPositionOfTopOfSpace();
            GameObject player = PhotonNetwork.Instantiate("NetworkObjects/Player", pos, Quaternion.identity);
            player.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.LocalPlayer);
            player.GetComponent<PhotonView>().RPC("RPCPlayerManagerPlayerMovedToSpace", RpcTarget.All, new Vector3(spawnLocation.x, 0, spawnLocation.y));
        }
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
}
