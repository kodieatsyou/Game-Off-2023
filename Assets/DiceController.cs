using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;

public enum DieType
{
    Number,
    Action
}

public delegate void RollDieCallback(int result);

public class DiceController : MonoBehaviour
{
    public float rollForce = 5f;
    public GameObject diceNumPrefab;
    public GameObject diceActionPrefab;
    public Vector3 dieSpawnBounds = new Vector3(16, 16, 16);
    public Transform dieSpawnPosition;
    public TMP_Text rollDieText;
    public DieType testType;

    public Queue<GameObject> dieQueue;

    public Boolean readyToRoll = false;

    private RollDieCallback rollDieCallback;
    void Start()
    {
        StopRoller();
    }
    void Update()
    {

    }

    public void StopRoller()
    {
        this.GetComponent<RawImage>().enabled = false;
        rollDieText.gameObject.SetActive(false);
    }

    public void StartRoller()
    {
        this.GetComponent<RawImage>().enabled = true;
        rollDieText.gameObject.SetActive(true);
        readyToRoll = true;
        StartCoroutine(ScaleText());
    }

    public void RollDie(DieType type, RollDieCallback callback)
    {
        if(readyToRoll)
        {
            Debug.Log("Rolling!");
            rollDieCallback = callback;
            readyToRoll = false;
            StopCoroutine(ScaleText());
            rollDieText.gameObject.SetActive(false);
            StartCoroutine(StartRolling(type));
        }
    }

    IEnumerator ScaleText()
    {
        while(readyToRoll)
        {
            // Grow the text
            yield return ScaleTextAnimation(rollDieText.transform.localScale, Vector3.one * 1.2f, 0.5f);

            // Shrink the text
            yield return ScaleTextAnimation(rollDieText.transform.localScale, Vector3.one, 0.5f);
        }
    }

