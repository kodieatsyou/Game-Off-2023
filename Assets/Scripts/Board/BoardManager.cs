using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using Unity.VisualScripting;
using static UnityEngine.RuleTile.TilingRuleOutput;
using System.Linq;

/// <summary>
/// Will only be attached to the initial board object!
///  This script is responsible for generating the board, initializing the spaces, and setting the board mode to: build, move, or select player
///  Build Mode: Sets board spaces to selectable if they are able to be built
///  Move Mode: Sets board spaces to selectable if they are able to be moved from
///  Player Mode: Sets board spaces to selectable if they have a player on them
/// </summary>
/// 

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;

    public bool OfflineMode = false;

    public int BaseSize = 10;
    public int RandomBlockScale = 4; // pick scale of random blocks to the board size. Ex: 4 means BaseSize * 4 = 40 random blocks
    public int HeightSize;
    private static int RandomBlockCount;
    public static string[] BoardSpace_Arr;
    public int yOfCurrentHeighestBuiltBlock = 0;
    public PhotonView BMPhotonView;

    public bool[] playersBoardsDoneInitializing;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            if(GetComponent<PhotonView>() != null)
            {
                BMPhotonView = GetComponent<PhotonView>();
            } else
            {
                transform.AddComponent<PhotonView>();
                BMPhotonView.OwnershipTransfer = OwnershipOption.Takeover;
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.OfflineMode = OfflineMode;

        playersBoardsDoneInitializing = new bool[PhotonNetwork.CurrentRoom.PlayerCount];

        HeightSize = BaseSize * 2;
        RandomBlockCount = BaseSize * RandomBlockScale;

        bool[,,] isRandom = GenerateRandomGrid();

        BoardSpace_Arr = new string[BaseSize * HeightSize * BaseSize];
        for (int x = 0; x < BaseSize; x++)
        {
            for (int y = 0; y < HeightSize; y++)
            {
                for (int z = 0; z < BaseSize; z++)
                {
                    int index = x + BaseSize * (y + HeightSize * z);
                    if (isRandom[x, y, z])
                    {
                        BoardSpace_Arr[index] = new BoardSpaceNetwork(new Vector3(x, y, z), 2.5f, true).ToJson();
                    }
                    else if (y == 0)
                    {
                        BoardSpace_Arr[index] = new BoardSpaceNetwork(new Vector3(x, y, z), 2.5f, true).ToJson();
                    }
                    else
                    {
                        BoardSpace_Arr[index] = new BoardSpaceNetwork(new Vector3(x, y, z), 2.5f, false).ToJson();
                    }
                }
            }
        }
        PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "InitialBoard", BoardSpace_Arr }, { "BoardHeightSize", HeightSize }, { "BoardBaseSize", BaseSize } });
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

    /*public BoardSpace GetViableSpawnPosition()
    {
        List<BoardSpace> viableLocations = new List<BoardSpace>();
        for (int x = 0; x < BaseSize; x++)
        {
            for (int y = 0; y < 1; y++)
            {
                for (int z = 0; z < BaseSize; z++)
                {
                    if(BoardSpace_Arr[x, y, z] != null)
                    {
                        BoardSpace space = BoardSpace_Arr[x, y, z].GetComponent<BoardSpace>();
                        if(space.GetIsBuilt() && space.GetValueOfNeighbors() % 11 != 0 && space.GetPlayerOnSpace() == null)
                        {
                            viableLocations.Add(space);
                        }
                    }
                }
            }
        }
        System.Random rand = new System.Random();
        return viableLocations[rand.Next(0, viableLocations.Count)];
    }*/

    [PunRPC]
    public void RPCBoardManagerBoardInitialized(int playerID)
    {
        playersBoardsDoneInitializing[playerID - 1] = true;
        if(!playersBoardsDoneInitializing.Contains(false))
        {
            Debug.Log("All players have initialized!");
            PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "InitialBoard", null }, { "BoardHeightSize", null }, { "BoardBaseSize", null } });
        }
    }

    [PunRPC]
    public void RPCBoardManagerPushChangesFromLocalBoard(string blocksChanged, int playerID)
    {
        string[] changes = blocksChanged.Split('~');
        PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "BoardChanges", changes }, { "BoardChanges-FromPlayer", playerID } });
    }


}
