using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.VFX;

public class SampleOnDashFullScreenEffect : MonoBehaviour
{
    [SerializeField]FullScreenPassRendererFeature rendererFeature;
    [SerializeField] float veloThreshold;

    private Rigidbody rb;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    // Update is called once per frame
    void Update()
    {
        //��葬�x�ȏ�ŃA�N�e�B�u

        if(Mathf.CeilToInt(rb.velocity.magnitude) > veloThreshold)
        {
            rendererFeature.SetActive(true);

        }
        else
        {
            rendererFeature.SetActive(false);
        }
    }
}
