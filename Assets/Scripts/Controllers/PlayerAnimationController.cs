using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    public bool test = false;
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
        Debug.Log("Playing triggered animation for player: " + " : " + animationName + PCPhotonView.Owner.NickName);
        continueAnimations = false;
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
    void RPCPlayerAnimationControllerPlayTriggeredAnimation(Player player, string animationName, PhotonMessageInfo info)
    {
        if(PCPhotonView.Owner == player) {

            switch(animationName) {
                case "Cry":
                    PlayerController.Instance.audioManager.amPhotonView.RPC("RPCAudioManagerPlayPlayerOneShotSound", RpcTarget.All, "taunt-sad");
                    PlayerController.Instance.GetTaunted(info.Sender.NickName);
                    break;
            }

            continueAnimations = false;
            animator.SetBool(animationName, true);
            currentTriggeredAnimationName = animationName;
        }
    }

}
