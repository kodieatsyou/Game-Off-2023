using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RootMotionSync : MonoBehaviour
{
    public Transform root;
    private Animator animator;
    private Vector3 initialBonePosition;
    private Vector3 bonePositionDelta;
    private string currentAnimationState = null;
    private string syncedAnimationState;

    void Start()
    {
        if (root == null)
        {
            Debug.LogError("Root not assigned!");
            return;
        }

        animator = GetComponent<Animator>();
        initialBonePosition = root.position;
    }

    void Update()
    {
        if(GetComponent<PhotonView>().IsMine)
        {
            // Get the name of the current animation state.
            currentAnimationState = animator.GetCurrentAnimatorStateInfo(0).fullPathHash.ToString();

            if (root == null || animator == null)
            {
                return;
            }

            if (currentAnimationState.Equals(syncedAnimationState))
            {
                bonePositionDelta = root.position - initialBonePosition;

                transform.position = Vector3.MoveTowards(transform.position, transform.position + bonePositionDelta, 100 * Time.deltaTime);
                initialBonePosition = root.position;
            }
            else
            {
                bonePositionDelta = Vector3.zero;
                initialBonePosition = root.position;
            }
        }
    }
    public void SetSyncedAnimationState()
    {
        syncedAnimationState = animator.GetCurrentAnimatorStateInfo(0).fullPathHash.ToString();
    }
}