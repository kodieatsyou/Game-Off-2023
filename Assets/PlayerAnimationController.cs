using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    private Animator animator;
    private bool continueAnimations = false;
    private string curAnimationName = null;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }
    public void PlayRootMotionAnimation(string animationName)
    {
        GetComponent<RootMotionNetworkSync>().syncRootMotion = true;
        continueAnimations = false;
        animator.SetBool("Continue", false);
        animator.SetBool(animationName, true);
        curAnimationName = animationName;
    }

    public void OnRootMotionAnimationFinished()
    {
        Debug.Log("Transform: " + transform.position + " delta: " + animator.deltaPosition);
        transform.position = transform.position + animator.deltaPosition;
        transform.rotation = animator.deltaRotation;
        GetComponent<RootMotionNetworkSync>().syncRootMotion = false;
        animator.SetBool(curAnimationName, false);
        curAnimationName = null;
        animator.SetBool("Continue", true);
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
}
