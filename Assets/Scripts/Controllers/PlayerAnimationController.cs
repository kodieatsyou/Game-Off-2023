using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{

    public Transform rightHand;
    private Animator animator;
    private bool continueAnimations = false;
    PhotonView PCPhotonView;
    string currentTriggeredAnimationName = null;

    private void Start()
    {
        animator = GetComponent<Animator>();
        PCPhotonView = GetComponent<PhotonView>();
    }
    public void PlayTriggeredAnimation(string animationName)
    {
        continueAnimations = false;
        //PCPhotonView.RPC("RPCPlayerAnimationControllerPlayTriggeredAnimation", RpcTarget.Others, animationName);
        animator.SetBool(animationName, true);
        currentTriggeredAnimationName = animationName;
    }

    public void OnTriggeredAnimationFinished()
    {
        animator.SetBool(currentTriggeredAnimationName, false);
        currentTriggeredAnimationName = null;
        continueAnimations = true;
    }

    public void SetAnimatorBool(string name, bool value)
    {
        animator.SetBool(name, value);
    }

    public bool CheckIfContinue()
    {
        return continueAnimations;
    }

    public void SpawnGrappleGun() {
        GameObject gun = Instantiate(GameAssets.i.prop_grapple_gun_);
        gun.transform.position = rightHand.position;
        gun.transform.rotation = Quaternion.Euler(gun.transform.rotation.eulerAngles + transform.rotation.eulerAngles);
        gun.transform.parent = rightHand;
    }

    [PunRPC]
    void RPCPlayerAnimationControllerPlayTriggeredAnimation(string animationName)
    {
        animator.SetTrigger(animationName);
    }

}
