using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SharkEvent : MonoBehaviour
{
    public event Action OnEnterShark = default!;
    public event Action OnExitShark = default!;

    public void OnEnter()
    {
        OnEnterShark?.Invoke();
    }

    public void OnExit()
    {
        OnExitShark?.Invoke();
    }
}
