using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class AudioManager : MonoBehaviour
{
    public PhotonView amPhotonView;

    public AudioClip[] playerLoopSounds;
    public AudioClip[] playerOneShotSounds;

    public AudioSource aSourcePlayerLoop;
    public AudioSource aSourcePlayerOneShot;

    private void Start()
    {
        amPhotonView = GetComponent<PhotonView>();
    }

    [PunRPC]
    void RPCAudioManagerPlayPlayerLoopSound(string soundName)
    {
        HandlePlayerLoopSound(soundName);
    }

    [PunRPC]
    void RPCAudioManagerTogglePausePlayerLoopSound()
    {
        if(aSourcePlayerLoop.isPlaying)
        {
            aSourcePlayerLoop.Pause();
        } else
        {
            aSourcePlayerLoop.UnPause();
        }
    }

    [PunRPC]
    void RPCAudioManagerStopPlayerLoopSound()
    {
        aSourcePlayerLoop.Stop();
    }

    void HandlePlayerLoopSound(string soundName)
    {
        switch (soundName)
        {
            case "movement":
                aSourcePlayerLoop.clip = playerLoopSounds[0];
                aSourcePlayerLoop.Play();
                break;

            case "ninja":
                aSourcePlayerLoop.clip = playerLoopSounds[1];
                aSourcePlayerLoop.Play();
                break;

            case "barrier":
                aSourcePlayerLoop.clip = playerLoopSounds[2];
                aSourcePlayerLoop.Play();
                break;

            default:
                Debug.LogWarning("Player Loop Audio Source Not Found");
                break;
        }
    }


    [PunRPC]
    void RPCAudioManagerPlayPlayerOneShotSound(string soundName)
    {
        HandlePlayerOneShotSound(soundName);
    }

    public void HandlePlayerOneShotSound(string soundName)
    {
        switch (soundName)
        {
            case "land":
                aSourcePlayerOneShot.clip = playerOneShotSounds[0];
                aSourcePlayerOneShot.Play();
                break;

            case "jump":
                aSourcePlayerOneShot.clip = playerOneShotSounds[1];
                aSourcePlayerOneShot.Play();
                break;

            case "barrier":
                aSourcePlayerOneShot.clip = playerOneShotSounds[2];
                aSourcePlayerOneShot.Play();
                break;

            case "build":
                aSourcePlayerOneShot.clip = playerOneShotSounds[3];
                aSourcePlayerOneShot.Play();
                break;

            case "taunt-flex":
                aSourcePlayerOneShot.clip = playerOneShotSounds[4];
                aSourcePlayerOneShot.Play();
                break;

            case "taunt-sad":
                Debug.Log("TAUNT SAD");
                aSourcePlayerOneShot.clip = playerOneShotSounds[5];
                aSourcePlayerOneShot.Play();
                break;

            case "punch":
                aSourcePlayerOneShot.clip = playerOneShotSounds[6];
                aSourcePlayerOneShot.Play();
                break;

            case "count-down":
                aSourcePlayerOneShot.clip = playerOneShotSounds[7];
                aSourcePlayerOneShot.Play();
                break;

            case "time-up":
                aSourcePlayerOneShot.clip = playerOneShotSounds[8];
                aSourcePlayerOneShot.Play();
                break;

            default:
                Debug.LogWarning("Player One Shot Audio Source Not Found");
                break;
        }
    }
}
