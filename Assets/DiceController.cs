using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public enum DieType
{

}

public class DiceController : MonoBehaviour
{
    private bool isClicked = false;
    public Camera renderTextureCamera;
    public float distanceToCamera = 3f;
    private Rigidbody rb;
    public float goToCameraSpeed = 0.5f;
    public float rollForce = 5f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Check for mouse click
        if (Input.GetMouseButtonDown(0) && !isClicked)
        {
            isClicked = true;
            JumpAndRoll();
        }
    }

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

                // Add a vertical force to die
                rb.AddForce(Vector3.up * rollForce, ForceMode.Impulse);

                // Add a random torque to die
                float torqueForce = Random.Range(10f, 20f);
                Vector3 torque = new Vector3(Random.Range(-torqueForce, torqueForce), Random.Range(-torqueForce, torqueForce), Random.Range(-torqueForce, torqueForce));
                rb.AddTorque(torque, ForceMode.Impulse);
                StartCoroutine(LandAnimation());
            }
        }
    }

    IEnumerator LandAnimation()
    {
        yield return new WaitForSeconds(3f); // Adjust the delay as needed

        // Record the landed on rotation
        Quaternion startRotation = RoundToNearest90(transform.rotation.eulerAngles);

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
        transform.rotation = RoundToNearest90(transform.rotation.eulerAngles);
        transform.position = targetPosition;

        // Disable the Rigidbody to freeze the die in place
        rb.isKinematic = true;
        

        // Snap the rotation to the recorded rounded rotation
        transform.rotation = startRotation;
        yield return new WaitForSeconds(5f);
        rb.isKinematic = false;
        isClicked = false;

    }

    Quaternion RoundToNearest90(Vector3 euler)
    {
        float xRemainder = Mathf.Abs(euler.x % 90);
        float yRemainder = Mathf.Abs(euler.y % 90);
        float zRemainder = Mathf.Abs(euler.z % 90);

        if (xRemainder < yRemainder && xRemainder < zRemainder)
        {
            euler.x = Mathf.Round(euler.x / 90) * 90;
            euler.y = euler.z = 0;
        }
        else if (yRemainder < xRemainder && yRemainder < zRemainder)
        {
            euler.y = Mathf.Round(euler.y / 90) * 90;
            euler.x = euler.z = 0;
        }
        else
        {
            euler.z = Mathf.Round(euler.z / 90) * 90;
            euler.x = euler.y = 0;
        }

        return Quaternion.Euler(euler);
    }

    /*Quaternion RoundToNearest90(Vector3 euler)
    {
        euler.x = Mathf.Round(euler.x / 90) * 90;
        euler.y = Mathf.Round(euler.y / 90) * 90;
        euler.z = Mathf.Round(euler.z / 90) * 90;

        // Ensure the result is within the range [-180, 180)
        euler.x = (euler.x + 360) % 360;
        euler.y = (euler.y + 360) % 360;
        euler.z = (euler.z + 360) % 360;

        if (euler.x >= 180)
            euler.x -= 360;
        if (euler.y >= 180)
            euler.y -= 360;
        if (euler.z >= 180)
            euler.z -= 360;

        return Quaternion.Euler(euler);
    }*/

    public int checkDiceRoll()
    {
        return 0;
    }
}
