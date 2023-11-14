using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    Menu,
    WaitingForPlayers,
    PlayerTurn,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameState CurrentState;
    public static event System.Action<GameState> OnStateChange;
    public Player[] Players;
    public int CurrentPlayerIndex;

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
        UpdateGameState(GameState.WaitingForPlayers);
        CurrentPlayerIndex = 0;
    }
    
    // Update is called once per frame
    private void Update()
    {
        
    }

    public void UpdateGameState(GameState newState)
    {
        CurrentState = newState;

        switch (newState)
        {
            case GameState.Menu:
                break;
            case GameState.WaitingForPlayers:
                break;
            case GameState.PlayerTurn:
                break;
            case GameState.GameOver:
                break;
            default:
                Debug.Log("Invalid Game State");
                throw new System.Exception("Invalid Game State");
        }

        OnStateChange?.Invoke(newState);
    }
}
