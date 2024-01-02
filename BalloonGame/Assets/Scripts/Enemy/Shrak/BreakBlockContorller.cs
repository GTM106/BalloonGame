using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class BreakBlockContorller : MonoBehaviour
{
    [Header("jóGtFNg")]
    [SerializeField] VisualEffect breakEffect = default!;
    [Header("jóSE")]
    [SerializeField, Required] AudioSource _breakAudioSource = default!;
    public void BlockBreak(Collision collision)
    {
        Destroy(collision.gameObject);
        SoundManager.Instance.PlaySE(_breakAudioSource, SoundSource.SE062_BreakBlock);
        breakEffect.Play();
    }
}
