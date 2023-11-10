using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEvents : MonoBehaviour
{
    private GameObject propObj;
    private AnimationProp propAnim;
    private GameObject boneObj;
    private GameObject particleObj;
    private GameObject getBone(string boneName)
    {
        Transform[] ts = gameObject.transform.GetComponentsInChildren<Transform>();
        foreach (Transform t in ts) if (t.gameObject.name == boneName) return t.gameObject;
        return null;
    }
    void Spawn(AnimationProp prop)
    {
        propAnim = prop;
        boneObj = getBone(prop.parentBoneName);
        if (prop.prop != null)
        {
            propObj = Instantiate(prop.prop, boneObj.transform.position, Quaternion.identity);
            propObj.transform.position = boneObj.transform.position;
            propObj.transform.SetParent(boneObj.transform, true);

        } else
        {
            propObj = null;
        }
    }

    void PlayParticleEffect()
    {
        if(propAnim.particleEffect != null)
        {
            particleObj = Instantiate(propAnim.particleEffect, boneObj.transform.position, Quaternion.identity);
            if(propAnim.parentParticleToBone)
            {
                particleObj.transform.SetParent(boneObj.transform, true);
            }
        }
    }

    void Despawn()
    {
        if(propObj != null)
        {
            Destroy(propObj);
        }
        if(!propAnim.particleHasEnd)
        {
            Destroy(particleObj);
        }
    }

    void ChangeMaterialTexture(Texture newTexture)
    {
        this.gameObject.GetComponentInChildren<SkinnedMeshRenderer>().material.SetTexture("newText", newTexture);
    }
}
