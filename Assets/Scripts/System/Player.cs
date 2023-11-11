using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private GameManager gameManager;
    public string Name;
    public string Id; // allows backend to track player if same name
    public int Score;
    public int PlayerIndex;
    public bool IsCurrentPlayer;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameManager.Instance;
        Id = System.Guid.NewGuid().ToString();
        Name = "Player " + PlayerIndex; // TODO Replace with name from menu
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    void addScore(int score)
    {
        Score += score;
    }
}
