using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    GameInitializing,
    GameStarted,
    GameWaiting,
    GameEnded
}

public class GameManagerTest : MonoBehaviour
{
    public static GameManagerTest Instance;

    public GameState State = GameState.GameInitializing;
    public PhotonView GMPhotonView;
    private List<KeyValuePair<int, Player>> players;

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
        
        players = new List<KeyValuePair<int, Player>>();
        GMPhotonView = GetComponent<PhotonView>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartGame()
    {
        State = GameState.GameStarted;
        UIController.Instance.PlayAnnouncement("Roll to decide turn order!", AnnouncementType.ScrollLR);
        PlayerController.Instance.RollForTurn();
    }

    public void PlayerInitialized(Player player)
    {
        /*Debug.Log("Player: " + player.NickName + " Has Initialized!");
        players.Add(-1, player);
        if(players.Count == PhotonNetwork.CurrentRoom.PlayerCount)
        {
            StartGame();
        }*/
    }

    [PunRPC]
    void RPCGameManagerPlayerRolledForTurn(int roll, PhotonMessageInfo info)
    {
        Debug.Log(info.Sender.NickName + " ROLLED A: " + roll + " for their turn order!");
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].Value == info.Sender)
            {
                players[i] = new KeyValuePair<int, Player>(roll, info.Sender);
                break;
            }
        }
    }

    [PunRPC]
    void RPCGameManagerStartPlayerTurn(int playerID)
    {
        if (playerID == PhotonNetwork.LocalPlayer.ActorNumber)
        {
            PlayerController.Instance.StartTurn();
        }
        else
        {
            return;
        }
    }

    [PunRPC]
    public void RPCGameManagerPlayerInitialized(PhotonMessageInfo info)
    {
        Debug.Log("Player: " + info.Sender.NickName + " Has Initialized!");
        players.Add(new KeyValuePair<int, Player>(-1, info.Sender));
        if (players.Count == PhotonNetwork.CurrentRoom.PlayerCount)
        {
            StartGame();
        }
    }

}
