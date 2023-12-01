using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Billboard : MonoBehaviour
{
    GameObject playerCamera = null;

    private void Start()
    {
        playerCamera = GameObject.FindGameObjectWithTag("Camera");
    }
    void Update()
    {

        if (playerCamera != null)
        {
            Vector3 toCamera = playerCamera.transform.position - transform.position;
            transform.LookAt(transform.position + toCamera, Vector3.up);
        }
    }
}