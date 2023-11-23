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
    private float NetworkTurnLength;
    private float CurrTurnLength;
    private int ActionsRemaining;
    private bool IsActiveTurn;
    private GameManager GM = GameManager.Instance;
    private UIController UI;

    #region UnityFrameFunctions
    void Start()
    {
        PlayerID = PhotonNetwork.LocalPlayer.ActorNumber;
        PlayerName = PhotonNetwork.LocalPlayer.NickName;
        NetworkTurnLength = (float)PhotonNetwork.CurrentRoom.CustomProperties["WinHeight"];
        CurrTurnLength = NetworkTurnLength;
        ActionsRemaining = 3; // move, roll, build
        IsActiveTurn = false;
        UI = GameObject.FindGameObjectWithTag("UI").GetComponent<UIController>();
        Debug.Log("Awake Player with name: " + PlayerName);
    }
    void Update()
    {
        if (IsActiveTurn)
        {
            CurrTurnLength -= Time.deltaTime;
            if (CurrTurnLength < 0f || ActionsRemaining < 0)
            {
                UI.SetTurnTime(CurrTurnLength); // SetTurnTime(CurrTurnLength) Bug, currently SetTurnTime does not take in an argument
                EndTurn();
            }
        }
    }
    #endregion

    #region PlayerNetwork
    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey("CurrentPlayerTurn"))
        {
            Debug.Log("CurrentPlayerTurn change detected");
            int networkPlayerValue = (int)PhotonNetwork.CurrentRoom.CustomProperties["CurrentPlayerTurn"];
            if (networkPlayerValue == PlayerID)
            {
                StartTurn();
            }
        }
    }
    #endregion

    #region PlayerActions
    private void StartTurn()
    {
        IsActiveTurn = true;
        // TODO: Add UI elements to indicate turn has started, enable UI
    }

    private void EndTurn()
    {
        IsActiveTurn = false;
        CurrTurnLength = NetworkTurnLength; // reset turn length at the end of the turn
        // TODO: Add UI elements to indicate turn has ended, disable UI
        GM.GMPhotonView.RPC("RPCGameManagerEndTurn", RpcTarget.All); // Inform game manager turn has ended.
    }
    # endregion
}
