using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BalloonHandler : MonoBehaviour
{
    [SerializeField] InputActionReference _ringPushAction = default!;
    [SerializeField] BalloonController _balloonController = default!;

    private void Awake()
    {
        _ringPushAction.action.performed += OnRingconPushed;
    }

    private void OnRingconPushed(InputAction.CallbackContext obj)
    {
        _balloonController.Expand();
    }
}
