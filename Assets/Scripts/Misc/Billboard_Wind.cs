using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard_Wind : MonoBehaviour
{
    GameObject playerCamera = null;

    public float clampedYRot;

    private void Start()
    {
        playerCamera = GameObject.FindGameObjectWithTag("Camera");
    }
    void Update()
    {

        if (playerCamera != null)
        {
            Vector3 toCamera = playerCamera.transform.position - transform.position;

            // Get the rotation to face the camera
            Quaternion targetRotation = Quaternion.LookRotation(toCamera, Vector3.up);

            // Calculate the clamped rotation
            //float angleY = Mathf.Clamp(targetRotation.eulerAngles.y, -30f, 30f);
            Quaternion clampedRotation = Quaternion.Euler(targetRotation.eulerAngles.x, 0, targetRotation.eulerAngles.z);

            // Apply the clamped rotation
            transform.localRotation = clampedRotation;
        }
    }
}
