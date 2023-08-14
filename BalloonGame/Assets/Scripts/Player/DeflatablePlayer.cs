using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeflatablePlayer : IPlayer
{
    readonly PlayerParameter _playerParameter;
    static readonly Vector3 ignoreYCorrection = new(1f, 0f, 1f);

    public float Multiplier => _playerParameter.MultiplierNormal;

    public DeflatablePlayer(PlayerParameter playerParameter)
    {
        _playerParameter = playerParameter;
    }

    public void Dash()
    {
        Vector2 axis = _playerParameter.JoyconRight.Stick;

        //Y�𖳎�
        Vector3 cameraForward = Vector3.Scale(_playerParameter.CameraTransform.forward, ignoreYCorrection).normalized;
        Vector3 cameraRight = Vector3.Scale(_playerParameter.CameraTransform.right, ignoreYCorrection).normalized;

        Vector3 moveVec = (axis.y * cameraForward + axis.x * cameraRight);
        Vector3 force = moveVec.normalized * (_playerParameter.TargetMoveSpeed);

        _playerParameter.Rb.velocity = new(force.x, _playerParameter.Rb.velocity.y, force.z);
    }

    public void BoostDash()
    {
        //�Ԃ���у_�b�V�����s���̏������L�q
        Debug.Log("�Ԃ���у_�b�V��������ɂ͕��D���c���ł���K�v������܂�");
    }

    public void Jump(Rigidbody rb)
    {
        rb.AddForce(Vector3.up * _playerParameter.JumpPower, ForceMode.Impulse);
    }

    public void AdjustingGravity()
    {
        //1����Ƃ���l�����d�͂�ǉ��Ŋ|����
        _playerParameter.Rb.AddForce((Multiplier - 1f) * Physics.gravity, ForceMode.Acceleration);
    }
}
