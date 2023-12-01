using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum GameState
{
    GameInitializing,
    GameRollingForTurns,
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

public class PlayerHeight
{
    public int height { get; set; }
    public Player player { get; set; }

    public PlayerHeight(int height, Player player)
    {
        this.height = height;
        this.player = player;
    }
}

public class GameManagerTest : MonoBehaviour
{
    public static GameManagerTest Instance;

    public GameState State = GameState.GameInitializing;
    public PhotonView GMPhotonView;
    private List<PlayerTurnOrder> turnOrder;
    private List<PlayerHeight> playerHeights;
    private int currentTurnIndex = 0;

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
        playerHeights = new List<PlayerHeight>();
        GMPhotonView = GetComponent<PhotonView>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DealRandomCards()
    {
        for(int i = 0; i < 6; i++)
        {
            UIController.Instance.AddCard();
        }
    }

    public void DealOneOfEachCards()
    {
        foreach (CardType type in Enum.GetValues(typeof(CardType))) {
            UIController.Instance.AddSpecificCard(type);
        }
    }

    public void RollForTurns()
    {
        State = GameState.GameRollingForTurns;
        UIController.Instance.PlayAnnouncement("Roll to decide turn order!", AnnouncementType.ScrollLR);
        PlayerController.Instance.RollForTurn();
    }

    public void StartGame()
    {
        State = GameState.GameStarted;
        StartTurn();
    }

    public void StartTurn()
    {
        UIController.Instance.HighlightTurn(currentTurnIndex);
        if (turnOrder[currentTurnIndex].player == PhotonNetwork.LocalPlayer)
        {
            PlayerController.Instance.StartTurn((float)PhotonNetwork.CurrentRoom.CustomProperties["TurnTime"]);
            UIController.Instance.PlayAnnouncement("Your turn!", AnnouncementType.ScrollLR);
        }
        else
        {
            UIController.Instance.PlayAnnouncement(turnOrder[currentTurnIndex].player.NickName + "'s turn!", AnnouncementType.ScrollLR);
        }
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

        if(!turnOrder.Exists(pair => pair.order == -1))
        {
            UIController.Instance.StopCurrentAnnouncements();
            StartGame();
            DealRandomCards();
            //DealOneOfEachCards();
        }
    }

    [PunRPC]
    void RPCGameManagerStartPlayerTurn()
    {
        StartTurn();
    }

    [PunRPC]
    public void RPCGameManagerPlayerInitialized(PhotonMessageInfo info)
    {
        Debug.Log("Player: " + info.Sender.NickName + " Has Initialized!");
        turnOrder.Add(new PlayerTurnOrder(-1, info.Sender));
        if (turnOrder.Count == PhotonNetwork.CurrentRoom.PlayerCount)
        {
            RollForTurns();
        }
    }

    [PunRPC]
    public void RPCGameManagerPlayerEndedTurn(PhotonMessageInfo info)
    {
        if(currentTurnIndex + 1 >= turnOrder.Count) {
            currentTurnIndex = 0;
        } else {
            currentTurnIndex += 1;
        }
        StartTurn();
    }

    [PunRPC]
    public void RPCGameManagerPlayerWon(Player player, PhotonMessageInfo info)
    {
        if (GMPhotonView.IsMine)
        {
            UIController.Instance.PlayAnnouncement("You won!", AnnouncementType.DropBounce);
        } else
        {
            UIController.Instance.PlayAnnouncement(player.NickName + " won!", AnnouncementType.DropBounce);
        }
        EndGame(player);
    }

    void EndGame(Player winningPlayer)
    {
        foreach(PlayerHeight p in playerHeights)
        {
            if (p.player == winningPlayer)
            {
                UIController.Instance.AddPlayertoGameOverBoard(p.player.NickName, p.height, true);
            }
            else
            {
                UIController.Instance.AddPlayertoGameOverBoard(p.player.NickName, p.height, false);
            }
        }
        UIController.Instance.ToggleGameOverScreen(true);
        State = GameState.GameEnded;
    }

    public void SetPlayerHeight(int height, Player player)
    {
        if (!playerHeights.Exists(pair => pair.player == player))
        {
            playerHeights.RemoveAll(pair => pair.player == player);
            playerHeights.Add(new PlayerHeight(height, player));
        }
    }

}
