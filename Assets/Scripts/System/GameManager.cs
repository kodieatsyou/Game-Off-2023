using System.Collections.Generic;
using System.Linq;
using UnityEngine;
//using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public enum GameState
{
    GameStarted,
    GameWaiting,
    GameEnded
}

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;
    public PhotonView GMPhotonView;

    //private BoardManager BM = BoardManager.Instance;
    private UIController UI;

    private int CurrentPlayerTurnIndex;
    private int winHeight;
    private int PlayerCount;
    private bool[] PlayerActorLeft; // false if still in lobby, true if left
    private int PlayerRollCount; // tracks number of players who have rolled for turn positioning

    private Hashtable CustomRoomProperties = new Hashtable();
    private int[] PlayerTurnOrder;
    private Dictionary<int, int> PlayerTurnOrderRolls = new Dictionary<int, int>();
    private GameState CurrentGameState = GameState.GameWaiting;

    void Awake() 
    {
        if (Instance == null)
        {
            Instance = this;
            GMPhotonView = GetComponent<PhotonView>();
            if (GMPhotonView == null)
            {
                GMPhotonView = GetComponent<PhotonView>();
                GMPhotonView.OwnershipTransfer = OwnershipOption.Takeover;
            }
            winHeight = (int)PhotonNetwork.CurrentRoom.CustomProperties["WinHeight"];
            UI = GameObject.FindGameObjectWithTag("UI").GetComponent<UIController>();
            PlayerCount = PhotonNetwork.CurrentRoom.PlayerCount;
            PlayerActorLeft = new bool[PlayerCount];
            PlayerTurnOrder = new int[PlayerCount];
            for (int i = 0; i < PlayerTurnOrder.Length; i++) { PlayerTurnOrder[i] = -1; } // set all playerTurnOrders to -1
            PlayerRollCount = 0;

            // If the GameManager properies are not set, initialize them
            if (!PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("CurrentPlayerTurnIndex") ||
                !PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("CurrentGameState")
                )
            {
                CustomRoomProperties.Add("CurrentPlayerTurnIndex", -1);
                CustomRoomProperties.Add("CurrentGameState", GameState.GameWaiting);
                PhotonNetwork.CurrentRoom.SetCustomProperties(CustomRoomProperties);
            }
        }
        else {
            Destroy(this);
        }
    }

    #region UnityFrameFunctions
    // Start is called before the first frame update
    void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                photonView.RPC("RPCGameManagerSyncGameState", RpcTarget.All, GameState.GameWaiting); // notify all clients game is waiting for player order
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (CurrentGameState == GameState.GameWaiting)
        {
            if (PlayerRollCount == PlayerCount)
            {
                PlayerTurnOrder = PlayerTurnOrderRolls.OrderBy(kv => kv.Value).Select(kv => kv.Key).ToArray(); // get player order
                if (PhotonNetwork.IsMasterClient)
                {
                    photonView.RPC("RPCGameManagerSyncGameState", RpcTarget.All, GameState.GameStarted); // notify all clients game has started
                }
            }

        }
        else if (CurrentGameState == GameState.GameStarted)
        {
            bool gameOver = CheckForWin(); // Maybe this should be called on a timer instead of every frame?
            if (gameOver)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    photonView.RPC("RPCGameManagerSyncGameState", RpcTarget.All, GameState.GameEnded); // notify all clients game has ended
                }
            }
        }
        // Do nothing when State is GameEnded?
    }
    #endregion

    #region GameNetwork
    /// <summary>
    /// RPC designated function that takes state update for each local player and determines how to handle the switches.
    /// </summary>
    /// <param name="newState">GameState enum </param>
    [PunRPC]
    public void RPCGameManagerSyncGameState(GameState newState)
    {
        if (newState != CurrentGameState)
        {
            CurrentGameState = newState;
            switch (newState)
            {
                case GameState.GameWaiting:
                    Debug.Log("State has switched to 'Waiting'.");
                    break;

                case GameState.GameStarted:
                    Debug.Log("Game has switched to 'Started', starting first players turn");
                    if (PhotonNetwork.IsMasterClient)
                    {
                        photonView.RPC("RPCGameManagerSyncPlayerTurnIndex", RpcTarget.All, true); // send all local players a game
                    }
                    break;

                case GameState.GameEnded:
                    Debug.Log("Game has switched to 'Ended'.");
                    ShowGameOver();
                    break;

                default:
                    Debug.Log("Invalid Game State");
                    throw new System.Exception("Invalid Game State");
            }
        }
        else
        {
            Debug.Log("No new state detected.");
        }
    }

    /// <summary>
    /// Handles messages over RPC that the game board was initialized then spawns the player objects
    /// </summary>
    [PunRPC]
    public void RPCGameManagerSyncPlayerTurnIndex(bool isFirstTurn)
    {
        int nextPlayerTurnIdx = 0;
        if (!isFirstTurn)
        {
            Debug.Log("Finding new PlayerTurnIndex");
            // skip over players who have left the room
            nextPlayerTurnIdx = (CurrentPlayerTurnIndex + 1) % PlayerCount;
            while (PlayerActorLeft[nextPlayerTurnIdx] == true)
            {
                nextPlayerTurnIdx = (CurrentPlayerTurnIndex + 1) % PlayerCount;
            }
        }

        int PlayerTurnID = PlayerTurnOrder[nextPlayerTurnIdx];
        CustomRoomProperties["CurrentPlayerTurnIndex"] = PlayerTurnID;
        PhotonNetwork.CurrentRoom.SetCustomProperties(CustomRoomProperties); // Set the updated custom properties

        CurrentPlayerTurnIndex = nextPlayerTurnIdx;
    }

    /// <summary>
    /// Handles messages over RPC that a player has ended their turn. Then posts a message to RPC that the next player should start their turn.
    /// </summary>
    [PunRPC]
    public void RPCGameManagerEndTurn()
    {
        // RPC called on all clients when a player ends their turn
        Debug.Log("End of Turn RPC received.");

        // Start the next player's turn on the master client
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("RPCGameManagerSyncPlayerTurnIndex", RpcTarget.All, false); // send all local players a game
        }
    }
    /// <summary>
    /// Recieves RPC events that add items to the ordering dict
    /// </summary>
    /// <param name="playerActorID">playerActorID</param>
    /// <param name="roll">Amount that was rolled</param>
    [PunRPC]
    public void RPCSetPlayerOrder(int playerActorID, int roll)
    {
        PlayerTurnOrderRolls.Add(playerActorID, roll);
        PlayerRollCount++;
    }
    #endregion

    #region GameCondition
    /// <summary>
    /// Check for win based on player height relative to the room's winHeight
    /// </summary>
    /// <returns>(bool) True if win is detected, false if not.</returns>
    private bool CheckForWin()
    {
        var playerObjs = GameObject.FindGameObjectsWithTag("Player");
        foreach (var player in playerObjs)
        {
            float y = player.transform.position.y;
            if (y >= winHeight)
            {
                // If there is a winner, broadcast to all players that the game is over
                var nickName = player.GetComponent<PhotonView>().Owner.NickName;
                GMPhotonView.RPC("RpcPlayerControllerGameOver", RpcTarget.All, nickName);
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Show game over for the local player based on the level standings
    /// </summary>
    private void ShowGameOver()
    {
        var playerObjs = GameObject.FindGameObjectsWithTag("Player");
        foreach (var player in playerObjs)
        {
            var nickName = player.GetComponent<PhotonView>().Owner.NickName;
            // UI.addPlayerToGameOver(player.GetComponent<PhotonView>().Owner.NickName);
        }
    }
    #endregion

    #region PUNOverrides
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.CreateRoom("Offline Room", null, null, null);
    }
    public override void OnPlayerLeftRoom(Player playerLeft)
    {
        int playerLeftID = playerLeft.ActorNumber;
        PlayerActorLeft[playerLeftID] = true; // mark player as left.
        // TODO UI element that player left?

    }
    public override void OnPlayerEnteredRoom(Player playerLeft)
    {
        int playerEnterID = playerLeft.ActorNumber;
        PlayerActorLeft[playerEnterID] = false; // mark player as re-entered
        // TODO UI element that player rejoined?
    }
    #endregion
}