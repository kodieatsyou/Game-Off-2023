using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIDiceController : MonoBehaviour
{
    // Speed of the dice spin
    public float spinSpeed = 5f;
    public GameObject die;

    // Function to roll the dice
    public void Roll()
    {
        // Stop any previous spinning coroutine
        StopAllCoroutines();

        // Get a random side
        int randomSide = Random.Range(1, 7); // Assuming a six-sided dice

        // Calculate the target rotation based on the random side
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, 90f * (randomSide - 1));

        // Start the rolling coroutine
        StartCoroutine(SpinAndLerp(targetRotation));
    }

    // Coroutine to spin and lerp the dice to the target rotation
    private IEnumerator SpinAndLerp(Quaternion targetRotation)
    {
        // Spin the dice for a short duration before lerping to the final rotation
        float spinDuration = 1f;
        float spinTimer = 0f;

        while (spinTimer < spinDuration)
        {
            die.transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime);
            die.transform.Rotate(Vector3.right, spinSpeed * Time.deltaTime);
            die.transform.Rotate(Vector3.back, spinSpeed * Time.deltaTime);
            spinTimer += Time.deltaTime;
            yield return null;
        }

        // Set the final rotation instantly to avoid snapping
        die.transform.rotation = targetRotation;
    }
}
