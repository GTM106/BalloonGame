using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class BreakBlockContorller : MonoBehaviour
{
    [Header("破壊時エフェクト")]
    [SerializeField] VisualEffect breakEffect = default!;
    [Header("破壊時SE")]
    [SerializeField, Required] AudioSource _breakAudioSource = default!;
    public void BlockBreak(Collision collision)
    {
        Destroy(collision.gameObject);
        SoundManager.Instance.PlaySE(_breakAudioSource, SoundSource.SE062_BreakBlock);
        breakEffect.Play();
    }
}
