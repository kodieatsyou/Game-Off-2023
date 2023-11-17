using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public enum GameState
{
    Menu,
    Gameplay,
    GameOver
}

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;
    public static event System.Action<GameState> OnStateChange;
    public float turnTime = 15.0f;
    public PlayerController[] PlayerArr { get; private set; }

    private static BoardManager Board;
    private float currentTurnTime;
    private GameState CurrentState;
    private int CurrentPlayerIndex;

    void Awake() 
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
            Board = BoardManager.Instance;
        }
        else {
            Destroy(this);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        CurrentPlayerIndex = 0;

        UpdateGameState(GameState.Menu); // Launch the menu after placing players, let another script handle when to start gameplay by chaning the state to GameState.Gameplay

        if (PhotonNetwork.IsMasterClient)
        {
            StartNewTurn();
        }

        PlayerArr = new PlayerController[6]; // TODO This should be reference to the variable which controls the 
    }
    
    // Update is called once per frame
    void Update()
    {
        if (CurrentState == GameState.Menu)
        {
            // TODO display menu scene
        }
        else if (CurrentState == GameState.Gameplay)
        {
            PlayerController currPlayer = PlayerArr[CurrentPlayerIndex];

            Debug.Log("Current player: " + currPlayer.PlayerName);
            currentTurnTime -= Time.deltaTime;
            if (currentTurnTime <= 0.0f || currPlayer.ActionsRemaining < 0)
            {

            }
        }
        else if (CurrentState == GameState.GameOver)
        {
            // Display the game over screen
        }
    }

    // Called everytime there is a switch in the state of the game.
    public void UpdateGameState(GameState newState)
    {
        if (newState != CurrentState)
        {
            CurrentState = newState;
            switch (newState)
            {
                case GameState.Menu:
                    break;

                case GameState.Gameplay:
                    Debug.Log("State is now Gameplay");
                    if (CurrentPlayerIndex == -1) // first move
                    {
                        Debug.Log("Starting the first turn.");
                        CurrentPlayerIndex = 0;
                        StartNewPlayerTurn();
                    }
                    Debug.Log("Ended player turn, new player: " + CurrentPlayerIndex);
                    break;

                case GameState.GameOver:
                    Debug.Log("State is now GameOver");
                    break;

                default:
                    Debug.Log("Invalid Game State");
                    throw new System.Exception("Invalid Game State");
            }
            OnStateChange?.Invoke(newState);
        }
        else
        {
            Debug.Log("No new state detected.");
        }
    }

    // A new player has entered the room, instantiate their player prefab
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        Vector3 spawnPoint = new Vector3(0.0f, 1.0f, 0.0f); // TODO THIS SHOULD COME FROM A METHOD IN THE BOARD MANAGER

        int nick = newPlayer.NickName;

        GameObject player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint, Quaternion.identity);

        PlayerController playerControl = new PlayerController();
        playerControl.Spawn(player, nick, spawnPoint);

        int actorNum = newPlayer.ActorNumber;
        PlayerArr[actorNum] = playerControl;
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        // Handle player leaving the room
    }

    private void ChoosePlayerOrder()
    {
        Debug.Log("Generating Player Order");
        // TODO Update to the dice rolling game to determine player order
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < playerObjects.Length; i++)
        {
            //PlayerArr[i] = playerObjects[i];
        }
    }

    void StartNewPlayerTurn()
    {
        // Reset turn timer and action count
        currentTurnTime = turnTime;
        PlayerArr[CurrentPlayerIndex].StartTurn();
        // Perform any other initialization for the new turn
        Debug.Log("Player " + CurrentPlayerIndex + "'s turn started.");
    }

    void EndTurn()
    {
        // Perform any end-of-turn actions or cleanup
        Debug.Log("Player " + CurrentPlayerIndex + "'s turn ended.");

        // Switch to the next player
        SwitchToNextPlayer();
    }

    void SwitchToNextPlayer()
    {
        CurrentPlayerIndex = (CurrentPlayerIndex + 1) % MaxPlayerCount;

        // Start a new turn for the next player
        StartNewPlayerTurn();
    }
}
