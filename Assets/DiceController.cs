using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public enum DieType
{
    Number,
    Action
}

public class DiceController : MonoBehaviour
{
    public Camera renderTextureCamera;
    public float distanceToCamera = 3f;
    public float goToCameraSpeed = 0.5f;
    public float rollForce = 5f;
    public GameObject dicePrefab;
    public TMP_Text statusText;
    public Vector3 diePitPos;
    void Start()
    {
        StartCoroutine("RollDice", Vector3.zero);
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && EventSystem.current.IsPointerOverGameObject())
        {
            Debug.Log("Click");

            // Get the click position in the RenderTexture's coordinate system
            Vector3 clickPosInViewport = renderTextureCamera.ScreenToViewportPoint(Input.mousePosition);

            // Convert the position from viewport coordinates to the RenderTexture coordinates
            Vector3 clickPosInRenderTexture = new Vector3(
                clickPosInViewport.x * GetComponent<RectTransform>().sizeDelta.x,
                clickPosInViewport.y * GetComponent<RectTransform>().sizeDelta.y,
                0
            );

            Debug.Log("RenderTexture Pos: " + clickPosInRenderTexture);

            // Use ViewportPointToRay to create a ray from the click position
            Ray ray = renderTextureCamera.ViewportPointToRay(new Vector3(
                clickPosInViewport.x,
                clickPosInViewport.y,
                0f
            ));

            RaycastHit hit;

            int layerMask = 1 << LayerMask.NameToLayer("DiePit");

            // Check if the ray intersects with objects in the world
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
            {
                Vector3 fingerTipPosition = hit.point;
                Debug.Log("Finger Tip Pos: " + fingerTipPosition + " Hit: " + hit.collider.name);

                // Perform your logic with the finger tip position in the real world
                StartCoroutine(RollDice(fingerTipPosition));
            }
        }
    }

    // Function to roll the dice
    IEnumerator RollDice(Vector3 position)
    {
        Debug.Log("Rolling Dice at: " + position);
        // Display a message while waiting for the player to click
        statusText.text = "Rolling Dice...";

        // Once the player clicks, stop the continuous growth and shrinkage
        statusText.text = "";

        // Simulate dice rolling logic and return the result (always returning 0 for now)
        int diceResult = 0;

        // Instantiate the dice at the specified position
        GameObject dice = Instantiate(dicePrefab, position, Quaternion.identity);

        // Give the dice a force in a random direction
        Vector3 forceDirection = Random.onUnitSphere;
        dice.GetComponent<Rigidbody>().AddForce(forceDirection * rollForce, ForceMode.Impulse);

        // Give the dice a rotational force in a random direction
        Vector3 torqueDirection = Random.onUnitSphere;
        dice.GetComponent<Rigidbody>().AddTorque(torqueDirection * rollForce, ForceMode.Impulse);

        // Wait for a short time to allow the dice to settle before returning the result
        yield return new WaitForSeconds(5f);

        // Destroy the dice object as it is no longer needed
        Destroy(dice);

        // Return the simulated dice result
        yield return diceResult;
    }
    /*
    void JumpAndRoll()
    {
        Debug.Log("Rolling");
        if (renderTextureCamera != null)
        {
            // Get the mouse position in screen coordinates
            Vector3 mousePosition = Input.mousePosition;

            // Create a ray from the camera through the mouse position
            Ray ray = renderTextureCamera.ScreenPointToRay(mousePosition);
            RaycastHit hit;

            int layerMask = 1 << LayerMask.NameToLayer("DiePit");

            if (Physics.Raycast(ray, out hit, float.MaxValue, layerMask, QueryTriggerInteraction.Collide))
            {
                Rigidbody rb = GetComponent<Rigidbody>();

                // Add a vertical force to die based on rollForce
                rb.AddForce(Vector3.up * rollForce, ForceMode.Impulse);

                // Add a torque to die based on rollForce
                float torqueForce = rollForce * Random.Range(1f, 2f); // Adjust the multiplier as needed
                Vector3 torque = new Vector3(Random.Range(-torqueForce, torqueForce), Random.Range(-torqueForce, torqueForce), Random.Range(-torqueForce, torqueForce));
                rb.AddTorque(torque, ForceMode.Impulse);

                // Add a random force in a random direction based on rollForce
                float horizontalForce = rollForce * Random.Range(1f, 2f); // Adjust the multiplier as needed
                float verticalForce = rollForce * Random.Range(1f, 2f); // Adjust the multiplier as needed
                Vector3 randomForce = new Vector3(Random.Range(-horizontalForce, horizontalForce), verticalForce, Random.Range(-horizontalForce, horizontalForce));
                rb.AddForce(randomForce, ForceMode.Impulse);

                StartCoroutine(LandAnimation());
            }
        }
    }

    IEnumerator LandAnimation()
    {
        yield return new WaitForSeconds(5f); // Adjust the delay as needed
        Vector3 currentRotation = transform.rotation.eulerAngles;
        Debug.Log("Initial rotation: " + currentRotation);
        Vector3 closestRotation = GetClosestRotation(currentRotation);
        Debug.Log("Rounded rotation: " + closestRotation);

        // Use the rounded rotation as the starting rotation
        Quaternion startRotation = Quaternion.Euler(closestRotation);

        // Spin the die randomly while moving to the camera up to the specified distance
        float distanceThreshold = 0.01f; // Threshold for reaching the specified distance
        Quaternion randomStartRotation = transform.rotation;
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = renderTextureCamera.transform.position + renderTextureCamera.transform.forward * distanceToCamera;

        while (Vector3.Distance(transform.position, targetPosition) > distanceThreshold)
        {
            // Spin the die randomly
            transform.rotation = Quaternion.Euler(Random.Range(0, 4) * 90f, Random.Range(0, 4) * 90f, Random.Range(0, 4) * 90f) * randomStartRotation;

            // Move the die toward the camera
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * goToCameraSpeed);

            yield return null;
        }

        // Ensure the die reaches the final position
        transform.rotation = Quaternion.Euler(closestRotation);
        transform.position = targetPosition;

        // Disable the Rigidbody to freeze the die in place
        rb.isKinematic = true;

        // Snap the rotation to the recorded rounded rotation
        transform.rotation = startRotation;
        yield return new WaitForSeconds(5f);

        // Enable the Rigidbody to make the die movable again
        rb.isKinematic = false;
        isClicked = false;
    }

    public Vector3 NormalizeAndRoundEulerAngles(Vector3 eulerAngles)
    {
        // Normalize the x and y rotations
        eulerAngles.x = Mathf.Repeat(eulerAngles.x, 360.0f);
        eulerAngles.y = Mathf.Repeat(eulerAngles.y, 360.0f);

        // Round the rotations to the nearest multiple of 90 both positive and negative
        eulerAngles.x = Mathf.Round(eulerAngles.x / 90.0f) * 90.0f;
        eulerAngles.y = Mathf.Round(eulerAngles.y / 90.0f) * 90.0f;

        // If the axis rotation is rounded to 360 or -360, round it to 0
        if (Mathf.Approximately(eulerAngles.z, 360.0f) || Mathf.Approximately(eulerAngles.z, -360.0f))
        {
            eulerAngles.z = 0.0f;
        }
        // If the axis rotation is rounded to 270, round it to -90
        else if (Mathf.Approximately(eulerAngles.z, 270.0f))
        {
            eulerAngles.z = -90.0f;
        }
        // If the axis rotation is rounded to -270, round it to 90
        else if (Mathf.Approximately(eulerAngles.z, -270.0f))
        {
            eulerAngles.z = 90.0f;
        }
        // Round other cases to the nearest multiple of 90
        else
        {
            eulerAngles.z = Mathf.Round(eulerAngles.z / 90.0f) * 90.0f;
        }

        return eulerAngles;
    }

    public KeyValuePair<Vector3, int> FindClosestAngle(Dictionary<Vector3, int> angleDictionary, Vector3 roundedAngle)
    {
        float minDistance = float.MaxValue;
        KeyValuePair<Vector3, int> closestAngle = new KeyValuePair<Vector3, int>();

        foreach (var kvp in angleDictionary)
        {
            Vector3 angle = kvp.Key;

            // Calculate the squared distance between angles
            float distance = Vector3.SqrMagnitude(angle - roundedAngle);

            // Update closest angle if the current angle is closer
            if (distance < minDistance)
            {
                minDistance = distance;
                closestAngle = kvp;
            }
        }

        return closestAngle;
    }
    */

    /* Angles can be:
     * 0, 0, 0 - Wind
     * 90, 0, 0 - Power Card
     * -90, 0, 0 - Grapple
     * 0, 0, 90 - Grapple
     * 0, 0, -90 - Grapple
     * 180, 0, 0 - Wind
     * 
    public Vector3 GetClosestRotation(Vector3 rotationEulerAngles)
    {
        float roundedX = RoundToNearestMultiple(rotationEulerAngles.x, 90);
        float roundedZ = RoundToNearestMultiple(rotationEulerAngles.z, 90);

        // Check for special cases when the angle is closer to 0 than 90
        roundedX = Mathf.Abs(rotationEulerAngles.x % 360) < 45 ? 0 : roundedX;
        roundedZ = Mathf.Abs(rotationEulerAngles.z % 360) < 45 ? 0 : roundedZ;

        Vector3 roundedAngles = new Vector3(roundedX, rotationEulerAngles.y, roundedZ);

        Vector3[] predefinedRotations = {
        Vector3.zero,          // (0, 0, 0)
        new Vector3(90, 0, 0),  // (90, 0, 0)
        new Vector3(-90, 0, 0), // (-90, 0, 0)
        new Vector3(0, 0, 90),  // (0, 0, 90)
        new Vector3(0, 0, -90), // (0, 0, -90)
        new Vector3(180, 0, 0)  // (180, 0, 0)
    };

        Vector3 closestRotation = predefinedRotations
            .Select(predefinedRotation => NormalizeEulerAngles(predefinedRotation))
            .OrderBy(normalizedPredefinedRotation =>
                Mathf.Abs(roundedAngles.x - normalizedPredefinedRotation.x) +
                Mathf.Abs(roundedAngles.y - normalizedPredefinedRotation.y) +
                Mathf.Abs(roundedAngles.z - normalizedPredefinedRotation.z))
            .First();

        return closestRotation;
    }

    private float RoundToNearestMultiple(float value, float multiple)
    {
        return Mathf.Round(value / multiple) * multiple;
    }

    private Vector3 NormalizeEulerAngles(Vector3 eulerAngles)
    {
        float epsilon = 0.01f;
        return new Vector3(
            Mathf.Abs(eulerAngles.x % 360) < epsilon ? 0 : eulerAngles.x % 360,
            Mathf.Abs(eulerAngles.y % 360) < epsilon ? 0 : eulerAngles.y % 360,
            Mathf.Abs(eulerAngles.z % 360) < epsilon ? 0 : eulerAngles.z % 360
        );
    }
    */
}
