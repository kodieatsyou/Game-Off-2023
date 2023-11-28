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
public class BoardManagerLocal : MonoBehaviourPunCallbacks
{
    public static BoardManagerLocal Instance;
    public int BaseSize;
    public int HeightSize;
    public static BoardSpaceLocal[,,] BoardSpaceLocal_Arr;
    public int yOfCurrentHeighestBuiltBlock = 0;
    public SelectionMode selectionMode;
    public bool initialized = false;
    public bool syncing = false;

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
        if(!initialized)
        {
            if(PhotonNetwork.CurrentRoom.CustomProperties["InitialBoard"] != null)
            {
                InitializeBoard();
            }
        }
    }

    public void InitializeBoard()
    {
        initialized = true;
        string[] networkBoardSpaceArr = (string[])PhotonNetwork.CurrentRoom.CustomProperties["InitialBoard"];
        BaseSize = (int)PhotonNetwork.CurrentRoom.CustomProperties["BoardBaseSize"];
        HeightSize = (int)PhotonNetwork.CurrentRoom.CustomProperties["BoardHeightSize"];
        BoardSpaceLocal_Arr = new BoardSpaceLocal[BaseSize, HeightSize, BaseSize];

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
                        //Debug.Log("Index: " + index + " Position: " + networkBoardSpace.posInBoard);
                        BoardSpaceLocal_Arr[x, y, z] = Instantiate(GameAssets.i.board_space_object_, transform).GetComponent<BoardSpaceLocal>().InitializeSpace(networkBoardSpace.posInBoard, 2.5f, networkBoardSpace.isBuilt);
                    }
                }
            }
        }
        BoardManagerNetwork.Instance.BMPhotonView.RPC("RPCBoardManagerBoardInitialized", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber);
    }

    public void PullChangesFromNetworkedBoard()
    {
        string change = (string)PhotonNetwork.CurrentRoom.CustomProperties["BoardChanges"];
        BoardSpaceNetwork networkBoardSpace = BoardSpaceNetwork.FromJson(change);
        //Debug.Log("Pos changed: " + (int)networkBoardSpace.posInBoard.x + ", " + (int)networkBoardSpace.posInBoard.y + ", " + (int)networkBoardSpace.posInBoard.z);
        BoardSpaceLocal_Arr[(int)networkBoardSpace.posInBoard.x, (int)networkBoardSpace.posInBoard.y, (int)networkBoardSpace.posInBoard.z].SetIsBuiltNoUpdate(networkBoardSpace.isBuilt);
    }

    public void PushSyncFromLocalBoard(BoardSpaceLocal spaceChanged)
    {
        //Debug.Log("Pushing changed from block: " + spaceChanged.ToString());
        string spaceChangedString = new BoardSpaceNetwork(spaceChanged.GetPosInBoard(), spaceChanged.GetIsBuilt()).ToJson();
        BoardManagerNetwork.Instance.BMPhotonView.RPC("RPCBoardManagerPushChangesFromLocalBoard", RpcTarget.All, spaceChangedString, PhotonNetwork.LocalPlayer.ActorNumber);
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
        }
    }
}
