using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class PlayerController: MonoBehaviourPunCallbacks
{
    public static PlayerController Instance;

    private int PlayerID;
    private string PlayerName;
    private float NetworkTurnLength;
    private float CurrTurnLength;
    private int ActionsRemaining;
    private bool IsActiveTurn;
    private GameManager GM = GameManager.Instance;
    private UIController UI;
    public BoardSpace currentSpace;

    public int moveSpeed = 5;
    public int rotationSpeed = 10;
    

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
    #endregion

    #region Pathfinding

    public void MoveTo(BoardSpace spaceToMoveTo)
    {
        BoardSpace oldSpace = currentSpace;
        currentSpace = spaceToMoveTo;
        oldSpace.PlacePlayerOnSpace(null);
        StartCoroutine(MoveThroughWaypoints(new AStarPathfinding(oldSpace, spaceToMoveTo).FindPath().ToArray()));
    }
    IEnumerator MoveThroughWaypoints(Vector3[] waypoints)
    {
        int currentWaypointIndex = 0;

        while (currentWaypointIndex < waypoints.Length)
        {
            GetComponent<PlayerAnimationController>().SetAnimatorBool("Moving", true);
            // Calculate the distance to the next waypoint
            float distance = Vector3.Distance(transform.position, waypoints[currentWaypointIndex]);

            // Check if the next waypoint is one y level up
            if (waypoints[currentWaypointIndex].y > transform.position.y)
            {
                // Rotate horizontally towards the waypoint before climbing
                Vector3 horizontalDirection = (new Vector3(waypoints[currentWaypointIndex].x, transform.position.y, waypoints[currentWaypointIndex].z) - transform.position).normalized;
                Quaternion horizontalLookRotation = Quaternion.LookRotation(horizontalDirection);
                transform.rotation = horizontalLookRotation;

                // Play climbing animation
                GetComponent<PlayerAnimationController>().PlayTriggeredAnimation("Climb_Up");

                // Wait for animation to complete
                yield return new WaitUntil(() => GetComponent<PlayerAnimationController>().CheckIfContinue());

                transform.position = waypoints[currentWaypointIndex];
                currentWaypointIndex++;
                yield return null;
            }
            else if (waypoints[currentWaypointIndex].y < transform.position.y)
            {
                // Rotate horizontally towards the waypoint before climbing
                Vector3 horizontalDirection = (new Vector3(waypoints[currentWaypointIndex].x, transform.position.y, waypoints[currentWaypointIndex].z) - transform.position).normalized;
                Quaternion horizontalLookRotation = Quaternion.LookRotation(horizontalDirection);
                transform.rotation = horizontalLookRotation;

                // Play climbing animation
                GetComponent<PlayerAnimationController>().PlayTriggeredAnimation("Climb_Down");

                // Wait for animation to complete
                yield return new WaitUntil(() => GetComponent<PlayerAnimationController>().CheckIfContinue());

                transform.position = waypoints[currentWaypointIndex];
                currentWaypointIndex++;
                yield return null;
            }
            else
            {
                // Move towards the waypoint
                transform.position = Vector3.MoveTowards(transform.position, waypoints[currentWaypointIndex], moveSpeed * Time.deltaTime);

                // Rotate towards the waypoint
                Vector3 direction = (waypoints[currentWaypointIndex] - transform.position).normalized;
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);

                // If the GameObject is close enough to the waypoint, move to the next waypoint
                if (distance < 0.1f)
                {
                    currentWaypointIndex++;
                }
                yield return null; // Yield control until the next frame
            }
        }

        GetComponent<PlayerAnimationController>().SetAnimatorBool("Moving", false);
        currentSpace.PlacePlayerOnSpace(this.gameObject);
    }
    #endregion

}
