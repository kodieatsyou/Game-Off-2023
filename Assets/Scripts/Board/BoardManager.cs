using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

/// <summary>
/// Will only be attached to the initial board object!
///  This script is responsible for generating the board, initializing the spaces, and setting the board mode to: build, move, or select player
///  Build Mode: Sets board spaces to selectable if they are able to be built
///  Move Mode: Sets board spaces to selectable if they are able to be moved from
///  Player Mode: Sets board spaces to selectable if they have a player on them
/// </summary>

public enum SelectionMode
{
    None,
    Build,
    Move,
    Player
}

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;

    public int BaseSize = 10;
    public int RandomBlockScale = 4; // pick scale of random blocks to the board size. Ex: 4 means BaseSize * 4 = 40 random blocks
    public int HeightSize;
    private static int RandomBlockCount;
    public static BoardSpace[,,] BoardSpace_Arr;
    public int yOfCurrentHeighestBuiltBlock = 0;
    public SelectionMode selectionMode;

    private void Awake()
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

    // Start is called before the first frame update
    void Start()
    {
        //PhotonNetwork.OfflineMode = true;
        HeightSize = BaseSize * 2;
        RandomBlockCount = BaseSize * RandomBlockScale;

        BoardSpace_Arr = new BoardSpace[BaseSize, HeightSize, BaseSize];

        selectionMode = SelectionMode.None;

        bool[,,] isRandom = GenerateRandomGrid();

        // Fill CanBuildOn_Arr and IsBuilt_Arr with rule-based values
        for (int x = 0; x < BaseSize; x++)
        {
            for (int y = 0; y < HeightSize; y++)
            {
                for (int z = 0; z < BaseSize; z++)
                {
                    if (isRandom[x, y, z])
                    {
                        BoardSpace_Arr[x, y, z] = Instantiate(GameAssets.i.test_Space_Object_, transform).GetComponent<BoardSpace>().InitializeSpace(new Vector3(x, y, z), 2.5f, true);
                    }
                    else if (y == 0)
                    {
                        BoardSpace_Arr[x, y, z] = Instantiate(GameAssets.i.test_Space_Object_, transform).GetComponent<BoardSpace>().InitializeSpace(new Vector3(x, y, z), 2.5f, true);
                    }
                    else
                    {
                        BoardSpace_Arr[x, y, z] = Instantiate(GameAssets.i.test_Space_Object_, transform).GetComponent<BoardSpace>().InitializeSpace(new Vector3(x, y, z), 2.5f, false);
                    }
                }
            }
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

    public Vector3 GetBoardMiddlePosAtYLevel(int yLevel)
    {
        if (Math.Floor(BaseSize / 2.0f) == BaseSize / 2.0f) //Check if the middle of the grid is a float or not. If its a whole number the middle is between 2 blocks else the middle is a whole block. 
        {
            return new Vector3(((BaseSize / 2.0f) * 2.5f) - 1, yLevel * 2.5f, ((BaseSize / 2.0f) * 2.5f) - 1); // Subtract one to get the location between the two "middle" blocks

        }
        else
        {
            return new Vector3((BaseSize / 2.0f) * 2.5f, yLevel * 2.5f, (BaseSize / 2.0f) * 2.5f); // Get the location of the center of the middle block
        }
    }

    public BoardSpace GetViableSpawnPosition()
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
    }
}
