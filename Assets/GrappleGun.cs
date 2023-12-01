using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrappleGun : MonoBehaviour
{
    public GameObject gunAttachPoint;
    public GameObject hookAttachPoint;
    public GameObject firedGun;
    public GameObject loadedGun;

    public GameObject hook;


    LineRenderer lr;
    public void SpawnString() {
        lr.enabled = true;
        firedGun.transform.parent = transform.parent;
        Quaternion rotationBeforeNullParent = transform.rotation;
        transform.parent = null;
        transform.rotation = rotationBeforeNullParent;
    }

    public void DestroyGun() {
        Destroy(gameObject);
        Destroy(firedGun);
    }
    void Start()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(lr.enabled && gunAttachPoint != null && hookAttachPoint != null) {
            lr.SetPosition(0, gunAttachPoint.transform.position);
            lr.SetPosition(1, hookAttachPoint.transform.position);
        }
        
    }
}
