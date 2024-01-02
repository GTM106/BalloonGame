using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using static UnityEngine.ParticleSystem;

public class BreakBlockContorller : MonoBehaviour
{
    [Header("�j�󎞃G�t�F�N�g")]
    [SerializeField] ParticleSystem breakEffect = default!;
    [Header("�j��SE")]
    [SerializeField, Required] AudioSource _breakAudioSource = default!;

    public void BlockBreak(Collider other)
    {
        breakEffect.transform.position = other.gameObject.transform.position;
        Destroy(other.gameObject);

        breakEffect.Play();
        SoundManager.Instance.PlaySE(_breakAudioSource, SoundSource.SE066_BreakBlock);
    }
}
