using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using static UnityEngine.ParticleSystem;

public class BreakBlockContorller : MonoBehaviour
{
    [Header("破壊時エフェクト")]
    [SerializeField] ParticleSystem breakEffect = default!;
    [Header("破壊時SE")]
    [SerializeField, Required] AudioSource _breakAudioSource = default!;

    public void BlockBreak(Collider other)
    {
        breakEffect.transform.position = other.gameObject.transform.position;
        Destroy(other.gameObject);

        breakEffect.Play();
        SoundManager.Instance.PlaySE(_breakAudioSource, SoundSource.SE066_BreakBlock);
    }
}
