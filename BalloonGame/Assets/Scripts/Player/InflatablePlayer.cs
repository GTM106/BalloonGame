using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 風船が膨張している時のプレイヤーの処理
/// </summary>
public class InflatablePlayer : IPlayer
{
    PlayerParameter _playerParameter;

    public InflatablePlayer(PlayerParameter playerParameter)
    {
        _playerParameter = playerParameter;
    }

    public void Dash()
    {
        Vector2 axis = _playerParameter.JoyconHandler.Stick;

        if (axis.magnitude <= 0.02f) return;

        //Yを無視
        Vector3 cameraForward = Vector3.Scale(_playerParameter.CameraTransform.forward, new Vector3(1f, 0f, 1f)).normalized;

        Vector3 moveVec = (axis.y * cameraForward + axis.x * _playerParameter.CameraTransform.right);
        Vector3 force = moveVec.normalized * (_playerParameter.TargetMoveSpeed - _playerParameter.Rb.velocity.magnitude);

        _playerParameter.Rb.AddForce(force * _playerParameter.MovePower, ForceMode.Acceleration);
    }

    public void BoostDash()
    {

    }

    public void Jump(Rigidbody rb)
    {
        rb.AddForce(Vector3.up * _playerParameter.JumpPower, ForceMode.Impulse);
    }
}
