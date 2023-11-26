using Photon.Pun;
using Photon.Realtime;
using System;
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

[Serializable]
public class PlayerTurnOrder
{
    public int order {  get; set; }
    public Player player {  get; set; }

    public PlayerTurnOrder(int order, Player player)
    {
        this.order = order;
        this.player = player;
    }
}

public class GameManagerTest : MonoBehaviour
{
    public static GameManagerTest Instance;

    public GameState State = GameState.GameInitializing;
    public PhotonView GMPhotonView;
    private List<PlayerTurnOrder> turnOrder;

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
        
        turnOrder = new List<PlayerTurnOrder>();
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
        for (int i = 0; i < turnOrder.Count; i++)
        {
            if (turnOrder[i].player == info.Sender)
            {
                turnOrder[i].order = roll;
                turnOrder.Sort((pair1, pair2) => pair2.order.CompareTo(pair1.order));
                UIController.Instance.SortTurnPanelBasedOnTurnOrder(turnOrder);
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
        turnOrder.Add(new PlayerTurnOrder(-1, info.Sender));
        if (turnOrder.Count == PhotonNetwork.CurrentRoom.PlayerCount)
        {
            StartGame();
        }
    }

}
