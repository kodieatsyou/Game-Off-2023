using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeNetworkObjects : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Instantiate("NetworkObjects/Board Manager", Vector3.zero, Quaternion.identity);
        }
    }
}
