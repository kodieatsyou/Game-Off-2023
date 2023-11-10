using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class Lobby : MonoBehaviourPunCallbacks
{
    #region Class Variables

    public static Lobby Instance;

    [SerializeField] private TMP_InputField roomNameInput;
    [SerializeField] private TMP_Text roomName;
    [SerializeField] public TMP_InputField playerNameInput;
    [SerializeField] private byte maxPlayersPerRoom = 4;
    [SerializeField] Transform playerListContent;
	[SerializeField] GameObject playerListItemPrefab;
    [SerializeField] Transform roomListContent;
	[SerializeField] GameObject roomListItemPrefab;
    [SerializeField] GameObject startGameButton;
    [SerializeField] TMP_Text errorHeader;
    [SerializeField] TMP_Text errorBody;

    private string gameVersion = "1";

    private static Dictionary<string, RoomInfo> cachedRoomList = new Dictionary<string, RoomInfo>();

    #endregion

    #region Class Functions

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.ConnectUsingSettings();
    }

    public void Quickplay()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.JoinRandomRoom();
        }
    }

    public void HandlePlayerNameInput()
    {
        PhotonNetwork.NickName = playerNameInput.text;

        if(PhotonNetwork.NickName == "")
        {
            PhotonNetwork.NickName = NameGenerator.Instance.GenerateName();
        }
    }

    public void CreateRoom()
    {
        if(string.IsNullOrEmpty(roomNameInput.text))
        {
            PhotonNetwork.CreateRoom("Room " + Random.Range(0, 10000).ToString("0000"), new RoomOptions { MaxPlayers = maxPlayersPerRoom }, TypedLobby.Default);
        }
        else
        {
            PhotonNetwork.CreateRoom(roomNameInput.text, new RoomOptions { MaxPlayers = maxPlayersPerRoom }, TypedLobby.Default);
        }
        MenuManager.Instance.OpenMenu("loading");
    }

    public void JoinRoom(RoomInfo info)
	{
		PhotonNetwork.JoinRoom(info.Name);
		MenuManager.Instance.OpenMenu("loading");
	}

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public void StartGame()
    {
        PhotonNetwork.LoadLevel(1);
    }

    public void RefreshPlayerList()
    {
        Player[] players = PhotonNetwork.PlayerList;

		foreach(Transform child in playerListContent)
		{
			Destroy(child.gameObject);
		}

		for(int i = 0; i < players.Length; i++)
		{
			Instantiate(playerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetInfo(players[i]);
		}
    }

    #endregion

    #region Photon Callback Functions

    public override void OnConnectedToMaster()
    {
        Debug.Log("OnConnectedToMaster() called, client connected to master server");

        PhotonNetwork.JoinLobby();
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
	{
        startGameButton.SetActive(PhotonNetwork.IsMasterClient);
	}

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarningFormat("OnDisconnected() called, client disconnected from photon server, cause: {0}", cause);
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("OnJoinedLobby() called, client is connected to the default lobby");

        MenuManager.Instance.OpenMenu("lobby");

        PhotonNetwork.NickName = "";

        if(PhotonNetwork.NickName == "")
        {
            PhotonNetwork.NickName = NameGenerator.Instance.GenerateName();
        }
    }

    public override void OnLeftLobby()
    {
        Debug.Log("OnLeftLobby() called, client has left the default lobby");
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("OnCreatedRoom() called, a new room has been created");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("OnJoinedRoom() called, client is connected to a room");

        RefreshPlayerList();
        roomName.text = PhotonNetwork.CurrentRoom.Name;
        MenuManager.Instance.OpenMenu("room");
        startGameButton.SetActive(PhotonNetwork.IsMasterClient);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("OnCreateRoomFailed() called, room to create room");

        errorHeader.text = "Failed to Create Room";
        errorBody.text = message;
        MenuManager.Instance.OpenMenu("error");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
	{
        Debug.Log("OnPlayerEnteredRoom() called, a player has entered the room");

		Instantiate(playerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetInfo(newPlayer);
	}

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
	{
        Debug.LogWarning("OnRoomListUpdate() called, room list updated");

        foreach (Transform trans in roomListContent)
        {
            Destroy(trans.gameObject);
        }

        for (int i = 0; i < roomList.Count; i++)
        {
            RoomInfo info = roomList[i];
            if (info.RemovedFromList || !(info.IsOpen && info.IsVisible && info.PlayerCount > 0))
            {
                cachedRoomList.Remove(info.Name);
            }
            else
            {
                cachedRoomList[info.Name] = info;
            }
        }

        foreach (KeyValuePair<string, RoomInfo> entry in cachedRoomList)
        {
            Instantiate(roomListItemPrefab, roomListContent).GetComponent<RoomListItem>().SetInfo(cachedRoomList[entry.Key]);
        }

	}

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("OnJoinRandomFailed() called, failed to join random room");

        errorHeader.text = "Quickplay Failed";
        errorBody.text = message;
        MenuManager.Instance.OpenMenu("error");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("OnJoinRoomFailed() called, failed to join selected room");

        errorHeader.text = "Failed to Join Room";
        errorBody.text = message;
        MenuManager.Instance.OpenMenu("error");
    }

    public override void OnLeftRoom()
    {
        Debug.Log("OnLeftRoom() called, client left room");

        MenuManager.Instance.OpenMenu("lobby");
    }

    #endregion
}
