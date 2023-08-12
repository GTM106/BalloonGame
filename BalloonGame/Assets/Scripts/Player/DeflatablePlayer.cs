using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeflatablePlayer : IPlayer
{
    readonly PlayerParameter _playerParameter;

    public DeflatablePlayer(PlayerParameter playerParameter)
    {
        _playerParameter = playerParameter;
    }

    public void BoostDash()
    {
        throw new System.NotImplementedException();
    }

    public void Dash()
    {
        Vector2 axis = _playerParameter.JoyconHandler.Stick;

        if (axis.magnitude <= 0.02f) return;

        //Y‚ð–³Ž‹
        Vector3 cameraForward = Vector3.Scale(_playerParameter.CameraTransform.forward, new Vector3(1f, 0f, 1f)).normalized;

        Vector3 moveVec = (axis.y * cameraForward + axis.x * _playerParameter.CameraTransform.right);
        Vector3 force = moveVec.normalized * (_playerParameter.TargetMoveSpeed - _playerParameter.Rb.velocity.magnitude);

        _playerParameter.Rb.AddForce(force * _playerParameter.MovePower, ForceMode.Acceleration);
    }

    public void Jump(Rigidbody rb)
    {
        rb.AddForce(Vector3.up * _playerParameter.JumpPower, ForceMode.Impulse);
    }
}
