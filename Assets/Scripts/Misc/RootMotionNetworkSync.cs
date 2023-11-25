using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PhotonView))]
public class RootMotionNetworkSync : MonoBehaviourPun, IPunObservable
{
    private Animator animator;
    private Vector3 lastPosition;
    private Quaternion lastRotation;

    private void Start()
    {
        animator = GetComponent<Animator>();
        lastPosition = transform.position;
        lastRotation = transform.rotation;
    }

    void Update()
    {
        // Check if the local player owns the PhotonView
        if (photonView.IsMine)
        {
            // Apply root motion locally
            ApplyRootMotion();
        }
    }

    void ApplyRootMotion()
    {
        Vector3 deltaPosition = transform.position - lastPosition;
        Quaternion deltaRotation = transform.rotation * Quaternion.Inverse(lastRotation);

        photonView.RPC("SyncRootMotion", RpcTarget.Others, deltaPosition, deltaRotation);

        lastPosition = transform.position;
        lastRotation = transform.rotation;
    }

    [PunRPC]
    void SyncRootMotion(Vector3 deltaPosition, Quaternion deltaRotation)
    {
        // Apply root motion on remote clients
        animator.applyRootMotion = true;

        // Update position and rotation
        transform.position += deltaPosition;
        transform.rotation *= deltaRotation;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Send data to other clients
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            // Receive data from the network and update remote position and rotation
            transform.position = (Vector3)stream.ReceiveNext();
            transform.rotation = (Quaternion)stream.ReceiveNext();
        }
    }
}