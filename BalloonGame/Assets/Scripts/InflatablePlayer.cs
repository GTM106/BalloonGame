using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ���D���c�����Ă��鎞�̃v���C���[�̏���
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

        //Todo:Axis��magunitude�ɂ����return������
        //Y�𖳎�
        Vector3 cameraForward = Vector3.Scale(_playerParameter.CameraTransform.forward, new Vector3(1f, 0f, 1f)).normalized;

        Vector3 moveVec = (axis.y * cameraForward + axis.x * _playerParameter.CameraTransform.right) * _playerParameter.MoveSpeed;

        _playerParameter.Rb.velocity = moveVec;
    }

    public void BoostDash()
    {

    }

    public void Jump(Rigidbody rb)
    {
        rb.AddForce(Vector3.up * _playerParameter.JumpPower, ForceMode.Impulse);
    }
}
