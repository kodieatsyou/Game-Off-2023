using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnAnimationProp : MonoBehaviour
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
        if (prop.prop == null)
        {
            propObj = new GameObject();
            propObj.transform.position = boneObj.transform.position;
            propObj.transform.SetParent(boneObj.transform, true);

        } else
        {
            propObj = Instantiate(prop.prop, boneObj.transform.position, Quaternion.identity);
        }
        propObj.transform.SetParent(boneObj.transform, true);
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
        Destroy(propObj);
        if(!propAnim.particleHasEnd)
        {
            Destroy(particleObj);
        }
    }
}
