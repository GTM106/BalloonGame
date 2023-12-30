using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunglassController : MonoBehaviour
{
    [SerializeField, Required] GameObject _sunglassObject = default!;

    private void Awake()
    {
        Disable();
    }

    public void Enable()
    {
        _sunglassObject.SetActive(true);
    }    
    
    public void Disable()
    {
        _sunglassObject.SetActive(false);
    }
}
