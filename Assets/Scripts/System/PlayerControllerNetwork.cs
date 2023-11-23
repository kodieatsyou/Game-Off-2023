using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class PlayerControllerNetwork : MonoBehaviourPunCallbacks, IPunInstantiateMagicCallback
{

    private Player player;
    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        return;
    }

    public void SetupPlayer(int playerID)
    {



        player = PhotonNetwork.CurrentRoom.GetPlayer(playerID);

        Debug.Log("Setting up player object " + player.ActorNumber);

        Transform headAccessory = transform.Find("Armature/body/neck/head/head_end");
        Transform faceAccessory = transform.Find("Armature/body/neck/head");

        int textureIndex = (int)player.CustomProperties["texture"];
        int headIndex = (int)player.CustomProperties["head"] - 1;
        int faceIndex = (int)player.CustomProperties["face"] - 1;

        Debug.Log("Texture index: " + textureIndex + " Head Index " + headIndex + " Face Index: " + faceIndex);

        GetComponentInChildren<SkinnedMeshRenderer>().material.SetTexture("_BaseMap", GameAssets.i.character_skins_[textureIndex]);

        if (headIndex != -1)
        {
            GameObject head = PhotonNetwork.Instantiate(string.Format("Props/Accessories/Head/{0}", GameAssets.i.character_head_accessories_[headIndex].name), Vector3.zero, Quaternion.Euler(-90f, 90f, 0f));
            head.transform.SetParent(headAccessory, false);
        }
        if (faceIndex != -1)
        {
            GameObject face = PhotonNetwork.Instantiate(string.Format("Props/Accessories/Face/{0}", GameAssets.i.character_face_accessories_[faceIndex].name), Vector3.zero, Quaternion.Euler(-90f, 90f, 0f));
            face.transform.SetParent(faceAccessory, false);
        }


    }
}
