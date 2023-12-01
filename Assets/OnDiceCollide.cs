using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnDiceCollide : MonoBehaviour
{
    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "DieWall")
        {
            audioSource.Play();
        }
    }
}
