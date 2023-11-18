using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DieNamePlate : MonoBehaviour
{
    Quaternion fixedRotation;
    void Awake()
    {
        fixedRotation = transform.rotation;
    }


    private void LateUpdate()
    {
        transform.rotation = fixedRotation;
    }
    
}
