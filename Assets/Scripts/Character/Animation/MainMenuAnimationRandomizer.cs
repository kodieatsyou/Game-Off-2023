using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuAnimationRandomizer : MonoBehaviour
{
    Animator anim;
    public int randomAnimPercentChance = 70;
    bool inRandomAnimation = false;

    private void OnEnable()
    {
        anim = GetComponent<Animator>();
    }

    private void Start()
    {
        anim.SetInteger("Anim_Selector", 0);
    }

    public void CheckForRandomAnim()
    {
        //Check if we want to pick a random animation
        System.Random rand = new System.Random();
        if (rand.Next(100) <= randomAnimPercentChance && !inRandomAnimation)
        {
            //Pick a random anim to go to
            anim.SetInteger("Anim_Selector", rand.Next(1, 4));
        }
    }

    public void Reset()
    {
        anim.SetInteger("Anim_Selector", 0);
        inRandomAnimation = false;
    }


}
