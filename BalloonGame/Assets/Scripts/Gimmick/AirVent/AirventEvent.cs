using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirventEvent : MonoBehaviour
{
    public event Action OnEnterAirVent = default!;
    public event Action OnExitAirVent = default!;

    public void OnEnter()
    {
        OnEnterAirVent?.Invoke();
    }

    public void OnExit()
    {
        OnExitAirVent?.Invoke();
    }
}
