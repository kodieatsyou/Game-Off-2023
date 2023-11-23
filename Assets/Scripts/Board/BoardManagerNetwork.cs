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

public class BoardManagerNetwork : MonoBehaviourPunCallbacks, IPunInstantiateMagicCallback
{
    public static BoardManagerNetwork Instance;

    public bool OfflineMode = false;

    public int BaseSize = 10;
    public int RandomBlockScale = 4; // pick scale of random blocks to the board size. Ex: 4 means BaseSize * 4 = 40 random blocks
    public int HeightSize;
    private static int RandomBlockCount;
    public static BoardSpaceNetwork[,,] BoardSpace_Arr;
    public int yOfCurrentHeighestBuiltBlock = 0;
    public PhotonView BMPhotonView;

    public int numPlayersBoardsDoneInitializing;
    public List<int> playerIDsWhoHaveInitialized;
    public List<BoardSpaceNetwork> potentialSpawnLocations = new List<BoardSpaceNetwork>();

    public GameObject[] playersOnBoard;

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        if (Instance == null)
        {
            Instance = this;
            if (GetComponent<PhotonView>() != null)
            {
                BMPhotonView = GetComponent<PhotonView>();
            }
            else
            {
                transform.AddComponent<PhotonView>();
                BMPhotonView.OwnershipTransfer = OwnershipOption.Takeover;
            }
        }
        else
        {
            Destroy(gameObject);
        }

        numPlayersBoardsDoneInitializing = 0;
        playerIDsWhoHaveInitialized = new List<int>();

        HeightSize = BaseSize * 2;
        RandomBlockCount = BaseSize * RandomBlockScale;

        bool[,,] isRandom = GenerateRandomGrid();

        string[] initialBoardSpaceArray = new string[BaseSize * HeightSize * BaseSize];
        BoardSpace_Arr = new BoardSpaceNetwork[BaseSize, HeightSize, BaseSize];
        for (int x = 0; x < BaseSize; x++)
        {
            for (int y = 0; y < HeightSize; y++)
            {
                for (int z = 0; z < BaseSize; z++)
                {
                    int index = x + BaseSize * (y + HeightSize * z);
                    if (isRandom[x, y, z])
                    {
                        BoardSpaceNetwork newSpace = new BoardSpaceNetwork(new Vector3(x, y, z), true);
                        initialBoardSpaceArray[index] = newSpace.ToJson();
                        BoardSpace_Arr[x, y, z] = newSpace;
                    }
                    else if (y == 0)
                    {
                        BoardSpaceNetwork newSpace = new BoardSpaceNetwork(new Vector3(x, y, z), true);
                        initialBoardSpaceArray[index] = newSpace.ToJson();
                        BoardSpace_Arr[x, y, z] = newSpace;
                        potentialSpawnLocations.Add(newSpace);
                    }
                    else
                    {
                        BoardSpaceNetwork newSpace = new BoardSpaceNetwork(new Vector3(x, y, z), false);
                        initialBoardSpaceArray[index] = newSpace.ToJson();
                        BoardSpace_Arr[x, y, z] = newSpace;
                    }
                }
            }
        }
        PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "InitialBoard", initialBoardSpaceArray }, { "BoardHeightSize", HeightSize }, { "BoardBaseSize", BaseSize } });
        initialBoardSpaceArray = null;
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

    public void PlacePlayerOnViableSpawnPosition(int id)
    {
        List<BoardSpaceNetwork> viableSpawns = new List<BoardSpaceNetwork>();
        foreach(BoardSpaceNetwork b in potentialSpawnLocations)
        {
            if (!BoardSpace_Arr[(int)b.posInBoard.x, (int)(b.posInBoard.y + 1), (int)b.posInBoard.z].isBuilt)
            {
                viableSpawns.Add(b);
                //Debug.Log("Viable spawn found at location: " + b.posInBoard);
            }
        }
        System.Random rand = new System.Random();
        int randomIndex = rand.Next(0, viableSpawns.Count);
        BoardSpaceNetwork spawnSpace = viableSpawns[randomIndex];
        viableSpawns.RemoveAt(randomIndex);
        spawnSpace.playerIDOnSpace = id;
        Debug.Log("Spawning player on block: " + spawnSpace.posInBoard);
        PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "BoardChanges", spawnSpace.ToJson() }, { "BoardChanges-FromPlayer", -1 } });
        playersOnBoard[id] = PhotonNetwork.InstantiateRoomObject("NetworkObjects/Player", spawnSpace.GetWorldPositionOfTopOfSpace(), Quaternion.identity);
        playersOnBoard[id].transform.SetParent(transform, true);
        playersOnBoard[id].transform.name = "Player: " + (id + 1);
        playersOnBoard[id].GetComponent<PlayerControllerNetwork>().SetupPlayer(id + 1);
    }

    [PunRPC]
    public void RPCBoardManagerBoardInitialized(int playerID)
    {
        if (!playerIDsWhoHaveInitialized.Contains(playerID))
        {
            playerIDsWhoHaveInitialized.Add(playerID);
        }
        Debug.Log("Initialized Player Count: " +  playerIDsWhoHaveInitialized.Count + " all player count: " + PhotonNetwork.CurrentRoom.PlayerCount);
        if(playerIDsWhoHaveInitialized.Count == PhotonNetwork.CurrentRoom.PlayerCount)
        {
            Debug.Log("All players have initialized!");
            //PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "InitialBoard", null }, { "BoardHeightSize", null }, { "BoardBaseSize", null } });
            playersOnBoard = new GameObject[PhotonNetwork.CurrentRoom.PlayerCount];
            for (int i = 0; i < PhotonNetwork.CurrentRoom.PlayerCount; i++)
            {
                PlacePlayerOnViableSpawnPosition(i);
            }
        }
    }

    [PunRPC]
    public void RPCBoardManagerPushChangesFromLocalBoard(string blockChanged, int playerID)
    {
        //Debug.Log("Changed recieved from player: " + playerID);
        if (playerIDsWhoHaveInitialized.Count == PhotonNetwork.CurrentRoom.PlayerCount)
        {
            //Debug.Log("Updating board on network");
            BoardSpaceNetwork changedSpace = new BoardSpaceNetwork(blockChanged);
            BoardSpace_Arr[(int)changedSpace.posInBoard.x, (int)changedSpace.posInBoard.y, (int)changedSpace.posInBoard.z].isBuilt = changedSpace.isBuilt;

            PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "BoardChanges", blockChanged }, { "BoardChanges-FromPlayer", playerID } });
            //Debug.Log("Pushed changes from player: " + playerID + " to props");
        }
    }
}
