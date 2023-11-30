using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System;

public class PlayerController: MonoBehaviourPunCallbacks
{
    public static PlayerController Instance;

    private int PlayerID;
    private string PlayerName;
    private float NetworkTurnLength;
    private float CurrTurnLength;
    public int ActionsRemaining;
    public int blocksLeftToPlace;
    private bool IsActiveTurn;
    private bool IsRollingDie;

    public BoardSpace currentSpace;

    //Moving
    public int moveSpeed = 5;

    public int fallSpeed = 12;
    public int rotationSpeed = 10;
    public bool moving = false;
    

    #region UnityFrameFunctions
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        PlayerID = PhotonNetwork.LocalPlayer.ActorNumber;
        PlayerName = PhotonNetwork.LocalPlayer.NickName;
        //NetworkTurnLength = (float)PhotonNetwork.CurrentRoom.CustomProperties["WinHeight"];
        NetworkTurnLength = 3.0f;
        CurrTurnLength = NetworkTurnLength;
        ActionsRemaining = 3; // move, roll, build
        IsActiveTurn = false;

        GameManagerTest.Instance.GMPhotonView.RPC("RPCGameManagerPlayerInitialized", RpcTarget.All);
    }
    void Update()
    {
        if (IsActiveTurn)
        {
            CurrTurnLength -= Time.deltaTime;
            UIController.Instance.SetTurnTime(CurrTurnLength);
            UIController.Instance.SetBlocksLeftToBuild(blocksLeftToPlace);
            if (ActionsRemaining <= 0)
            {
                UIController.Instance.PlayAnnouncement("Out of Actions!", AnnouncementType.DropBounce);
                GameManagerTest.Instance.GMPhotonView.RPC("RPCGameManagerPlayerEndedTurn", RpcTarget.All);
                IsActiveTurn = false;
            }
            if(CurrTurnLength <= 0f) {
                UIController.Instance.PlayAnnouncement("Out of Time!", AnnouncementType.DropBounce);
                GameManagerTest.Instance.GMPhotonView.RPC("RPCGameManagerPlayerEndedTurn", RpcTarget.All);
                IsActiveTurn = false;
            }
        }
    }
    #endregion

    #region PlayerNetwork
    /*public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
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
    }*/
    #endregion

    #region PlayerActions
    public void StartTurn(float turnTime)
    {
        CurrTurnLength = turnTime;
        ActionsRemaining = 3;
        blocksLeftToPlace = 2;

        IsActiveTurn = true;
        UIController.Instance.StartTurnSetUI(CurrTurnLength);
        UIController.Instance.SetBlocksLeftToBuild(blocksLeftToPlace);
    }

    private void EndTurn()
    {
        IsActiveTurn = false;
        CurrTurnLength = NetworkTurnLength; // reset turn length at the end of the turn
        // TODO: Add UI elements to indicate turn has ended, disable UI
    }

    public void RollForTurn()
    {
        DiceController.Instance.ReadyRoller(DieType.Number, SendTurnRollToGameManager);
    }

    public void RollForActionDie()
    {
        DiceController.Instance.ReadyRoller(DieType.Action, DoActionDieResult);
    }

    void DoActionDieResult(int roll)
    {
        Debug.Log("Rolled a: " + roll);
        switch(roll)
        {
            case 1:
                UIController.Instance.PlayAnnouncement("Wind", AnnouncementType.DropBounce);
                UIController.Instance.ToggleWindButton(true);
                break;
            case 2:
                UIController.Instance.PlayAnnouncement("Grapple", AnnouncementType.DropBounce);
                UIController.Instance.ToggleGrappleButton(true);
                break;
            case 3:
                UIController.Instance.PlayAnnouncement("Power Card", AnnouncementType.DropBounce);
                UIController.Instance.ToggleCardsButton(true);
                break;
        }
    }

    void SendTurnRollToGameManager(int roll)
    {
        UIController.Instance.PlayAnnouncement(new string[] { "Waiting for other players.", "Waiting for other players..", "Waiting for other players..." }, AnnouncementType.StaticFrame);
        GameManagerTest.Instance.GMPhotonView.RPC("RPCGameManagerPlayerRolledForTurn", RpcTarget.All, roll);
    }

    #endregion

    #region Pathfinding

    public void MoveTo(BoardSpace spaceToMoveTo)
    {
        if(!moving)
        {
            moving = true;
            StartCoroutine(MoveThroughWaypoints(new AStarPathfinding(currentSpace, spaceToMoveTo).FindPath().ToArray(), spaceToMoveTo));
        }
    }
    IEnumerator MoveThroughWaypoints(Vector3[] waypoints, BoardSpace endSpace)
    {
        int currentWaypointIndex = 0;

        while (currentWaypointIndex < waypoints.Length)
        {
            GetComponent<PlayerAnimationController>().SetAnimatorBool("Moving", true);
            float distance = Vector3.Distance(transform.position, waypoints[currentWaypointIndex]);

            if (waypoints[currentWaypointIndex].y > transform.position.y)
            {
                Vector3 horizontalDirection = (new Vector3(waypoints[currentWaypointIndex].x, transform.position.y, waypoints[currentWaypointIndex].z) - transform.position).normalized;
                Quaternion horizontalLookRotation = Quaternion.LookRotation(horizontalDirection);
                transform.rotation = horizontalLookRotation;

                GetComponent<PlayerAnimationController>().PlayTriggeredAnimation("Climb_Up");

                yield return new WaitUntil(() => GetComponent<PlayerAnimationController>().CheckIfContinue());

                transform.position = waypoints[currentWaypointIndex];
                currentWaypointIndex++;

                yield return null;
            }
            else if (waypoints[currentWaypointIndex].y < transform.position.y)
            {
                Vector3 horizontalDirection = (new Vector3(waypoints[currentWaypointIndex].x, transform.position.y, waypoints[currentWaypointIndex].z) - transform.position).normalized;
                Quaternion horizontalLookRotation = Quaternion.LookRotation(horizontalDirection);
                transform.rotation = horizontalLookRotation;

                GetComponent<PlayerAnimationController>().PlayTriggeredAnimation("Climb_Down");

                yield return new WaitUntil(() => GetComponent<PlayerAnimationController>().CheckIfContinue());

                transform.position = waypoints[currentWaypointIndex];
                currentWaypointIndex++;

                yield return null;
            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, waypoints[currentWaypointIndex], moveSpeed * Time.deltaTime);

                Vector3 direction = (waypoints[currentWaypointIndex] - transform.position).normalized;
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);

                if (distance < 0.1f)
                {
                    currentWaypointIndex++;
                }

                yield return null;
            }
        }
        GetComponent<PlayerAnimationController>().SetAnimatorBool("Moving", false);
        BoardManager.Instance.BMPhotonView.RPC("RPCBoardManagerPlacePlayerOnSpace", RpcTarget.All, PhotonNetwork.LocalPlayer, endSpace.GetPosInBoard(), currentSpace.GetPosInBoard());
        moving = false;
    }
    #endregion

    public void GrappleToBlock(BoardSpace grappleTo) {
        if(Mathf.Abs(grappleTo.GetPosInBoard().y - currentSpace.GetPosInBoard().y) == 2) {
            Debug.Log("GRAPPLIJG");
            StartCoroutine(Grapple(grappleTo));
        } else {
            Debug.Log("Moving");
            MoveTo(grappleTo);
        }
    }

    public IEnumerator Grapple(BoardSpace target) {
        Vector3 horizontalDirection = (new Vector3(target.GetPosInWorld().x, transform.position.y, target.GetPosInWorld().z) - transform.position).normalized;
        Quaternion horizontalLookRotation = Quaternion.LookRotation(horizontalDirection);
        transform.rotation = horizontalLookRotation;

        GetComponent<PlayerAnimationController>().PlayTriggeredAnimation("Grapple");

        yield return new WaitUntil(() => GetComponent<PlayerAnimationController>().CheckIfContinue());

        transform.position = target.GetWorldPositionOfTopOfSpace();
        BoardManager.Instance.BMPhotonView.RPC("RPCBoardManagerPlacePlayerOnSpace", RpcTarget.All, PhotonNetwork.LocalPlayer, target.GetPosInBoard(), currentSpace.GetPosInBoard());
    }
    public void GetPushedByWind(WindDir dir) {
        switch(dir){
            case WindDir.North:
                transform.rotation = Quaternion.Euler(0, 90, 0);
                break;
            case WindDir.East:
                transform.rotation = Quaternion.Euler(0, 180, 0);
                break;
            case WindDir.South:
                transform.rotation = Quaternion.Euler(0, -90, 0);
                break;
            case WindDir.West:
                transform.rotation = Quaternion.Euler(0, 0, 0);
                break;
        }
        StartCoroutine(GetPushed(Board.Instance.GetWindPushBlock(currentSpace, dir)));
    }

    IEnumerator GetPushed(BoardSpace spaceToLandOn) 
    {
        currentSpace.PlacePlayerOnSpace(null);
        bool falling = false;
        if(spaceToLandOn.GetPosInBoard().y < currentSpace.GetPosInBoard().y - 1) {
            GetComponent<PlayerAnimationController>().SetAnimatorBool("Wind_Fall", true);
            falling = true;
        }
        if(spaceToLandOn == currentSpace) {
            GetComponent<PlayerAnimationController>().PlayTriggeredAnimation("Wind_Push_Into_Block");
            yield break;
        }
        GetComponent<PlayerAnimationController>().PlayTriggeredAnimation("Wind_Push");
        yield return new WaitUntil(() => GetComponent<PlayerAnimationController>().CheckIfContinue());
        if(falling) {
            float distance = Vector3.Distance(transform.position, spaceToLandOn.GetWorldPositionOfTopOfSpace());
            while (distance > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, spaceToLandOn.GetWorldPositionOfTopOfSpace(), fallSpeed * Time.deltaTime);
                distance = Vector3.Distance(transform.position, spaceToLandOn.GetWorldPositionOfTopOfSpace());
                yield return null;
            }
            GetComponent<PlayerAnimationController>().SetAnimatorBool("Wind_Fall", false);
            falling = false;
            Debug.Log("DONE!");
        }
        transform.position = spaceToLandOn.GetWorldPositionOfTopOfSpace();
        BoardManager.Instance.BMPhotonView.RPC("RPCBoardManagerPlacePlayerOnSpace", RpcTarget.All, PhotonNetwork.LocalPlayer, spaceToLandOn.GetPosInBoard(), currentSpace.GetPosInBoard());
    }
}
