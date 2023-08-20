using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostProcessController : MonoBehaviour
{
    [SerializeField]
    float intensity;
    [SerializeField]
    Volume postFXvolume;
    [SerializeField]
    private Animator _vinetteAnim;

    private bool _isStartVignette = false;
    private Vignette _vignette;

    private void Awake()
    {
        postFXvolume.profile.TryGet(out _vignette);
        if (_vignette == null) return;
    }

    void Update()
    {
        if (!_isStartVignette) return;
        _vignette.intensity.value = intensity;
    }

    public void OnVignette()
    {
        _isStartVignette = true;
        _vinetteAnim.SetBool("IsVignette",_isStartVignette );
    }
    

}