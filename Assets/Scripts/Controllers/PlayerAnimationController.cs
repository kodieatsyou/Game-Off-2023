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

    GameObject currentParticle;

    GameObject currentProp;
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
        currentProp = Instantiate(GameAssets.i.prop_grapple_gun_);
        currentProp.transform.position = rightHand.position;
        currentProp.transform.rotation = Quaternion.Euler(currentProp.transform.rotation.eulerAngles + transform.rotation.eulerAngles);
        currentProp.transform.parent = rightHand;
    }

    public void SpawnHourGlass() {
        currentProp = Instantiate(GameAssets.i.prop_hourglass_);
        currentProp.transform.position = rightHand.position;
        currentProp.transform.rotation = Quaternion.Euler(currentProp.transform.rotation.eulerAngles + transform.rotation.eulerAngles);
        currentProp.transform.parent = rightHand;
    }

    public void LevitateBuildBlock() {
        BoardSpace currentSpace = GetComponent<PlayerController>().currentSpace;
        BoardManager.Instance.BMPhotonView.RPC("BoardManagerSetSpaceIsBuilt", RpcTarget.All, new Vector3(currentSpace.GetPosInBoard().x, currentSpace.GetPosInBoard().y + 1, currentSpace.GetPosInBoard().z), true);
    }

    public void DestroyParticle() {
        if(currentParticle != null) {
            Destroy(currentParticle);
        }
        currentParticle = null;
    }

    public void DestroyProp() {
        if(currentProp != null) {
            Destroy(currentProp);
        }
        currentProp = null;
    }

    public void SpawnParticle(GameObject particle) {
        DestroyParticle();
        currentParticle = Instantiate(particle);
        currentParticle.transform.position = transform.position;
        currentParticle.transform.rotation = transform.rotation;
    }

    [PunRPC]
    void RPCPlayerAnimationControllerPlayTriggeredAnimation(string animationName)
    {
        PlayTriggeredAnimation(animationName);
    }

}
