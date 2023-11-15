using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public string Name;
    public int Score;
    public int CurrentLevel;
    public bool IsActiveTurn { private set; get; }
    public float TurnLength = 15f;

    private Coroutine TimerCoroutine;
    private GameManager Game;

    private void Awake()
    {
        Game = GameManager.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (IsActiveTurn)
        {
            // TODO Implement turn logic
        }
        else
        {
            // TODO Implement what happens when not player turn
        }
    }

    public void StartTurn()
    {
        TimerCoroutine = StartCoroutine(TurnTimeEnd());
        IsActiveTurn = true;
    }

    public void EndTurn()
    {
        StopCoroutine(TimerCoroutine);
    }

    private IEnumerator TurnTimeEnd()
    {
        if (TurnLength <= 0f) // end of turn
        {
            IsActiveTurn = false;
        }
        else
        {
            TurnLength -= Time.deltaTime;
            float seconds = Mathf.FloorToInt(TurnLength % 60);
            //TODO attach to UI timer element timeText.text = string.Format("{0:00}", seconds);
        }
        yield return null;
    }
}
