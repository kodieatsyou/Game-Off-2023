using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCursorArrow : MonoBehaviour
{
    public float rotationSpeed = 2.0f;
    public float bobSpeed = 1.0f;
    public float bobHeight = 0.5f;

    private float originalY;

    void Start()
    {
        originalY = transform.position.y;
    }

    void Update()
    {
        RotateObject();
        BobObject();
    }

    void RotateObject()
    {
        // Rotate the object continuously
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }

    void BobObject()
    {
        // Calculate bobbing movement using sine function
        float newY = originalY + Mathf.Sin(Time.time * bobSpeed) * bobHeight;

        // Update the object's position to create bobbing effect
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}
