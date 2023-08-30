using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AirVentHandler : MonoBehaviour
{
    [SerializeField] InputActionReference _ringPushAction = default!;

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

    }
}
