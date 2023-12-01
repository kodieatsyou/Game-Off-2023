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

    public bool isTaunted = false;
    private bool hasBarrier = false;

    public BoardSpace currentSpace;

    //Moving
    public int moveSpeed = 5;

    public int fallSpeed = 12;
    public int rotationSpeed = 10;
    public bool moving = false;

    public AudioManager audioManager;

    private bool countDownPlayed = false;

    private GameObject currentCardInUse = null;

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
        if(isTaunted) {
            ActionsRemaining = 2; // roll, build
        } else {
            ActionsRemaining = 3; // move, roll, build
        }
        
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
            if (CurrTurnLength <= 4 && !countDownPlayed)
            {
                audioManager.amPhotonView.RPC("RPCAudioManagerPlayPlayerOneShotSound", RpcTarget.All, "count-down");
                countDownPlayed = true;
            }
            if (ActionsRemaining <= 0)
            {
                EndTurn();
                countDownPlayed = false;
                UIController.Instance.PlayAnnouncement("Out of Actions!", AnnouncementType.DropBounce);
                GameManagerTest.Instance.GMPhotonView.RPC("RPCGameManagerPlayerEndedTurn", RpcTarget.All);
            } else if(CurrTurnLength <= 0f) {
                GetComponent<AudioManager>().HandlePlayerOneShotSound("time-up");
                EndTurn();
                countDownPlayed = false;
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

        if(frozen) {
            UIController.Instance.PlayAnnouncement("You are frozen for this turn!", AnnouncementType.ScrollLR);
            EndTurn();
            GameManagerTest.Instance.GMPhotonView.RPC("RPCGameManagerPlayerEndedTurn", RpcTarget.All);
            return;
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
        if(frozen) {
            frozen = false;
        }
        if(isTaunted) {
            isTaunted = false;
        }
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
        switch(roll)
        {
            case 1:
                UIController.Instance.PlayAnnouncement("Wind", AnnouncementType.DropBounce);
                UIController.Instance.ToggleWindButton(true);
                break;
            case 2:
                if(isTaunted) {
                    UIController.Instance.PlayAnnouncement("Grapple Blocked by Taunt!", AnnouncementType.DropBounce);
                    ActionsRemaining -= 1;
                } else {
                    UIController.Instance.PlayAnnouncement("Grapple", AnnouncementType.DropBounce);
                }
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
            BoardSpace[] path = new AStarPathfinding(currentSpace, spaceToMoveTo).FindPath().ToArray();
            if(path == null) {
                //No valid path need to do teleport.
                return;
            } else {
                StartCoroutine(MoveThroughWaypoints(path, spaceToMoveTo));
            }
            
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
            audioManager.amPhotonView.RPC("RPCAudioManagerPlayPlayerOneShotSound", RpcTarget.All, "taunt-sad");
            GetComponent<PlayerAnimationController>().SetAnimatorBool("Wind_Fall", false);
            falling = false;
            Debug.Log("DONE!");
        }
        transform.position = spaceToLandOn.GetWorldPositionOfTopOfSpace();
        BoardManager.Instance.BMPhotonView.RPC("RPCBoardManagerPlacePlayerOnSpace", RpcTarget.All, PhotonNetwork.LocalPlayer, GetComponent<PhotonView>().ViewID, spaceToLandOn.GetPosInBoard(), currentSpace.GetPosInBoard());
        audioManager.amPhotonView.RPC("RPCAudioManagerPlayPlayerOneShotSound", RpcTarget.All, "land");
    }

    #endregion

    #region  Cards

    public void UseCard(GameObject cardOBJ, CardType type) {
        StopAllCardAnimations();
        Board.Instance.selectionMode = SelectionMode.None;
        /*
        case CardType.Switch:
                EnableOtherPlayerColliders();
                GetComponent<PlayerAnimationController>().SetAnimatorBool("Power_Switch_Ready", true);
                UIController.Instance.ToggleCardsScreen(false);
                break;
                */
        switch(type){
            case CardType.Punch:
                StopAllCardAnimations();
                GetComponent<PlayerAnimationController>().SetAnimatorBool("Power_Punch_Ready", true);
                UIController.Instance.ToggleCardsScreen(false);
                currentCardInUse = cardOBJ;
                DoPunch();
                break;
            case CardType.TimeStop:
                StopAllCardAnimations();
                GetComponent<PlayerAnimationController>().SetAnimatorBool("Power_Time_Stop_Ready", true);
                UIController.Instance.ToggleCardsScreen(false);
                currentCardInUse = cardOBJ;
                DoTimeStop();
                break;
            case CardType.Levitate:
                StopAllCardAnimations();
                StartCoroutine(DoLevitate());
                UIController.Instance.ToggleCardsButton(false);
                ActionsRemaining -= 1;
                UIController.Instance.ToggleCardsScreen(false);
                UIController.Instance.RemoveCard(cardOBJ);
                break;
            case CardType.Barrier:
                StopAllCardAnimations();
                audioManager.amPhotonView.RPC("RPCAudioManagerPlayPlayerOneShotSound", RpcTarget.All, "barrier");
                GetComponent<PlayerAnimationController>().SetAnimatorBool("Power_Barrier", true);
                UIController.Instance.ToggleCardsScreen(false);
                UIController.Instance.ToggleCardsButton(false);
                hasBarrier = true;
                ActionsRemaining = 0;
                UIController.Instance.RemoveCard(cardOBJ);
                audioManager.amPhotonView.RPC("RPCAudioManagerPlayPlayerLoopSound", RpcTarget.All, "barrier");
                break;
            case CardType.Taunt:
                StopAllCardAnimations();
                audioManager.amPhotonView.RPC("RPCAudioManagerPlayPlayerOneShotSound", RpcTarget.All, "taunt-flex");
                GetComponent<PlayerAnimationController>().PlayTriggeredAnimation("Power_Taunt");
                UIController.Instance.ToggleCardsScreen(false);
                UIController.Instance.ToggleCardsButton(false);
                ActionsRemaining -= 1;
                UIController.Instance.RemoveCard(cardOBJ);
                DoTaunt();
                break;
            case CardType.Ninja:
                StopAllCardAnimations();
                GetComponent<PlayerAnimationController>().SetAnimatorBool("Power_Ninja_Ready", true);
                UIController.Instance.ToggleCardsScreen(false);
                Board.Instance.selectionMode = SelectionMode.Ninja;
                currentCardInUse = cardOBJ;
            break;
        }  
    }

    public void GetTaunted(string nameOfPlayerWhoTaunted) {
        isTaunted = true;
        UIController.Instance.PlayAnnouncement("You were taunted by " + nameOfPlayerWhoTaunted + "!", AnnouncementType.DropBounce);
    }

    public void DoPunch() {
        List<BoardSpace> spacesWithPlayers = Board.Instance.GetPlayerObjectsAroundSpace(currentSpace);
        if(spacesWithPlayers.Count != 0) {
            foreach(BoardSpace space in spacesWithPlayers) {
                Debug.Log(space);
                space.GetPlayerObjOnSpace().GetComponent<PlayerClickOnHandler>().ToggleSelectability(true);
                space.GetPlayerObjOnSpace().GetComponent<PlayerClickOnHandler>().OnPlayerClicked += HandlePlayerPunched;
            }
        }
    }

    public void DoTimeStop() {
        GameObject[] otherPlayers = GameObject.FindGameObjectsWithTag("OtherPlayer");
        Debug.Log("OTHER PLAYERS: " + otherPlayers.Length);
        foreach(GameObject op in otherPlayers) {
            Debug.Log(op.name);
            if(op.GetComponent<PlayerClickOnHandler>() != null) {
                op.GetComponent<PlayerClickOnHandler>().ToggleSelectability(true);
                op.GetComponent<PlayerClickOnHandler>().OnPlayerClicked += HandlePlayerFrozen;
            }
        }
    }

    public void HandlePlayerPunched(GameObject playerObj) {
        Vector3 horizontalDirection = (new Vector3(playerObj.transform.position.x, transform.position.y, playerObj.transform.position.z) - transform.position).normalized;
        Quaternion horizontalLookRotation = Quaternion.LookRotation(horizontalDirection);
        transform.rotation = horizontalLookRotation;
        
        GameObject[] otherPlayers = GameObject.FindGameObjectsWithTag("OtherPlayer");
        foreach(GameObject op in otherPlayers) {
            if(op.GetComponent<PlayerClickOnHandler>() != null) {
                op.GetComponent<PlayerClickOnHandler>().ToggleSelectability(false);
                op.GetComponent<PlayerClickOnHandler>().OnPlayerClicked -= HandlePlayerPunched;
            }
        }
        Debug.Log("Player Punched!");
        GetComponent<PlayerAnimationController>().PlayTriggeredAnimation("Power_Punch", "Power_Punch_Ready");
        UIController.Instance.ToggleCardsButton(false);
        ActionsRemaining -= 1;
        UIController.Instance.RemoveCard(currentCardInUse);
        currentCardInUse = null;
        playerObj.GetComponent<PhotonView>().RPC("RPCPlayerPunched", RpcTarget.Others, playerObj.GetComponent<PhotonView>().Owner, currentSpace.GetPosInBoard());
    }

    public void HandlePlayerFrozen(GameObject playerObj) {
        GameObject[] otherPlayers = GameObject.FindGameObjectsWithTag("OtherPlayer");
        foreach(GameObject op in otherPlayers) {
            if(op.GetComponent<PlayerClickOnHandler>() != null) {
                op.GetComponent<PlayerClickOnHandler>().ToggleSelectability(false);
                op.GetComponent<PlayerClickOnHandler>().OnPlayerClicked -= HandlePlayerFrozen;
            }
        }
        Debug.Log("Player Frozen!");
        GetComponent<PlayerAnimationController>().PlayTriggeredAnimation("Power_Time_Stop");
        StopAllCardAnimations();
        UIController.Instance.ToggleCardsButton(false);
        ActionsRemaining -= 1;
        UIController.Instance.RemoveCard(currentCardInUse);
        currentCardInUse = null;
        playerObj.GetComponent<PhotonView>().RPC("RPCPlayerFrozen", RpcTarget.Others, playerObj.GetComponent<PhotonView>().Owner);
        //GetComponent<PlayerAnimationController>().SetAnimatorBool("Power_Punch_Ready", false);
    }

    [PunRPC]
    void RPCPlayerPunched(Player player, Vector3 puncherBlock) {
        Debug.Log("PUNCHED");
        if(player == GetComponent<PhotonView>().Owner) {
            puncherBlock = Board.Instance.boardArray[(int)puncherBlock.x, (int)puncherBlock.y, (int)puncherBlock.z].GetPosInBoard();
            BoardSpace punchedToBlock = null;
            if(puncherBlock.x > currentSpace.GetPosInBoard().x) {
                punchedToBlock = Board.Instance.GetWindPushBlock(currentSpace, WindDir.North);
                transform.rotation = Quaternion.Euler(0, 90, 0);
            } else if (puncherBlock.x < currentSpace.GetPosInBoard().x) {
                punchedToBlock = Board.Instance.GetWindPushBlock(currentSpace, WindDir.South);
                transform.rotation = Quaternion.Euler(0, -90, 0);
            } else if (puncherBlock.z > currentSpace.GetPosInBoard().z) {
                punchedToBlock = Board.Instance.GetWindPushBlock(currentSpace, WindDir.West);
                transform.rotation = Quaternion.Euler(0, 0, 0);
            } else {
                punchedToBlock = Board.Instance.GetWindPushBlock(currentSpace, WindDir.East);
                transform.rotation = Quaternion.Euler(0, 180, 0);
            }
            Debug.Log("Punched to block: " + punchedToBlock.GetPosInBoard());
            StartCoroutine(GetPunched(punchedToBlock));
        }
    }

    IEnumerator GetPunched(BoardSpace spaceToLandOn) 
    {
        bool falling = false;
        if(spaceToLandOn.GetPosInBoard().y < currentSpace.GetPosInBoard().y - 1) {
            GetComponent<PlayerAnimationController>().SetAnimatorBool("Get_Punched_Fall", true);
            falling = true;
        }
        if(spaceToLandOn == currentSpace) {
            GetComponent<PlayerAnimationController>().PlayTriggeredAnimation("Get_Punched_Into_Block");
            yield break;
        }
        GetComponent<PlayerAnimationController>().PlayTriggeredAnimation("Get_Punched");
        yield return new WaitUntil(() => GetComponent<PlayerAnimationController>().CheckIfContinue());
        if(falling) {
            float distance = Vector3.Distance(transform.position, spaceToLandOn.GetWorldPositionOfTopOfSpace());
            while (distance > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, spaceToLandOn.GetWorldPositionOfTopOfSpace(), fallSpeed * Time.deltaTime);
                distance = Vector3.Distance(transform.position, spaceToLandOn.GetWorldPositionOfTopOfSpace());
                yield return null;
            }
            GetComponent<PlayerAnimationController>().SetAnimatorBool("Get_Punched_Fall", false);
            falling = false;
            Debug.Log("DONE!");
        }
        transform.position = spaceToLandOn.GetWorldPositionOfTopOfSpace();
        StopAllCardAnimations();
        BoardManager.Instance.BMPhotonView.RPC("RPCBoardManagerPlacePlayerOnSpace", RpcTarget.All, PhotonNetwork.LocalPlayer, GetComponent<PhotonView>().ViewID, spaceToLandOn.GetPosInBoard(), currentSpace.GetPosInBoard());
    }

    public void DoTaunt() {
        List<BoardSpace> spacesWithPlayers = Board.Instance.GetPlayerObjectsAroundSpace(currentSpace);
        if(spacesWithPlayers.Count != 0) {
            foreach(BoardSpace space in spacesWithPlayers) {
                space.GetPlayerObjOnSpace().GetComponent<PhotonView>().RPC("RPCPlayerTaunted", RpcTarget.Others, space.GetPlayerOnSpace());
                //space.GetPlayerObjOnSpace().gameObject.GetComponent<PhotonView>().RPC("RPCPlayerAnimationControllerPlayTriggeredAnimation", RpcTarget.All, space.GetPlayerOnSpace(), "Cry");
            }
        }
    }

    [PunRPC]
    void RPCPlayerTaunted(Player player) {
        Debug.Log("Taunted!");
        if(player == GetComponent<PhotonView>().Owner) {
            GetComponent<PlayerAnimationController>().PlayTriggeredAnimation("Cry");
            isTaunted = true;
            UIController.Instance.PlayAnnouncement("You were taunted!", AnnouncementType.DropBounce);
        }
    }

    [PunRPC]
    void RPCPlayerFrozen(Player player) {
        Debug.Log("Frozen!");
        if(player == GetComponent<PhotonView>().Owner) {
            frozen = true;
            UIController.Instance.PlayAnnouncement("You were frozen in time!", AnnouncementType.DropBounce);
        }
    }

    IEnumerator DoLevitate() {
        GetComponent<PlayerAnimationController>().PlayTriggeredAnimation("Power_Levitate");
        yield return new WaitUntil(() => GetComponent<PlayerAnimationController>().CheckIfContinue());
        BoardManager.Instance.BMPhotonView.RPC("RPCBoardManagerPlacePlayerOnSpace", RpcTarget.All, PhotonNetwork.LocalPlayer, GetComponent<PhotonView>().ViewID, new Vector3(currentSpace.GetPosInBoard().x, currentSpace.GetPosInBoard().y + 1, currentSpace.GetPosInBoard().z), currentSpace.GetPosInBoard());
        transform.position = currentSpace.GetWorldPositionOfTopOfSpace();
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
        UIController.Instance.RemoveCard(currentCardInUse);
        currentCardInUse = null;

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
        GetComponent<PlayerAnimationController>().SetAnimatorBool("Power_Punch_Ready", false);
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

    void DisableOtherPlayerColliders() {
        GameObject[] otherPlayers = GameObject.FindGameObjectsWithTag("OtherPlayer");
        foreach(GameObject op in otherPlayers) {
            if(op.GetComponent<PlayerClickOnHandler>() != null) {
                op.GetComponent<PlayerClickOnHandler>().ToggleSelectability(false);
                op.GetComponent<PlayerClickOnHandler>().OnPlayerClicked -= HandlePlayerClickedOn;
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
