using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerController: MonoBehaviour
{
    public Guid PlayerID { get; private set; }
    public string PlayerName { get; set; }
    public bool IsActiveTurn { get; set; }
    public int Score { set; get; }
    public int CurrentLevel { set; get; }
    public float TurnLength = 15f;
    public GameObject GameObjectPrefab;
    public Vector3 BoardPosition;
    public int ActionsRemaining { private set; get; }

    private Coroutine TimerCoroutine;
    private BoardManager Board;

    void Spawn(GameObject prefab, String name, Point Vector3)
    {
        PlayerID = Guid.NewGuid();
        Board = BoardManager.Instance;
        PlayerName = name;
        GameObjectPrefab = prefab;
        IsActiveTurn = false;
        ActionsRemaining = 3;
        BoardPosition = point;
        Debug.Log("Awake Player with name: " + PlayerName);
    }

    public void StartTurn()
    {
        IsActiveTurn = true;
        TimerCoroutine = StartCoroutine(TurnTimeEnd());
    }

    public void EndTurn()
    {
        StopCoroutine(TimerCoroutine);

        TurnLength = 15f;
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
