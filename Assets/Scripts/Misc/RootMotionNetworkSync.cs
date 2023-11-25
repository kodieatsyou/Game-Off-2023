using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PhotonView))]
public class RootMotionNetworkSync : MonoBehaviourPun
{
    private Vector3 rootPosition;
    private Quaternion rootRotation;

    public bool syncRootMotion = false;

    void Start()
    {

    }

    /*void Update()
    {
        if (photonView.IsMine)
        {
            // Send the root position to other players if syncRootMotion is true
            if (syncRootMotion)
            {
                GetComponent<Animator>().applyRootMotion = true;
                rootPosition = GetComponent<Animator>().deltaPosition;
                rootRotation = GetComponent<Animator>().deltaRotation;
            }
        }
        else
        {
            GetComponent<Animator>().applyRootMotion = false;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Send the local position and rotation to the network
            stream.SendNext(GetComponent<Animator>().deltaPosition);
            stream.SendNext(GetComponent<Animator>().deltaRotation);
        }
        else
        {
            // Receive the network position and rotation
            transform.position = (Vector3)stream.ReceiveNext();
            transform.rotation = (Quaternion)stream.ReceiveNext();
        }
    }*/
}