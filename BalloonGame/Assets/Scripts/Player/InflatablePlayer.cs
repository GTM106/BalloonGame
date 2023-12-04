using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 風船が膨張している時のプレイヤーの処理
/// </summary>
public class InflatablePlayer : IPlayer
{
    readonly PlayerParameter _playerParameter;
    readonly Rigidbody _rigidbody;

    static readonly Vector3 ignoreYCorrection = new(1f, 0f, 1f);

    public float Multiplier => _playerParameter.MultiplierExpand;

    public InflatablePlayer(PlayerParameter playerParameter)
    {
        _playerParameter = playerParameter;
        _rigidbody = _playerParameter.Rb;
    }

    public void Dash(IState.E_State state)
    {
        Vector2 axis = _playerParameter.JoyconRight.Stick;
#if UNITY_EDITOR
        if (Gamepad.current != null)
        {
            axis += Gamepad.current.leftStick.ReadValue();
        }
        if (Keyboard.current != null)
        {
            axis += new Vector2(Keyboard.current.aKey.isPressed ? -1f : Keyboard.current.dKey.isPressed ? 1f : 0f
              , Keyboard.current.sKey.isPressed ? -1f : Keyboard.current.wKey.isPressed ? 1f : 0f);
        }
#endif
        //Yを無視
        Vector3 cameraForward = Vector3.Scale(_playerParameter.CameraTransform.forward, ignoreYCorrection).normalized;
        Vector3 cameraRight = Vector3.Scale(_playerParameter.CameraTransform.right, ignoreYCorrection).normalized;

        Vector3 moveVec = (axis.y * cameraForward + axis.x * cameraRight);
        Vector3 force = moveVec.normalized * (_playerParameter.MoveSpeed);
        force.Set(force.x, 0f, force.z);

        //最大スピードを超えたら加速等の制御ができないようにする。
        //上昇や落下の速度は最大スピードに含めない。
        Vector3 currentVelocityIgnoreY = Vector3.Scale(_rigidbody.velocity, ignoreYCorrection);

        Vector3 groundNormal = GetGroundNormal();

        //進行方向を地面に投影したベクトル
        Vector3 projectOnPlaneForce = Vector3.ProjectOnPlane(force, groundNormal).normalized;

        //地面の法線と進行方向のなす角度
        float angle = Vector3.Angle(force, groundNormal);

        //調整する重力
        Vector3 gravity = Multiplier * Physics.gravity;

        //坂道の調整
        if (Mathf.Approximately(0f, angle)) gravity = Vector3.zero;
        if (Mathf.Approximately(90f, angle)) gravity = Vector3.zero;
        float slope = (90f - angle) / 90f;
        _rigidbody.AddForce(gravity * _playerParameter.SloopSpeed(slope), ForceMode.Acceleration);

        float currentForceMag = force.magnitude;

        if (currentVelocityIgnoreY.magnitude < _playerParameter.MaxMoveSpeed)
        {
            //指定したスピードから現在の速度を引いて加速力を求める
            float currentSpeed = _playerParameter.MoveSpeed - currentVelocityIgnoreY.magnitude;

            _rigidbody.AddForce(currentForceMag * currentSpeed * projectOnPlaneForce, ForceMode.Acceleration);
        }

        if (axis.magnitude <= 0.02f) return;

        //進行方向を向く
        Vector3 direction = cameraForward * axis.y + cameraRight * axis.x;
        _rigidbody.transform.localRotation = Quaternion.LookRotation(direction);

        if (state is IState.E_State.Control)
        {
            _playerParameter.AnimationChanger.ChangeAnimation(E_Atii.Run);
        }
    }

    public async void BoostDash(BoostDashData boostFrame)
    {
        Vector3 dir = _playerParameter.BoostDashType switch
        {
            BoostDashDirection.CameraForward => _playerParameter.CameraTransform.forward,
            BoostDashDirection.PlayerForward => _rigidbody.transform.forward,
            _ => throw new System.NotImplementedException()
        };

        Vector3 velocity = dir.normalized * _playerParameter.BoostDashPower(boostFrame);
        velocity.Set(velocity.x, _playerParameter.BoostDashAngle, velocity.z);

        _playerParameter.AnimationChanger.ChangeAnimation(E_Atii.BDash);

        //処理中に変更されるおそれがあるため値を保存しておく
        int maxFrame = boostFrame.Value;

        for (int currentFrame = 0; currentFrame < maxFrame; currentFrame++)
        {
            await UniTask.Yield(PlayerLoopTiming.FixedUpdate);

            _rigidbody.velocity = velocity * _playerParameter.BoostDashSpeed(currentFrame, maxFrame);
        }

        _rigidbody.velocity = Vector3.zero;
    }

    public void Jump(Rigidbody rb)
    {
        rb.AddForce(Vector3.up * _playerParameter.JumpPower, ForceMode.Impulse);

        _playerParameter.AnimationChanger.ChangeAnimation(E_Atii.BJump);
    }

    public void AdjustingGravity()
    {
        //1を基準とする値だけ重力を追加で掛ける
        _rigidbody.AddForce((Multiplier - 1f) * Physics.gravity, ForceMode.Acceleration);
    }

    public void OnWaterStay()
    {
        _rigidbody.AddForce(Vector3.up * _playerParameter.BuoyancyExpand, ForceMode.Acceleration);
    }

    public void Fall()
    {
        _playerParameter.AnimationChanger.ChangeAnimation(E_Atii.BFall);
    }

    private Vector3 GetGroundNormal()
    {
        float raycastDistance = 1.5f;

        if (Physics.Raycast(_rigidbody.position, Vector3.down, out RaycastHit hit, raycastDistance))
        {
            return hit.normal;
        }

        //ヒットしなかった場合、上向きの法線
        return Vector3.up;
    }
}
