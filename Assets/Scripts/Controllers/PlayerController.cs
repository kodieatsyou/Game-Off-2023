using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System;
using Unity.VisualScripting;
using ExitGames.Client.Photon.StructWrapping;

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

    private bool frozen = false;

    private bool hasBarrier = false;

    public BoardSpace currentSpace;

    //Moving
    public int moveSpeed = 5;

    public int fallSpeed = 12;
    public int rotationSpeed = 10;
    public bool moving = false;

    private AudioManager audioManager;

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

        audioManager = GetComponent<AudioManager>();
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
                EndTurn();
                UIController.Instance.PlayAnnouncement("Out of Actions!", AnnouncementType.DropBounce);
                GameManagerTest.Instance.GMPhotonView.RPC("RPCGameManagerPlayerEndedTurn", RpcTarget.All);
            } else if(CurrTurnLength <= 0f) {
                EndTurn();
                UIController.Instance.PlayAnnouncement("Out of Time!", AnnouncementType.DropBounce);
                GameManagerTest.Instance.GMPhotonView.RPC("RPCGameManagerPlayerEndedTurn", RpcTarget.All);
            }
        }
    }
    #endregion

    #region PlayerActions
    public void StartTurn(float turnTime)
    {
        if(hasBarrier) {
            audioManager.amPhotonView.RPC("RPCAudioManagerStopPlayerLoopSound", RpcTarget.All);
            GetComponent<PlayerAnimationController>().SetAnimatorBool("Power_Barrier", false);
            GetComponent<PlayerAnimationController>().DestroyParticle();
            hasBarrier = false;
        }

        Debug.Log("Starting turn!");
        IsActiveTurn = true;
        CurrTurnLength = turnTime;
        ActionsRemaining = 3;
        blocksLeftToPlace = 2;
        UIController.Instance.StartTurnSetUI(CurrTurnLength);
        UIController.Instance.SetBlocksLeftToBuild(blocksLeftToPlace);
    }

    private void EndTurn()
    {
        Debug.Log("Ending turn!");
        IsActiveTurn = false;
        UIController.Instance.EndTurnSetUI();
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
        UIController.Instance.PlayAnnouncement("Power Card", AnnouncementType.DropBounce);
        UIController.Instance.ToggleCardsButton(true);
        /*switch(roll)
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
        }*/
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
    IEnumerator MoveThroughWaypoints(BoardSpace[] waypoints, BoardSpace endSpace)
    {
        int currentWaypointIndex = 0;
        audioManager.amPhotonView.RPC("RPCAudioManagerPlayPlayerLoopSound", RpcTarget.All, "movement");
        while (currentWaypointIndex < waypoints.Length)
        {
            GetComponent<PlayerAnimationController>().SetAnimatorBool("Moving", true);
            Vector3 targetWorldPosition = waypoints[currentWaypointIndex].GetWorldPositionOfTopOfSpace();
            Vector3 targetBoardPosition = waypoints[currentWaypointIndex].GetPosInBoard();


            Debug.Log("Going to: " + targetBoardPosition + " current: " + currentSpace.GetPosInBoard());

            float distance = Vector3.Distance(transform.position, targetWorldPosition);

            if (targetBoardPosition.y > currentSpace.GetPosInBoard().y)
            {

                audioManager.amPhotonView.RPC("RPCAudioManagerTogglePausePlayerLoopSound", RpcTarget.All);
                audioManager.amPhotonView.RPC("RPCAudioManagerPlayPlayerOneShotSound", RpcTarget.All, "jump");
                Vector3 horizontalDirection = (new Vector3(targetWorldPosition.x, transform.position.y, targetWorldPosition.z) - transform.position).normalized;
                Quaternion horizontalLookRotation = Quaternion.LookRotation(horizontalDirection);
                transform.rotation = horizontalLookRotation;

                GetComponent<PlayerAnimationController>().PlayTriggeredAnimation("Climb_Up");

                yield return new WaitUntil(() => GetComponent<PlayerAnimationController>().CheckIfContinue());

                transform.position = targetWorldPosition;
                currentWaypointIndex++;
                BoardManager.Instance.BMPhotonView.RPC("RPCBoardManagerPlacePlayerOnSpace", RpcTarget.All, PhotonNetwork.LocalPlayer, GetComponent<PhotonView>().ViewID, targetBoardPosition, currentSpace.GetPosInBoard());
                audioManager.amPhotonView.RPC("RPCAudioManagerTogglePausePlayerLoopSound", RpcTarget.All);
                yield return null;
            }
            else if (targetBoardPosition.y < currentSpace.GetPosInBoard().y)
            {
                audioManager.amPhotonView.RPC("RPCAudioManagerTogglePausePlayerLoopSound", RpcTarget.All);
                audioManager.amPhotonView.RPC("RPCAudioManagerPlayPlayerOneShotSound", RpcTarget.All, "jump");
                Vector3 horizontalDirection = (new Vector3(targetWorldPosition.x, transform.position.y, targetWorldPosition.z) - transform.position).normalized;
                Quaternion horizontalLookRotation = Quaternion.LookRotation(horizontalDirection);
                transform.rotation = horizontalLookRotation;

                GetComponent<PlayerAnimationController>().PlayTriggeredAnimation("Climb_Down");

                yield return new WaitUntil(() => GetComponent<PlayerAnimationController>().CheckIfContinue());

                audioManager.amPhotonView.RPC("RPCAudioManagerPlayPlayerOneShotSound", RpcTarget.All, "land");
                
                transform.position = targetWorldPosition;
                currentWaypointIndex++;
                BoardManager.Instance.BMPhotonView.RPC("RPCBoardManagerPlacePlayerOnSpace", RpcTarget.All, PhotonNetwork.LocalPlayer, GetComponent<PhotonView>().ViewID, targetBoardPosition, currentSpace.GetPosInBoard());
                audioManager.amPhotonView.RPC("RPCAudioManagerTogglePausePlayerLoopSound", RpcTarget.All);
                yield return null;
            }
            else
            {
                Vector3 horizontalDirection = (new Vector3(targetWorldPosition.x, transform.position.y, targetWorldPosition.z) - transform.position).normalized;
                Quaternion horizontalLookRotation = Quaternion.LookRotation(horizontalDirection);
                transform.rotation = horizontalLookRotation;

                transform.position = Vector3.MoveTowards(transform.position, targetWorldPosition, moveSpeed * Time.deltaTime);

                if (distance < 0.1f)
                {
                    currentWaypointIndex++;
                    BoardManager.Instance.BMPhotonView.RPC("RPCBoardManagerPlacePlayerOnSpace", RpcTarget.All, PhotonNetwork.LocalPlayer, GetComponent<PhotonView>().ViewID, targetBoardPosition, currentSpace.GetPosInBoard());
                }

                yield return null;
            }
        }
        GetComponent<PlayerAnimationController>().SetAnimatorBool("Moving", false);
        audioManager.amPhotonView.RPC("RPCAudioManagerStopPlayerLoopSound", RpcTarget.All);
        moving = false;
    }
    #endregion

    #region Grapple
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
        BoardManager.Instance.BMPhotonView.RPC("RPCBoardManagerPlacePlayerOnSpace", RpcTarget.All, PhotonNetwork.LocalPlayer, GetComponent<PhotonView>().ViewID, target.GetPosInBoard(), currentSpace.GetPosInBoard());
    }
    #endregion
    
    #region Wind
    public void GetPushedByWind(WindDir dir) {
        if(!hasBarrier) {
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
    }

    IEnumerator GetPushed(BoardSpace spaceToLandOn) 
    {
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
        BoardManager.Instance.BMPhotonView.RPC("RPCBoardManagerPlacePlayerOnSpace", RpcTarget.All, PhotonNetwork.LocalPlayer, GetComponent<PhotonView>().ViewID, spaceToLandOn.GetPosInBoard(), currentSpace.GetPosInBoard());
    }

    #endregion

    #region  Cards

    public void UseCard(CardType type) {
        StopAllCardAnimations();
        Board.Instance.selectionMode = SelectionMode.None;
        switch(type){
            case CardType.Switch:
                EnableOtherPlayerColliders();
                GetComponent<PlayerAnimationController>().SetAnimatorBool("Power_Switch_Ready", true);
                UIController.Instance.ToggleCardsScreen();
                break;
            case CardType.Punch:
                EnableOtherPlayerColliders();
                GetComponent<PlayerAnimationController>().SetAnimatorBool("Power_Punch_Ready", true);
                UIController.Instance.ToggleCardsScreen();
                break;
            case CardType.TimeStop:
                EnableOtherPlayerColliders();
                GetComponent<PlayerAnimationController>().SetAnimatorBool("Power_Time_Stop_Ready", true);
                UIController.Instance.ToggleCardsScreen();
                break;
            case CardType.Levitate:
                StartCoroutine(DoLevitate());
                UIController.Instance.ToggleCardsScreen();
                break;
            case CardType.Barrier:
                audioManager.amPhotonView.RPC("RPCAudioManagerPlayPlayerOneShotSound", RpcTarget.All, "barrier");
                GetComponent<PlayerAnimationController>().SetAnimatorBool("Power_Barrier", true);
                UIController.Instance.ToggleCardsScreen();
                UIController.Instance.ToggleCardsButton(false);
                hasBarrier = true;
                ActionsRemaining = 0;
                audioManager.amPhotonView.RPC("RPCAudioManagerPlayPlayerLoopSound", RpcTarget.All, "barrier");
                break;
            case CardType.Taunt:
                audioManager.amPhotonView.RPC("RPCAudioManagerPlayPlayerOneShotSound", RpcTarget.All, "taunt-flex");
                GetComponent<PlayerAnimationController>().PlayTriggeredAnimation("Power_Taunt");
                UIController.Instance.ToggleCardsScreen();
                DoTaunt();
                break;
            case CardType.Ninja:
                GetComponent<PlayerAnimationController>().SetAnimatorBool("Power_Ninja_Ready", true);
                UIController.Instance.ToggleCardsScreen();
                Board.Instance.selectionMode = SelectionMode.Ninja;
            break;
        }  
    }

    public void DoTaunt() {
        List<BoardSpace> spacesWithPlayers = Board.Instance.GetPlayerObjectsAroundSpace(currentSpace);
        if(spacesWithPlayers.Count != 0) {
            foreach(BoardSpace space in spacesWithPlayers) {
                space.GetPlayerObjOnSpace().gameObject.GetComponent<PhotonView>().RPC("testRPC", RpcTarget.All, space.GetPlayerOnSpace(), "Cry");
            }
        }
        UIController.Instance.ToggleCardsButton(false);
        ActionsRemaining -= 1;
    }

    IEnumerator DoLevitate() {
        GetComponent<PlayerAnimationController>().PlayTriggeredAnimation("Power_Levitate");
        yield return new WaitUntil(() => GetComponent<PlayerAnimationController>().CheckIfContinue());
        BoardManager.Instance.BMPhotonView.RPC("RPCBoardManagerPlacePlayerOnSpace", RpcTarget.All, PhotonNetwork.LocalPlayer, GetComponent<PhotonView>().ViewID, new Vector3(currentSpace.GetPosInBoard().x, currentSpace.GetPosInBoard().y + 1, currentSpace.GetPosInBoard().z), currentSpace.GetPosInBoard());
        transform.position = currentSpace.GetWorldPositionOfTopOfSpace();
        UIController.Instance.ToggleCardsButton(false);
        ActionsRemaining -= 1;
    }

    public void DoNinja(BoardSpace target) {
        StartCoroutine(DoNinjaMovement(target, 10f));
    }

    IEnumerator DoNinjaMovement(BoardSpace target, float speed)
    {
        GetComponent<PlayerAnimationController>().SetAnimatorBool("Power_Ninja", true);
        GetComponent<PlayerAnimationController>().SetAnimatorBool("Power_Ninja_Ready", false);
        Vector3 startPosition = transform.position;
        Vector3 targetPos = target.GetWorldPositionOfTopOfSpace();

        Vector3 horizontalDirection = (new Vector3(targetPos.x, transform.position.y, targetPos.z) - transform.position).normalized;
        Quaternion horizontalLookRotation = Quaternion.LookRotation(-horizontalDirection);
        transform.rotation = horizontalLookRotation;

        float totalDistance = Vector3.Distance(startPosition, targetPos);

        float peakHeight = Mathf.Max(startPosition.y, targetPos.y) + 2f; // Adjust the peak height as needed

        float elapsedTime = 0f;

        audioManager.amPhotonView.RPC("RPCAudioManagerPlayPlayerLoopSound", RpcTarget.All, "ninja");

        while (elapsedTime < totalDistance / speed)
        {
            float t = elapsedTime / (totalDistance / speed);

            // Horizontal movement
            float x = Mathf.Lerp(startPosition.x, targetPos.x, t);
            float z = Mathf.Lerp(startPosition.z, targetPos.z, t);

            // Vertical movement using a parabolic motion
            float y = Mathf.Lerp(startPosition.y, targetPos.y, t) + Mathf.Sin(t * Mathf.PI) * peakHeight;

            Vector3 newPosition = new Vector3(x, y, z);
            transform.position = newPosition;

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        GetComponent<PlayerAnimationController>().SetAnimatorBool("Power_Ninja", false);
        // Ensure the object reaches the final position exactly
        transform.position = targetPos;
        BoardManager.Instance.BMPhotonView.RPC("RPCBoardManagerPlacePlayerOnSpace", RpcTarget.All, PhotonNetwork.LocalPlayer, GetComponent<PhotonView>().ViewID, target.GetPosInBoard(), currentSpace.GetPosInBoard());
        UIController.Instance.ToggleCardsButton(false);

        audioManager.amPhotonView.RPC("RPCAudioManagerPlayPlayerOneShotSound", RpcTarget.All, "land");
        audioManager.amPhotonView.RPC("RPCAudioManagerStopPlayerLoopSound", RpcTarget.All);

        ActionsRemaining -= 1;
    }

    public static float CalculateParabolaY(Vector3 startPosition, Vector3 endPosition, float xValue)
    {
        // Normalize the x value between 0 and 1 based on the start and end positions
        float t = Mathf.InverseLerp(startPosition.x, endPosition.x, xValue);

        // Calculate the parabolic height
        float parabolicHeight = 4f * t * (1 - t);

        // Calculate the y value on the parabola
        float y = Mathf.Lerp(startPosition.y, endPosition.y, t) + parabolicHeight;

        return y;
    }

    public void StopAllCardAnimations() {
        GetComponent<PlayerAnimationController>().DestroyParticle();
        GetComponent<PlayerAnimationController>().DestroyProp();
        GetComponent<PlayerAnimationController>().SetAnimatorBool("Power_Switch_Ready", false);
        GetComponent<PlayerAnimationController>().SetAnimatorBool("Power_Punch_Ready", false);
        GetComponent<PlayerAnimationController>().SetAnimatorBool("Power_Time_Stop_Ready", false);
        GetComponent<PlayerAnimationController>().SetAnimatorBool("Power_Ninja_Ready", false);
    }

    void EnableOtherPlayerColliders() {
        GameObject[] otherPlayers = GameObject.FindGameObjectsWithTag("OtherPlayer");
        foreach(GameObject op in otherPlayers) {
            if(op.GetComponent<PlayerClickOnHandler>() != null) {
                op.GetComponent<PlayerClickOnHandler>().ToggleSelectability(true);
                op.GetComponent<PlayerClickOnHandler>().OnPlayerClicked += HandlePlayerClickedOn;
            }
        }
    }
    void HandlePlayerClickedOn(GameObject playerClicked) {
        Debug.Log("You clicked on player: " + playerClicked.GetComponent<PlayerNetworkController>().PCPhotonView.Owner.NickName);
    }

    public void FreeInTime() {
        if(!hasBarrier) {
            frozen = true;
        }

    }

    #endregion
}
