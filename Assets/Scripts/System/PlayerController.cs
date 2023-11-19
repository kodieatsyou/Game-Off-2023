using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class PlayerController: MonoBehaviourPunCallbacks
{
    private int PlayerID;
    private string PlayerName;
    private float TurnLength;
    private int ActionsRemaining;
    private bool IsActiveTurn;
    private PhotonView photonView;

    #region UnityFrameFunctions
    void Start()
    {
        PlayerID = PhotonNetwork.LocalPlayer.ActorNumber;
        PlayerName = PhotonNetwork.LocalPlayer.NickName;
        Debug.Log("Awake Player with name: " + PlayerName);
        ActionsRemaining = 3; // move, roll, build
        IsActiveTurn = false;
        PhotonView photonView = PhotonView.Get(this);

        setTurnLength(); // set turn length at the start of the game
    }
    void Update()
    {
        if (IsActiveTurn)
        {
            TurnLength -= Time.deltaTime;
            if (TurnLength < 0f || ActionsRemaining < 0)
            {
                EndTurn();
            }
        }
    }
    #endregion

    # region PlayerNetwork
    [PunRPC]
    public void RpcPlayerControllerStartTurn()
    {
        // RPC called on all clients when a player ends their turn
        Debug.Log("Start of Turn RPC received.");
        int CurrentPlayerTurn = (int)PhotonNetwork.CurrentRoom.CustomProperties["CurrentPlayerTurn"];
        if (CurrentPlayerTurn == PlayerID)
        {
            setTurnLength(); // reset turn length at the start of the turn
            StartTurn();
        }
    }

    /// <summary>
    /// Handles messages over RPC that the game has ended. Forces the player to end turn (if it hasn't already) and displays the winner's name.
    /// </summary>
    /// <param name="winnerNickName">Nickname of the winning player</param>
    [PunRPC]
    public void RpcPlayerControllerGameOver(string winnerNickName)
    {
        // TODO Display winner's name
        Debug.Log("Game Over RPC received.");
        IsActiveTurn = true;
    }
    #endregion

    # region PlayerActions
    private void StartTurn()
    {
        IsActiveTurn = true;
        // TODO: Add UI elements to indicate turn has started
        photonView.RPC("RpcManagerStartTurn", RpcTarget.All); // Inform game manager turn has started.
    }

    private void EndTurn()
    {
        IsActiveTurn = false;
        // TODO: Add UI elements to indicate turn has ended
        photonView.RPC("RpcManagerEndTurn", RpcTarget.All); // Inform game manager turn has ended.
    }
    # endregion

    # region Setters
    /// <summary>
    /// Set TurnLength by calling the Photon Network to get the TurnLength from the room custom properties or set it to 15f if it doesn't exist.
    /// </summary>
    private void setTurnLength()
    {
        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("TurnLength", out var value))
        {
            TurnLength = (float)value;
        }
        else
        {
            TurnLength = 15f;
        }
    }
    #endregion
}