    IEnumerator ScaleTextAnimation(Vector3 startScale, Vector3 endScale, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            rollDieText.transform.localScale = Vector3.Lerp(startScale, endScale, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        rollDieText.transform.localScale = endScale;
    }

    IEnumerator StartRolling(DieType type)
    {
        Vector3 spawnPos = new Vector3(
            dieSpawnPosition.position.x,
            dieSpawnPosition.position.y + 6.0f,
            dieSpawnPosition.position.z
        );

        GameObject dice = null;

        if (type == DieType.Action)
        {
            dice = PhotonNetwork.Instantiate("Essential/Dice/ActionDie", spawnPos, Quaternion.Euler(new Vector3(180, 0, 0)));
            //dice = Instantiate(diceActionPrefab, spawnPos, Quaternion.Euler(new Vector3(180, 0, 0)));
        } else if(type == DieType.Number)
        {
            dice = PhotonNetwork.Instantiate("Essential/Dice/NumberDie", spawnPos, Quaternion.Euler(new Vector3(180, 0, 0)));
            //dice = Instantiate(diceNumPrefab, spawnPos, Quaternion.Euler(new Vector3(180, 0, 0)));
        } else
        {
            Debug.Log("ERROR ROLLING DIE");
            yield return -1;
        }
        

        Rigidbody rb = dice.GetComponent<Rigidbody>();

        float horizontalForce = rollForce * UnityEngine.Random.Range(1f, 2f);
        Vector3 randomForce = new Vector3(UnityEngine.Random.Range(-horizontalForce, horizontalForce), 0f, UnityEngine.Random.Range(-horizontalForce, horizontalForce));
        rb.AddForce(randomForce, ForceMode.Impulse);

        // Calculate torque to rotate around the x and z axes
        float torqueForce = rollForce * UnityEngine.Random.Range(1f, 2f);
        float torqueX = UnityEngine.Random.Range(-torqueForce, torqueForce);
        float torqueZ = UnityEngine.Random.Range(-torqueForce, torqueForce);
        Vector3 torque = new Vector3(torqueX, 0f, torqueZ);
        rb.AddTorque(torque, ForceMode.Impulse);


        yield return new WaitForSeconds(5f);

        Vector3 currentRotation = dice.transform.rotation.eulerAngles;
        int result = GetResult(currentRotation, type);
        PhotonNetwork.Destroy(dice);

        if (rollDieCallback != null)
        {
            rollDieCallback(result);
        }
    }

    public int GetResult(Vector3 rotationEulerAngles, DieType type)
    {
        Vector3[] predefinedRotations = {
            Vector3.zero,          // (0, 0, 0)
            new Vector3(90, 0, 0),  // (90, 0, 0)
            new Vector3(-90, 0, 0), // (-90, 0, 0)
            new Vector3(0, 0, 90),  // (0, 0, 90)
            new Vector3(0, 0, -90), // (0, 0, -90)
            new Vector3(180, 0, 0),  // (180, 0, 0)
            new Vector3(0, 0, 180)
        };

        Vector3 closestRotation = predefinedRotations
            .OrderBy(rotation => Vector3.Distance(rotation, NormalizeAndRoundEulerAngles(rotationEulerAngles)))
            .First();

        Debug.Log("Original rotation: " + rotationEulerAngles + " Rounded rotation: " + closestRotation);

        return GetRollValueBasedOnDieType(closestRotation, type);
    }

    // Function to normalize and round euler angles
    Vector3 NormalizeAndRoundEulerAngles(Vector3 inputEulerAngles)
    {
        // Normalize x and z coordinates to be within -360-0-360
        float normalizedX = Mathf.Repeat(inputEulerAngles.x, 360f);
        float normalizedZ = Mathf.Repeat(inputEulerAngles.z, 360f);

        // Round x and z rotations to the nearest multiple of 90
        float roundedX = Mathf.Round(normalizedX / 90f) * 90f;
        float roundedZ = Mathf.Round(normalizedZ / 90f) * 90f;

        // Handle special cases
        if (roundedX == 270f)
            roundedX = -90f;
        else if (roundedX == -270f)
            roundedX = 90f;
        else if (Mathf.Approximately(roundedX, 360f) || Mathf.Approximately(roundedX, -360f))
            roundedX = 0f;

        if (roundedZ == 270f)
            roundedZ = -90f;
        else if (roundedZ == -270f)
            roundedZ = 90f;
        else if (Mathf.Approximately(roundedZ, 360f) || Mathf.Approximately(roundedZ, -360f))
            roundedZ = 0f;

        return new Vector3(roundedX, inputEulerAngles.y, roundedZ);
    }

    private int GetRollValueBasedOnDieType(Vector3 direction, DieType type)
    {
        if(type == DieType.Action)
        {
            switch (direction)
            {
                case Vector3 d when d.Equals(Vector3.zero):
                    return 1; //Wind
                case Vector3 d when d.Equals(new Vector3(90, 0, 0)):
                    return 3; //Power Card
                case Vector3 d when d.Equals(new Vector3(-90, 0, 0)):
                    return 2; //Grapple
                case Vector3 d when d.Equals(new Vector3(0, 0, 90)):
                    return 2; //Grapple
                case Vector3 d when d.Equals(new Vector3(0, 0, -90)):
                    return 2; //Grapple
                case Vector3 d when d.Equals(new Vector3(180, 0, 0)):
                    return 1; //Wind
                case Vector3 d when d.Equals(new Vector3(0, 0, 180)):
                    return 1; //Wind
                default: return -1; //Shouldnt get here but check result for -1 to see if it failed
            }   
        } else if(type == DieType.Number)
        {
            switch (direction)
            {
                case Vector3 d when d.Equals(Vector3.zero):
                    return 5; //5
                case Vector3 d when d.Equals(new Vector3(90, 0, 0)):
                    return 3; //3
                case Vector3 d when d.Equals(new Vector3(-90, 0, 0)):
                    return 4; //4
                case Vector3 d when d.Equals(new Vector3(0, 0, 90)):
                    return 6; //6
                case Vector3 d when d.Equals(new Vector3(0, 0, -90)):
                    return 1; //1
                case Vector3 d when d.Equals(new Vector3(180, 0, 0)):
                    return 2; //2
                case Vector3 d when d.Equals(new Vector3(0, 0, 180)):
                    return 2; //2
                default: return -1; //Shouldnt get here but check result for -1 to see if it failed
            }
        } else
        {
            return -1;
        }
    }
}
