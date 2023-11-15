using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    StartPlayerTurn,
    EndPlayerTurn, // Used for updating games where they are not the current player
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public static event System.Action<GameState> OnStateChange;
    public float TurnLength = 15f;

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
        CurrentPlayerIndex = 0;
        UpdateGameState(GameState.StartPlayerTurn);
    }
    
    // Update is called once per frame
    private void Update()
    {
        ProcessGameCondition(); // Updates game state to GameOver if condition found

        if (CurrentState == GameState.GameOver)
        {
            // TODO display game over text
        }
        else
        {
            // All below processes update game state to EndPlayerTurn if found
            ProcessTurnTime();
            ProcessPlayerTurn();
            if (CurrentState == GameState.EndPlayerTurn)
            {
                UpdateGameState(GameState.StartPlayerTurn); // start new player turn based on the 
            }
        }
    }

    private void ProcessTurnTime()
    {
        if (TurnLength <= 0f) // end of turn
        {
            UpdateGameState(GameState.EndPlayerTurn);
        }
        else
        {
            float seconds = Mathf.FloorToInt(TurnLength % 60);
            //TODO attach to UI timer element timeText.text = string.Format("{0:00}", seconds);
        }
        TurnLength -= Time.deltaTime;
    }

    private void ProcessPlayerTurn()
    {
        if (CurrentPlayer.IsTurn == true)
        {
            UpdateGameState(GameState.EndPlayerTurn);
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

    public void UpdateGameState(GameState newState)
    {
        if (newState != CurrentState)
        {
            CurrentState = newState;
            switch (newState)
            {
                case GameState.StartPlayerTurn: // process start player turn
                    StartPlayerTurn();
                    break;

                case GameState.EndPlayerTurn: // process ending the players turn
                    EndPlayerTurn();
                    break;

                case GameState.GameOver:
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

    private void StartPlayerTurn()
    {
        CurrentPlayer = Players[CurrentPlayerIndex];
        CurrentPlayer.IsTurn = true;
        Debug.Log("Started turn for player: " + CurrentPlayerIndex);
    }

    private void EndPlayerTurn()
    {
        CurrentPlayer.IsTurn = false;
        CurrentPlayerIndex = (CurrentPlayerIndex + 1) % Players.Length;
        CurrentPlayer = Players[CurrentPlayerIndex];
        Debug.Log("Ended player turn, new player: " + CurrentPlayerIndex);
    }
}
