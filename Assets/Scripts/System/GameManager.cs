using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    Menu,
    Gameplay,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public static event System.Action<GameState> OnStateChange;
    public int MaxPlayerCount = 6;
    public float turnTime = 15.0f;
    public Player[] PlayerArr { get; private set; }

    private GameState CurrentState;
    private BoardManager Board;
    private int CurrentPlayerIndex;

    void Awake() 
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else {
            Destroy(this);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        PlayerArr = new Player[MaxPlayerCount];
        SpawnPlayers(); // populates PlayerArr
        CurrentPlayerIndex = -1;

        foreach (Player p in PlayerArr)
        {
            Debug.Log("Player: " + p.Name);
        }

        UpdateGameState(GameState.Menu); // Launch the menu after placing players, let another script handle when to start gameplay by chaning the state to GameState.Gameplay
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
            Debug.Log("Current player: " + PlayerArr[CurrentPlayerIndex].Name);
            currentTurnTime -= Time.deltaTime;
            if (currentTurnTime <= 0.0f || PlayerArr[CurrentPlayerIndex].currentActions < 0)
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

    private void ChoosePlayerOrder()
    {
        Debug.Log("Generating Player Order");
        // TODO Update to the dice rolling game to determine player order
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < playerObjects.Length; i++)
        {
            PlayerArr[i] = playerObjects[i];
        }
    }

    void StartNewPlayerTurn()
    {
        // Reset turn timer and action count
        currentTurnTime = turnTime;
        PlayerArr[CurrentPlayerIndex].StartTurn();
        // Perform any other initialization for the new turn
        Debug.Log("Player " + currentPlayerIndex + "'s turn started.");
    }

    void EndTurn()
    {
        // Perform any end-of-turn actions or cleanup
        Debug.Log("Player " + currentPlayerIndex + "'s turn ended.");

        // Switch to the next player
        SwitchToNextPlayer();
    }

    void SwitchToNextPlayer()
    {
        currentPlayerIndex = (currentPlayerIndex + 1) % numPlayers;

        // Start a new turn for the next player
        StartNewPlayerTurn();
    }
}
