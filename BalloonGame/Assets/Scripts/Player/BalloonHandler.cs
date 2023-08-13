using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.Experimental.GraphView.GraphView;

public class BalloonHandler : MonoBehaviour
{
    [SerializeField] InputActionReference _ringPushAction = default!;
    [SerializeField] InputActionReference _ringPullAction = default!;
    [SerializeField] BalloonController _balloonController = default!;

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
        _balloonController.Expand();
    }
    private void OnRingconPulled(InputAction.CallbackContext obj)
    {
        _balloonController.OnRingconPull();
    }
}
