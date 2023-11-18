using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerController: MonoBehaviourPunCallbacks
{
    public string PlayerName { get; set; }
    public bool IsActiveTurn { get; set; }
    public int Score { set; get; }
    public int CurrentLevel { set; get; }
    public float TurnLength = 15f;
    public GameObject GameObjectPrefab;
    public Vector3 BoardPosition;
    public int ActionsRemaining { private set; get; }

    private bool GameOver;
    private int PlayerID;
    private PhotonView photonView;
    private Coroutine TimerCoroutine;

    void Start()
    {
        int viewID = photonView.ViewID;
        PhotonView gameManagerPhotonView = gameManager.GetComponent<PhotonView>();
        photonView = PhotonView.Get(this);
        PlayerID = PhotonNetwork.LocalPlayer.ActorNumber - 1;
        PlayerName = name;
        // GET GAME OBJECT GameObjectPrefab = prefab;
        IsActiveTurn = false;
        ActionsRemaining = 3;
        //BoardPosition = BoardManager.Instance.SetPlayerSpawn();
        Debug.Log("Start player with name: " + PlayerName);
        TurnDone = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (IsActiveTurn == true)
        {
            if (turnDone)
            {
                photonView.RPC("RpcManagerEndTurn", RpcTarget.All); // after turn is done notify all other players this turn is done.
                IsActiveTurn = false;
            }
            else
            {
                // Do nothing?
            }
            // TODO Render the turn UI
        }
    }

    #region Network
    [PunRPC]
    public void RpcPlayerControllerStartTurn() // called by network in game manager
    {
        // Get the playerTurn from the game manager to determine if this is the 
        string propName = GameManager.Instance.GetCurrentTurnNumberPropertyName();
        int currentPlayerTurn = (int)PhotonNetwork.CurrentRoom.CustomProperties[propName];
        if (PlayerID == currentPlayerTurn)
        {
            IsActiveTurn = true;
        }
    }
    #endregion
}
