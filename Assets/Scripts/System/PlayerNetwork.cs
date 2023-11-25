using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class PlayerNetwork : MonoBehaviourPunCallbacks, IPunInstantiateMagicCallback
{
    public PhotonView PCPhotonView;
    public Vector3 spawnPosition;

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        PCPhotonView = GetComponent<PhotonView>();
        if(PCPhotonView.IsMine)
        {
            gameObject.AddComponent<PlayerController>();
        }
        PCPhotonView.RPC("RPCPlayerNetworkInitialize", RpcTarget.AllBuffered);
        return;
    }

    [PunRPC]
    void RPCPlayerNetworkInitialize()
    {
        int textureIndex = (int)PCPhotonView.Owner.CustomProperties["texture"];
        if (textureIndex >= 100)
        {
            GetComponentInChildren<SkinnedMeshRenderer>().material.SetTexture("_BaseMap", GameAssets.i.character_special_skins_[(textureIndex / 100) - 1]);
        }
        else
        {
            GetComponentInChildren<SkinnedMeshRenderer>().material.SetTexture("_BaseMap", GameAssets.i.character_skins_[(textureIndex)]);
        }
        Transform headAccessory = gameObject.transform.Find("Armature/body/neck/head/head_end");
        Transform faceAccessory = gameObject.transform.Find("Armature/body/neck/head");

        int headIndex = (int)PCPhotonView.Owner.CustomProperties["head"] - 1;
        int faceIndex = (int)PCPhotonView.Owner.CustomProperties["face"] - 1;

        if (headIndex != -1)
        {
            GameObject head = Instantiate(GameAssets.i.character_head_accessories_[headIndex], headAccessory.transform);
            head.transform.SetParent(headAccessory, false);
        }
        if (faceIndex != -1)
        {
            GameObject face = Instantiate(GameAssets.i.character_face_accessories_[faceIndex], faceAccessory.transform);
            face.transform.SetParent(faceAccessory, false);
        }

    }
}
