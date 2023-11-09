using UnityEngine;

[CreateAssetMenu(fileName = "AnimationProp", menuName = "ScriptableObjects/Animation/AnimationProp", order = 1)]
public class AnimationProp : ScriptableObject
{
    public GameObject prop;
    public string parentBoneName;
    public GameObject particleEffect;
    public bool parentParticleToBone;
    public bool particleHasEnd;
}
