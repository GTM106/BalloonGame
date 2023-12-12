using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BoostDashCommand : MonoBehaviour
{
    [SerializeField, Required] BoostDashEvent _boostDashEvent = default!;

    [SerializeField, Required] JoyconHandler _joyconRight = default!;
    [SerializeField, Required] InputActionReference _ringconPullAction = default!;
    [SerializeField, Required] InputActionReference _ringconPushHoldAction = default!;

    [Header("ジョイコンの十字ボタンでのぶっ飛びダッシュを許可")]
    [SerializeField] bool enableOnPressedJoyconButton = true;
    [Header("リングコン引っ張りでのぶっ飛びダッシュを許可")]
    [SerializeField] bool enableOnRingconPull = true;
    [Header("リングコン押し込みホールドでのぶっ飛びダッシュを許可")]
    [SerializeField] bool enableOnRingconPushHold = true;

    bool _isRingconPushHold = false;

    private void Awake()
    {
        if (enableOnPressedJoyconButton)
        {
            _joyconRight.OnRightButtonPressed += BoostDash;
            _joyconRight.OnLeftButtonPressed += BoostDash;
            _joyconRight.OnUpButtonPressed += BoostDash;
            _joyconRight.OnDownButtonPressed += BoostDash;
        }

        if (enableOnRingconPull)
        {
            _ringconPullAction.action.performed += BoostDashAction;
        }

        if (enableOnRingconPushHold)
        {
            _ringconPushHoldAction.action.performed += HoldPerformed;
            _ringconPushHoldAction.action.canceled += HoldCanceled;
        }
    }

    private void OnDestroy()
    {
        if (enableOnPressedJoyconButton)
        {
            _joyconRight.OnRightButtonPressed -= BoostDash;
            _joyconRight.OnLeftButtonPressed -= BoostDash;
            _joyconRight.OnUpButtonPressed -= BoostDash;
            _joyconRight.OnDownButtonPressed -= BoostDash;
        }

        if (enableOnRingconPull)
        {
            _ringconPullAction.action.performed -= BoostDashAction;
        }

        if (enableOnRingconPushHold)
        {
            _ringconPushHoldAction.action.performed -= HoldPerformed;
            _ringconPushHoldAction.action.canceled -= HoldCanceled;
        }
    }

    private void HoldPerformed(InputAction.CallbackContext obj)
    {
        _isRingconPushHold = true;
    }

    private void HoldCanceled(InputAction.CallbackContext obj)
    {
        //ホールド状態だったらぶっ飛びダッシュ開始
        if (_isRingconPushHold)
        {
            BoostDash();
        }

        _isRingconPushHold = false;
    }

    private void BoostDashAction(InputAction.CallbackContext obj)
    {
        BoostDash();
    }

    private void BoostDash()
    {
        _boostDashEvent.BoostDash();
    }
}
