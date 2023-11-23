using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.CullingGroup;

public enum SelectionMode
{
    None,
    Build,
    Move,
    Player
}
public class LocalBoardManager : MonoBehaviourPunCallbacks
{
    public static LocalBoardManager Instance;
    public int BaseSize;
    public int HeightSize;
    public static BoardSpace[,,] BoardSpace_Arr;
    public int yOfCurrentHeighestBuiltBlock = 0;
    public SelectionMode selectionMode;
    public bool initialized = false;
    public bool syncing = false;

    public List<BoardSpace> spacesChanged = new List<BoardSpace>();

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

    private void Start()
    {
        selectionMode = SelectionMode.Move;
    }

    private void Update()
    {
        if(spacesChanged.Count > 0)
        {
            PushSyncFromLocalBoard();
        }
    }

    private void LateUpdate()
    {
        //spacesChanged.Clear();
    }

    public void InitializeBoard()
    {
        StartCoroutine(InitializeBoardCoroutine());
    }

    private IEnumerator InitializeBoardCoroutine()
    {
        if (!initialized)
        {
            string[] networkBoardSpaceArr = (string[])PhotonNetwork.CurrentRoom.CustomProperties["InitialBoard"];
            BaseSize = (int)PhotonNetwork.CurrentRoom.CustomProperties["BoardBaseSize"];
            HeightSize = (int)PhotonNetwork.CurrentRoom.CustomProperties["BoardHeightSize"];
            BoardSpace_Arr = new BoardSpace[BaseSize, HeightSize, BaseSize];

            for (int x = 0; x < BaseSize; x++)
            {
                for (int y = 0; y < HeightSize; y++)
                {
                    for (int z = 0; z < BaseSize; z++)
                    {
                        int index = x + BaseSize * (y + HeightSize * z);
                        string jsonString = networkBoardSpaceArr[index];
                        if (!string.IsNullOrEmpty(jsonString))
                        {
                            BoardSpaceNetwork networkBoardSpace = BoardSpaceNetwork.FromJson(networkBoardSpaceArr[index]);
                            Debug.Log("Index: " + index + " Position: " + networkBoardSpace.posInBoard);
                            BoardSpace_Arr[x, y, z] = Instantiate(GameAssets.i.test_Space_Object_, transform).GetComponent<BoardSpace>().InitializeSpace(networkBoardSpace.posInBoard, networkBoardSpace.worldSpaceScalingFactor, networkBoardSpace.isBuilt);
                            // Place player on space if one exists
                        }
                    }
                }
            }
            BoardManager.Instance.BMPhotonView.RPC("RPCBoardManagerBoardInitialized", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber);
        }

        yield return null; // Wait for the next frame before continuing

        initialized = true;
        spacesChanged.Clear();
    }

    public void PullChangesFromNetworkedBoard()
    {
        string[] changes = (string[])PhotonNetwork.CurrentRoom.CustomProperties["BoardChanges"];
        foreach (string change in changes)
        {
            BoardSpaceNetwork networkBoardSpace = BoardSpaceNetwork.FromJson(change);
            Debug.Log("Pos changed: " + (int)networkBoardSpace.posInBoard.x + ", " + (int)networkBoardSpace.posInBoard.y + ", " + (int) networkBoardSpace.posInBoard.z);
            BoardSpace_Arr[(int)networkBoardSpace.posInBoard.x, (int)networkBoardSpace.posInBoard.y, (int)networkBoardSpace.posInBoard.z].SetIsBuilt(networkBoardSpace.isBuilt);
            BoardSpace_Arr[(int)networkBoardSpace.posInBoard.x, (int)networkBoardSpace.posInBoard.y, (int)networkBoardSpace.posInBoard.z].SetWorldSpaceScalingFactor(networkBoardSpace.worldSpaceScalingFactor);
        }
    }

    public void PushSyncFromLocalBoard()
    {
        Debug.Log("Pushing " + spacesChanged.Count + " changes");
        string spacesChangedString = "";
        foreach (BoardSpace b in spacesChanged)
        {
            Debug.Log("SPACE: " + b.GetPosInBoard() + " was changed!");
            spacesChangedString += new BoardSpaceNetwork(b.GetPosInBoard(), b.GetWorldSpaceScalingFactor(), b.GetIsBuilt()).ToJson() + "~";
        }
        BoardManager.Instance.BMPhotonView.RPC("RPCBoardManagerPushChangesFromLocalBoard", RpcTarget.All, spacesChangedString, PhotonNetwork.LocalPlayer.ActorNumber);
        spacesChanged.Clear();
    }

    public Vector3 GetBoardMiddlePosAtYLevel(int yLevel)
    {
        if (Mathf.Floor(BaseSize / 2.0f) == BaseSize / 2.0f) //Check if the middle of the grid is a float or not. If its a whole number the middle is between 2 blocks else the middle is a whole block. 
        {
            return new Vector3(((BaseSize / 2.0f) * 2.5f) - 1, yLevel * 2.5f, ((BaseSize / 2.0f) * 2.5f) - 1); // Subtract one to get the location between the two "middle" blocks

        }
        else
        {
            return new Vector3((BaseSize / 2.0f) * 2.5f, yLevel * 2.5f, (BaseSize / 2.0f) * 2.5f); // Get the location of the center of the middle block
        }
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey("BoardChanges") && (int)PhotonNetwork.CurrentRoom.CustomProperties["BoardChanges-FromPlayer"] != PhotonNetwork.LocalPlayer.ActorNumber)
        {
            PullChangesFromNetworkedBoard();
        } else if(propertiesThatChanged.ContainsKey("InitialBoard"))
        {
            InitializeBoard();
        }
    }
}
