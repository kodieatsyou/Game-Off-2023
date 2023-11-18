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

    private int CurrentPlayerTurn = 0;
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
            if (!PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("CurrentPlayerTurn"))
            {
                // If the turn property is not set, initialize it
                Hashtable initialProps = new Hashtable
                {
                    { "CurrentPlayerTurn", CurrentPlayerTurn }
                };

                PhotonNetwork.CurrentRoom.SetCustomProperties(initialProps);
            }

            // Retrieve the initial value of CurrentPlayerTurn from room custom properties
            CurrentPlayerTurn = (int)PhotonNetwork.CurrentRoom.CustomProperties["CurrentPlayerTurn"];
        }
    }
    // Update is called once per frame
    void Update()
    {
        Debug.Log($"Current Turn: {CurrentPlayerTurn}");
    }
    #endregion

    #region GameNetwork
    [PunRPC]
    public void RpcManagerEndTurn()
    {
        // RPC called on all clients when a player ends their turn
        Debug.Log("End of Turn RPC received.");

        // Start the next player's turn on the master client
        if (PhotonNetwork.IsMasterClient && !GameOver)
        {
            CurrentPlayerTurn = (CurrentPlayerTurn + 1) % PhotonNetwork.CurrentRoom.PlayerCount; // Switch to the next player's turn
            
            Hashtable turnProps = new Hashtable // Update room custom property to synchronize the current turn across the network
            {
                { "CurrentPlayerTurn", CurrentPlayerTurn }
            };

            PhotonNetwork.CurrentRoom.SetCustomProperties(turnProps);
            photonView.RPC("RpcPlayerControllerStartTurn", RpcTarget.All); // Inform other players that another turn has started.
        }
    }
    #endregion


}
