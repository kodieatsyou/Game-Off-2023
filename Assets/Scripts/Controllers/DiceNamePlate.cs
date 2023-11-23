using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class DiceNamePlate : MonoBehaviourPun, IPunInstantiateMagicCallback
{
    [SerializeField] GameObject gui;

    Quaternion fixedRotation;
    void Awake()
    {
        PhotonNetwork.AddCallbackTarget(this);
        fixedRotation = gui.transform.rotation;
        if (photonView.IsMine)
        {
            // Set the text for the local player
            GetComponentInChildren<TMP_Text>().text = PhotonNetwork.NickName;
        }
    }

    void OnDestroy()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    private void LateUpdate()
    {
        gui.transform.rotation = fixedRotation;
    }

    public void OnPhotonInstantiate(Photon.Pun.PhotonMessageInfo info)
    {
        Debug.Log("PHOTON INSTANTIATE DICE");
        if (!photonView.IsMine)
        {
            // Set the text for remote players
            gui.GetComponentInChildren<TMP_Text>().text = photonView.Owner.NickName;
        }
    }

}
