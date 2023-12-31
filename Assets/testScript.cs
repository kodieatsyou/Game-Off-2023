using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;
public class testScript : MonoBehaviourPunCallbacks
{

    void Start()
    {
        PhotonView pView = GetComponent<PhotonView>();
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if (PhotonNetwork.PlayerList[i] == pView.Owner)
            {
                Transform headAccessory = transform.Find("Armature/body/neck/head/head_end");
                Transform faceAccessory = transform.Find("Armature/body/neck/head");

                int textureIndex = (int)PhotonNetwork.PlayerList[i].CustomProperties["texture"];
                int headIndex = (int)PhotonNetwork.PlayerList[i].CustomProperties["face"]-1;
                int faceIndex = (int)PhotonNetwork.PlayerList[i].CustomProperties["head"]-1;

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
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (!photonView.IsMine && targetPlayer == photonView.Owner && changedProps.ContainsKey("texture"))
        {
            int textureID = (int)changedProps["texture"];

            GetComponentInChildren<SkinnedMeshRenderer>().material.SetTexture("_BaseMap", GameAssets.i.character_skins_[textureID]);
        }
    }
}
