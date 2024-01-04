using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SharkHandler : MonoBehaviour
{
    [SerializeField] InputActionReference _ringPushAction = default!;
    public event Action OnRingconPush;

    private void Awake()
    {
        _ringPushAction.action.performed += OnRingconPushed;
    }

    private void OnDestroy()
    {
        _ringPushAction.action.performed -= OnRingconPushed;
    }

    private void OnRingconPushed(InputAction.CallbackContext obj)
    {
        OnRingconPush?.Invoke();
    }
}
