using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AirVentHandler : MonoBehaviour
{
    [SerializeField] InputActionReference _ringPushAction = default!;
    [SerializeField] InputActionReference _ringPullAction = default!;

    private void Awake()
    {
        _ringPushAction.action.performed += OnRingconPushed;
        _ringPullAction.action.performed += OnRingconPulled;
    }

    private void OnDestroy()
    {
        _ringPushAction.action.performed -= OnRingconPushed;
        _ringPullAction.action.performed -= OnRingconPulled;
    }

    private void OnRingconPushed(InputAction.CallbackContext obj)
    {
    }
    private void OnRingconPulled(InputAction.CallbackContext obj)
    {

    }
}
