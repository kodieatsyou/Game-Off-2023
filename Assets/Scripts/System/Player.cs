using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public string Name;
    public int Score;
    public bool IsTurn { get; set; }
    public int CurrentLevel;
    public bool TurnState = false;

    private GameManager Game;

    // Start is called before the first frame update
    void Start()
    {
        Game = GameManager.Instance;
    }
    // Update is called once per frame
    void Update()
    {
        if (IsTurn)
        {
            // TODO Implement turn logic
        }
    }

    private void DetermineTurnCondition()
    {

    }
}
