using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DeflatablePlayer : IPlayer
{
    readonly PlayerParameter _playerParameter;
    readonly Rigidbody _rigidbody;
    static readonly Vector3 ignoreYCorrection = new(1f, 0f, 1f);

    public float Multiplier => _playerParameter.MultiplierNormal;

    public DeflatablePlayer(PlayerParameter playerParameter)
    {
        _playerParameter = playerParameter;
        _rigidbody = playerParameter.Rb;
    }

    public void Dash()
    {
        Vector2 axis = _playerParameter.JoyconRight.Stick;

        //Y�𖳎�
        Vector3 cameraForward = Vector3.Scale(_playerParameter.CameraTransform.forward, ignoreYCorrection).normalized;
        Vector3 cameraRight = Vector3.Scale(_playerParameter.CameraTransform.right, ignoreYCorrection).normalized;

        Vector3 moveVec = (axis.y * cameraForward + axis.x * cameraRight);
        Vector3 force = moveVec.normalized * (_playerParameter.MoveSpeed);
        force.Set(force.x, 0f, force.z);

        //�ő�X�s�[�h�𒴂�����������̐��䂪�ł��Ȃ��悤�ɂ���B
        //�㏸�◎���̑��x�͍ő�X�s�[�h�Ɋ܂߂Ȃ��B
        Vector3 currentVelocityIgnoreY = Vector3.Scale(_rigidbody.velocity, ignoreYCorrection);

        if (currentVelocityIgnoreY.magnitude < _playerParameter.MaxMoveSpeed)
        {
            //�w�肵���X�s�[�h���猻�݂̑��x�������ĉ����͂����߂�
            float currentSpeed = _playerParameter.MoveSpeed - currentVelocityIgnoreY.magnitude;
            
            //�������ꂽ�����͂ŗ͂�������
            _rigidbody.AddForce(force * currentSpeed);
        }      

        if (axis.magnitude <= 0.02f) return;

        //�i�s����������
        Vector3 direction = cameraForward * axis.y + cameraRight * axis.x;
        _rigidbody.transform.localRotation = Quaternion.LookRotation(direction);
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
        _rigidbody.AddForce((Multiplier - 1f) * Physics.gravity, ForceMode.Acceleration);
    }

    public void OnWaterStay()
    {
        _rigidbody.AddForce(Vector3.up * _playerParameter.BuoyancyNormal, ForceMode.Acceleration);
    }
}
