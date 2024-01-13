using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FunEvent : MonoBehaviour
{
    public event Action<Vector3> OnEnterAction;
    public event Action<Vector3> OnStayAction;
    public event Action<Vector3> OnExitAction;

    public void OnEnter(Vector3 windVec)
    {
        OnEnterAction?.Invoke(windVec);
    }

    public void OnStay(Vector3 windVec)
    {
        OnStayAction?.Invoke(windVec);
    }

    public void OnExit(Vector3 windVec)
    {
        OnExitAction?.Invoke(windVec);
    }
}
