using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;

    private int CurrentPlayerTurn;
    private PhotonView GCPhotonView;
    private bool GameOver = false;
    private Settings settingsInstance;
    private UIController UI;

    void Awake() 
    {
        if (Instance == null)
        {
            Instance = this;
            GCPhotonView = GetComponent<PhotonView>();
            GCPhotonView.OwnershipTransfer = OwnershipOption.Takeover;
            settingsInstance = new Settings();
            UI = GameObject.FindGameObjectWithTag("UI").GetComponent<UIController>();
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
            Debug.Log("Setting turns");
            SetCurrentPlayerTurn(0);
        }
    }
    // Update is called once per frame
    void Update()
    {
        //Debug.Log($"Current Turn: {CurrentPlayerTurn}");
        CheckForWin(); // Maybe this should be called on a timer instead of every frame?
        if (GameOver)
        {
            ShowGameOver();
        }
    }
    #endregion

    #region GameNetwork

    /// <summary>
    /// Handles messages over RPC that the game board was initialized then spawns the player objects
    /// </summary>
    [PunRPC]
    public void RpcBoardInitialized()
    {
        Debug.Log("Game board initialized!");
    }

    /// <summary>
    /// Handles messages over RPC that a player has ended their turn. Then posts a message to RPC that the next player should start their turn.
    /// </summary>
    [PunRPC]
    public void RpcManagerEndTurn()
    {
        // RPC called on all clients when a player ends their turn
        Debug.Log("End of Turn RPC received.");

        // Start the next player's turn on the master client
        if (PhotonNetwork.IsMasterClient && !GameOver)
        {
            int nextPlayerTurn = (CurrentPlayerTurn + 1) % PhotonNetwork.CurrentRoom.PlayerCount; // Switch to the next player's turn
            SetCurrentPlayerTurn(nextPlayerTurn);
            GCPhotonView.RPC("RpcPlayerControllerStartTurn", RpcTarget.All); // Inform other players that another turn has started.
        }
    }
    /// <summary>
    /// Handles messages over RPC that a player has started their turn.
    /// </summary>
    [PunRPC]
    public void RpcManagerStartTurn()
    {
        // RPC called on all clients when a player ends their turn
        Debug.Log("Player started turn RPC message received.");
    }
    /// <summary>
    /// Sets the GameManager CurrentPlayerTurn property and updates the CurrentPlayerTurn room property.
    /// </summary>
    /// <param name="newPlayerTurn">The number of the newPlayerTurn</param>
    private void SetCurrentPlayerTurn(int newPlayerTurn)
    {
        CurrentPlayerTurn = newPlayerTurn;
        if (!PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("CurrentPlayerTurn"))
        {
            // If the turn property is not set, initialize it
            Hashtable initialProps = new Hashtable
            {
                { "CurrentPlayerTurn", newPlayerTurn }
            };

            PhotonNetwork.CurrentRoom.SetCustomProperties(initialProps);
        }
    }
    #endregion

    #region GameCondition
    private void CheckForWin()
    {
        if (PhotonNetwork.IsMasterClient) // only the master client should check for win conditions
        {
            var playerObjs = GameObject.FindGameObjectsWithTag("Player");
            foreach (var player in playerObjs)
            {
                float y = player.transform.position.y;
                if (y >= settingsInstance.boardHeight)
                {
                    // If there is a winner, broadcast to all players that the game is over
                    var nickName = player.GetComponent<PhotonView>().Owner.NickName;
                    GCPhotonView.RPC("RpcPlayerControllerGameOver", RpcTarget.All, nickName);

                    GameOver = true; // mark the game as over
                }
            }
        }
    }
    private void ShowGameOver()
    {
        if (PhotonNetwork.IsMasterClient) // only the master client should spawn the game over screen for everyone
        {
            var playerObjs = GameObject.FindGameObjectsWithTag("Player");
            foreach (var player in playerObjs)
            {
                var nickName = player.GetComponent<PhotonView>().Owner.NickName;
                // UI.addPlayerToGameOver(player.GetComponent<PhotonView>().Owner.NickName);
            }
        }
    }
    
    #endregion

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.CreateRoom("Offline Room", null, null, null);
    }
}