using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngameStartEvent : MonoBehaviour
{
    public event Action OnStart;
    
    public void CalledStartEvent()
    {
        OnStart?.Invoke();
    }
}
