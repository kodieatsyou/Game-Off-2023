using ExitGames.Client.Photon.StructWrapping;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DieControllerTester : MonoBehaviour
{
    // Start is called before the first frame update

    public DiceController controller;
    void Start()
    {
    }

    public void TestRollDie()
    {
        controller.StartRoller();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && controller.readyToRoll)
        {
            controller.RollDie(DieType.Action, HandleRollResult);
        }
    }

    void HandleRollResult(int result)
    {
        Debug.Log("Roll result from tester: " + result);
        controller.StopRoller();
    }
}
