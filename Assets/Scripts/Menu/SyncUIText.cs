using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SyncUIText : MonoBehaviour
{
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // We own this player: send the others our data
            stream.SendNext(PhotonNetwork.LocalPlayer.NickName);
        }
        else
        {
            // Network player, receive data
            GetComponent<TextMeshProUGUI>().text = (string)stream.ReceiveNext();
        }
    }
}
