using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterEvent : MonoBehaviour
{
    public event Action OnEnterAction;
    public event Action OnStayAction;
    public event Action OnExitAction;

    public void OnEnter()
    {
        OnEnterAction?.Invoke();
    }

    public void OnStay()
    {
        OnStayAction?.Invoke();
    }

    public void OnExit()
    {
        OnExitAction?.Invoke();
    }
}
