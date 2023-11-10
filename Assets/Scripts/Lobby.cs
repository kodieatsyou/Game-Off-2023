using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

using Photon.Pun;
using Photon.Realtime;

public class Lobby : MonoBehaviourPunCallbacks
{
    #region Class Variables

    [SerializeField] private InputField roomNameInput;
    [SerializeField] private Text roomName;
    [SerializeField] private byte maxPlayersPerRoom = 4;
    [SerializeField] Transform playerListContent;
	[SerializeField] GameObject PlayerListItemPrefab;

    private string gameVersion = "1";

    private string[] lobbyList = new string[] {};

    #endregion

    #region Class Functions

    void Awake()
    {
        // Easier to have map sync with all players
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    void Start()
    {
        Connect();
    }

    private void Connect()
    {
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.GameVersion = gameVersion;
    }

    public void Quickplay()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.JoinRandomRoom();
        }
    }

    public void HandleCreateJoinRoomButton()
    {
        MenuManager.Instance.OpenMenu("createJoin");
    }

    public void HandleMenuButton()
    {
        MenuManager.Instance.OpenMenu("lobby");
    }

    public void CreateOrJoinRoom()
    {
        PhotonNetwork.JoinOrCreateRoom(roomNameInput.text, new RoomOptions { MaxPlayers = maxPlayersPerRoom }, TypedLobby.Default);
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        MenuManager.Instance.OpenMenu("lobby");
    }

    #endregion

    #region Photon Callback Functions

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
        MenuManager.Instance.OpenMenu("lobby");
        Debug.Log("OnConnectedToMaster() was called, client connected to photon server");
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
	{
        // Need to active start button for host
	}

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarningFormat("OnDisconnected() was called, client disconnected from photon server, cause: {0}", cause);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("OnJoinRoom() was called, this client is connected to a room");

        MenuManager.Instance.OpenMenu("room");
        roomName.text = PhotonNetwork.CurrentRoom.Name;

        Player[] players = PhotonNetwork.PlayerList;

		foreach(Transform child in playerListContent)
		{
			Destroy(child.gameObject);
		}

		for(int i = 0; i < players.Length; i++)
		{
			Instantiate(PlayerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(players[i]);
		}

		// startGameButton.SetActive(PhotonNetwork.IsMasterClient);

        // TODO: insert level name
        // PhotonNetwork.LoadLeve(<levelname here>);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.LogWarningFormat("OnJoinRandomFailed() was called, failed to join room, cause: {0}", message);
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("OnJoinLobby was called, this client is connected to the default server");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
	{
		Instantiate(PlayerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(newPlayer);
	}

    // Potential Server Browser Idea
    // public override void OnRoomListUpdate(List<RoomInfo> roomList)
    // {
    //     Debug.LogFormat ("OnRoomListUpdate was called, {0}", roomList);
    // }

    #endregion
}
