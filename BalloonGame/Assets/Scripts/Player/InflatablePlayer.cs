using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

/// <summary>
/// 風船が膨張している時のプレイヤーの処理
/// </summary>
public class InflatablePlayer : IPlayer
{
    readonly PlayerParameter _playerParameter;
    static readonly Vector3 ignoreYCorrection = new(1f, 0f, 1f);

    public float Multiplier => _playerParameter.MultiplierExpand;

    public InflatablePlayer(PlayerParameter playerParameter)
    {
        _playerParameter = playerParameter;
    }

    public void Dash()
    {
        Vector2 axis = _playerParameter.JoyconRight.Stick;

        //Yを無視
        Vector3 cameraForward = Vector3.Scale(_playerParameter.CameraTransform.forward, ignoreYCorrection).normalized;
        Vector3 cameraRight = Vector3.Scale(_playerParameter.CameraTransform.right, ignoreYCorrection).normalized;

        Vector3 moveVec = (axis.y * cameraForward + axis.x * cameraRight);
        Vector3 force = moveVec.normalized * (_playerParameter.TargetMoveSpeed);

        _playerParameter.Rb.velocity = new(force.x, _playerParameter.Rb.velocity.y, force.z);
    }

    public async void BoostDash()
    {
        _playerParameter.Rb.velocity = _playerParameter.CameraTransform.forward.normalized * _playerParameter.BoostDashPower;
        await UniTask.DelayFrame(_playerParameter.BoostFlame, PlayerLoopTiming.FixedUpdate);
        _playerParameter.Rb.velocity = Vector3.zero;
    }

    public void Jump(Rigidbody rb)
    {
        rb.AddForce(Vector3.up * _playerParameter.JumpPower, ForceMode.Impulse);
    }

    public void AdjustingGravity()
    {
        //1を基準とする値だけ重力を追加で掛ける
        _playerParameter.Rb.AddForce((Multiplier - 1f) * Physics.gravity, ForceMode.Acceleration);
    }
}
