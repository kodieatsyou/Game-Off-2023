using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerNetworkController : MonoBehaviour, IPunInstantiateMagicCallback
{
    public PhotonView PCPhotonView;

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        PCPhotonView = GetComponent<PhotonView>();
        PCPhotonView.RPC("RPCPlayerNetworkControllerInitialize", RpcTarget.AllBuffered);
        if(PCPhotonView.IsMine)
        {
            gameObject.GetComponentInChildren<SpriteRenderer>().enabled = true;
            gameObject.AddComponent<DiceController>();
            gameObject.AddComponent<PlayerController>();

            //DELETE THIS
            gameObject.AddComponent<PlayerClickOnHandler>();
            BoxCollider collider = gameObject.AddComponent<BoxCollider>();
            collider.center = new Vector3(0, 1, 0);
            collider.size = new Vector3(1.5f, 2.5f, 1);
            collider.isTrigger = true;

        } else {
            BoxCollider collider = gameObject.AddComponent<BoxCollider>();
            collider.center = new Vector3(0, 1, 0);
            collider.size = new Vector3(1.5f, 2.5f, 1);
            collider.isTrigger = true;
            gameObject.AddComponent<PlayerClickOnHandler>();
            gameObject.tag = "OtherPlayer";
        }
        return;
    }

    [PunRPC]
    void RPCPlayerNetworkControllerInitialize()
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

    //[PunRPC]
    /*public void RPCPlayerManagerPlayerMovedToSpace(Vector3 newSpace)
    {
        if (PCPhotonView.IsMine)
        {
            gameObject.GetComponent<PlayerController>().currentSpace = Board.Instance.boardArray[(int)newSpace.x, (int)newSpace.y, (int)newSpace.z];
        }
        Board.Instance.boardArray[(int)newSpace.x, (int)newSpace.y, (int)newSpace.z].PlacePlayerOnSpace(gameObject);
    }*/
}
