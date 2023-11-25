using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class PlayerControllerNetwork : MonoBehaviourPunCallbacks, IPunInstantiateMagicCallback
{

    public Player owner;
    public float moveSpeed = 5.0f;
    public float rotationSpeed = 5.0f;
    public PhotonView PCPhotonView;
    public Animator animator;

    public string currentPositionDisplacementAnimationName = null;
    public Vector3 currentPositionDisplacementAnimationPosWhenDone = new Vector3(0, 0, 0);

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        PCPhotonView = GetComponent<PhotonView>();
        animator = GetComponent<Animator>();
        return;
    }

    void StartPositionDisplacementAnimation(string nameOfAnimation, Vector3 posWhenDone)
    {
        animator.SetBool(nameOfAnimation, true);
        currentPositionDisplacementAnimationName = nameOfAnimation;
        currentPositionDisplacementAnimationPosWhenDone = posWhenDone;
    }

    void SetPositionForPositionDisplacementAnimation()
    {
        transform.position = currentPositionDisplacementAnimationPosWhenDone;
    }

    public void FinishPositionDisplacementAnimation()
    {
        Debug.Log("Finsihed timed animation!");
        animator.SetBool(currentPositionDisplacementAnimationName, false);
        currentPositionDisplacementAnimationName = null;
    }

    [PunRPC]
    void RPCPlayerControllerNetworkCreateAccessories()
    {
        if (photonView.IsMine)
        {
            Transform headAccessory = gameObject.transform.Find("Armature/body/neck/head/head_end");
            Transform faceAccessory = gameObject.transform.Find("Armature/body/neck/head");

            int headIndex = (int)owner.CustomProperties["head"] - 1;
            int faceIndex = (int)owner.CustomProperties["face"] - 1;

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

    [PunRPC]
    void RPCPlayerControllerNetworkInitialize(int id)
    {
        if(photonView.IsMine)
        {
            owner = PhotonNetwork.CurrentRoom.GetPlayer(id);
            int textureIndex = (int)owner.CustomProperties["texture"];
            if (textureIndex >= 100)
            {
                GetComponentInChildren<SkinnedMeshRenderer>().material.SetTexture("_BaseMap", GameAssets.i.character_special_skins_[(textureIndex / 100) - 1]);
            }
            else
            {
                GetComponentInChildren<SkinnedMeshRenderer>().material.SetTexture("_BaseMap", GameAssets.i.character_skins_[(textureIndex)]);
            }
            PCPhotonView.RPC("RPCPlayerControllerNetworkCreateAccessories", RpcTarget.AllBuffered);
        }
        
    }

    [PunRPC]
    public void RPCPlayerControllerNetworkMoveThroughPath(Vector3[] waypoints)
    {
        if(PCPhotonView.IsMine)
        {
            Debug.Log(transform.name + " is moving!");
            StartCoroutine(MoveThroughWaypoints(waypoints));
        }
    }

    IEnumerator MoveThroughWaypoints(Vector3[] waypoints)
    {
        int currentWaypointIndex = 0;

        while (currentWaypointIndex < waypoints.Length)
        {
            Debug.Log("Going to space: " + waypoints[currentWaypointIndex]);

            animator.SetBool("Moving", true);

            // Calculate the distance to the next waypoint
            float distance = Vector3.Distance(transform.position, waypoints[currentWaypointIndex]);

            // Check if the next waypoint is one y level up
            if(waypoints[currentWaypointIndex].y > transform.position.y)
            {
                // Rotate horizontally towards the waypoint before climbing
                Vector3 horizontalDirection = (new Vector3(waypoints[currentWaypointIndex].x, transform.position.y, waypoints[currentWaypointIndex].z) - transform.position).normalized;
                Quaternion horizontalLookRotation = Quaternion.LookRotation(horizontalDirection);
                transform.rotation = horizontalLookRotation;

                // Play climbing animation
                animator.SetBool("Moving", false);
                StartPositionDisplacementAnimation("Climbing_Up", waypoints[currentWaypointIndex]);
                yield return new WaitUntil(() => currentPositionDisplacementAnimationName == null);

                currentWaypointIndex++;
                yield return null;
            } 
            else if(waypoints[currentWaypointIndex].y < transform.position.y)
            {
                // Rotate horizontally towards the waypoint before climbing
                Vector3 horizontalDirection = (new Vector3(waypoints[currentWaypointIndex].x, transform.position.y, waypoints[currentWaypointIndex].z) - transform.position).normalized;
                Quaternion horizontalLookRotation = Quaternion.LookRotation(horizontalDirection);
                transform.rotation = horizontalLookRotation;

                // Play climbing animation
                animator.SetBool("Moving", false);
                StartPositionDisplacementAnimation("Climbing_Down", waypoints[currentWaypointIndex]);
                yield return new WaitUntil(() => currentPositionDisplacementAnimationName == null);

                currentWaypointIndex++;
                yield return null;
            }
            else
            {
                // Move towards the waypoint
                transform.position = Vector3.MoveTowards(transform.position, waypoints[currentWaypointIndex], moveSpeed * Time.deltaTime);

                // Rotate towards the waypoint
                Vector3 direction = (waypoints[currentWaypointIndex] - transform.position).normalized;
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);

                // If the GameObject is close enough to the waypoint, move to the next waypoint
                if (distance < 0.1f)
                {
                    currentWaypointIndex++;
                }
                yield return null; // Yield control until the next frame
            }
        }

        animator.SetBool("Moving", false);
    }
}
