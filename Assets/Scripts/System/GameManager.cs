using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    NewGame,
    OrderChoosing,
    StartPlayerTurn,
    EndPlayerTurn, // Used for updating games where they are not the current player
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public static event System.Action<GameState> OnStateChange;

    private Player[] Players;
    private GameState CurrentState;
    private BoardManager Board;
    private int CurrentPlayerIndex;
    private Player CurrentPlayer;

    private void Awake() 
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
    private void Start()
    {
        Board = BoardManager.Instance;
        UpdateGameState(GameState.OrderChoosing);
    }
    
    // Update is called once per frame
    private void Update()
    {
        if (CurrentState != GameState.OrderChoosing)
        {
            ProcessGameCondition(); // Updates game state to GameOver if condition found
            if (CurrentState == GameState.GameOver)
            {
                // TODO display game over text
            }
            else
            {
                if (CurrentState == GameState.EndPlayerTurn) // if previous player has now ended turn
                {
                    UpdateGameState(GameState.StartPlayerTurn);
                }
                else if (CurrentPlayer.IsActiveTurn == false) // current player turn is ended by its own logic
                {
                    UpdateGameState(GameState.EndPlayerTurn);
                }
            }
        }
    }

    private void ProcessGameCondition()
    {
        bool winDetected = false;
        bool gameOverDetected = false;
        // TODO Check for win conditions
        if (winDetected || gameOverDetected)
        {
            UpdateGameState(GameState.GameOver);
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
                case GameState.OrderChoosing:
                    Debug.Log("Changing state to OrderChoosing");
                    ChoosePlayerOrder();
                    break;

                case GameState.StartPlayerTurn: // allow current player to interact
                    Debug.Log("Changing state to StartPlayerTurn");
                    CurrentPlayer = Players[CurrentPlayerIndex];
                    CurrentPlayer.StartTurn();
                    Debug.Log("Started turn for player: " + CurrentPlayerIndex);
                    break;

                case GameState.EndPlayerTurn: // switch to the next player and notify all players
                    Debug.Log("Changing state to EndPlayerTurn");
                    CurrentPlayer.EndTurn(); // end current player turn

                    CurrentPlayerIndex = (CurrentPlayerIndex + 1) % Players.Length;
                    CurrentPlayer = Players[CurrentPlayerIndex];
                    Debug.Log("Ended player turn, new player: " + CurrentPlayerIndex);
                    break;

                case GameState.GameOver:
                    Debug.Log("Changing state to GameOver");
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
        // TODO Update to the dice rolling game to determine player order
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        int numPlayers = playerObjects.Length;
        for(int i=0; i < numPlayers; i++)
        {
            Player player = new Player();
            player.Object = playerObjects[i];
            Players[i] = player;
        }
        UpdateGameState(GameState.StartPlayerTurn);
    }
}
