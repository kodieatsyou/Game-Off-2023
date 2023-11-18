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
    public Button StartChooseOrderButton;
    public Button StartGameButton;
    
    private const string CurrentTurnNumberPropertyName = "CurrentPlayerTurn";
    private int CurrentPlayerTurn = 0;
    private PhotonView photonView;

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
            if (!PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(CurrentTurnNumberPropertyName))
            {
                // If the turn property is not set, initialize it
                Hashtable initialProps = new Hashtable
                {
                    { CurrentTurnNumberPropertyName, CurrentPlayerTurn }
                };

                PhotonNetwork.CurrentRoom.SetCustomProperties(initialProps);
            }

            // Retrieve the initial value of CurrentPlayerTurn from room custom properties
            CurrentPlayerTurn = (int)PhotonNetwork.CurrentRoom.CustomProperties[CurrentTurnNumberPropertyName];
        }
    }
    // Update is called once per frame
    void Update()
    {

    }
    #endregion

    #region TurnFunctions
    public void EndTurn()
    {
        // Inform other players that the current player has ended their turn
        photonView.RPC("RpcEndTurn", RpcTarget.All);

        // Switch to the next player's turn
        CurrentPlayerTurn = (CurrentPlayerTurn + 1) % PhotonNetwork.CurrentRoom.PlayerCount;

        // Update room custom property to synchronize the current turn across the network
        Hashtable turnProps = new Hashtable
        {
            { CurrentTurnNumberPropertyName, CurrentPlayerTurn }
        };

        PhotonNetwork.CurrentRoom.SetCustomProperties(turnProps);

        // Inform other players that another turn has started.
        photonView.RPC("RpcStartTurn", RpcTarget.All);
    }
    #endregion

    #region Getters
    public string GetCurrentTurnNumberPropertyName()
    {
        return CurrentTurnNumberPropertyName;
    }
    #endregion
}
