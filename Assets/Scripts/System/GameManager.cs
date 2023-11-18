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
    private PhotonView photonView;
    private bool GameOver = false;

    void Awake() 
    {
        if (Instance == null)
        {
            Instance = this;
            photonView = PhotonView.Get(this);
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
            SetCurrentPlayerTurn(0);
        }
    }
    // Update is called once per frame
    void Update()
    {
        Debug.Log($"Current Turn: {CurrentPlayerTurn}");
        // TODO Check for game over conditions
    }
    #endregion

    #region GameNetwork
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
            photonView.RPC("RpcPlayerControllerStartTurn", RpcTarget.All); // Inform other players that another turn has started.
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
}