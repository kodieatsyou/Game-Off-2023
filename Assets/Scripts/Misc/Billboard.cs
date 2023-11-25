using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Billboard : MonoBehaviour
{
    GameObject camera = null;

    private void Start()
    {
        camera = GameObject.FindGameObjectWithTag("Camera");
    }
    void Update()
    {

        if (camera != null)
        {
            // Calculate the direction from the sprite to the camera
            Vector3 toCamera = camera.transform.position - transform.position;

            // Ensure the sprite is always facing the camera
            transform.LookAt(transform.position + toCamera, Vector3.up);
        }
        else
        {
            Debug.LogError("Main camera not found!");
        }
    }
}