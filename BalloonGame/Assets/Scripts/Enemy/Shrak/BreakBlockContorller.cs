using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakBlockContorller : MonoBehaviour
{
    [Header("破壊時エフェクト")]
    [SerializeField] ParticleSystem breakEffect = default!;
    [Header("破壊時SE")]
    [SerializeField, Required] AudioSource _breakAudioSource = default!;

    public void BlockBreak(GameObject breakableBlock)
    {
        breakEffect.transform.position = breakableBlock.transform.position;
        Destroy(breakableBlock);

        breakEffect.Play();
        SoundManager.Instance.PlaySE(_breakAudioSource, SoundSource.SE066_BreakBlock);
    }
}
