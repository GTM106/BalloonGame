using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// ���D���c�����Ă��鎞�̃v���C���[�̏���
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

    public void Dash()
    {
        Vector2 axis = _playerParameter.JoyconRight.Stick.SnapToFourDirections();
        axis += Gamepad.current.leftStick.ReadValue();

        //Y�𖳎�
        Vector3 cameraForward = Vector3.Scale(_playerParameter.CameraTransform.forward, ignoreYCorrection).normalized;
        Vector3 cameraRight = Vector3.Scale(_playerParameter.CameraTransform.right, ignoreYCorrection).normalized;

        Vector3 moveVec = (axis.y * cameraForward + axis.x * cameraRight);
        Vector3 force = moveVec.normalized * (_playerParameter.MoveSpeed);

        //�ő�X�s�[�h�𒴂�����������̐��䂪�ł��Ȃ��悤�ɂ���B
        //�㏸�◎��(Y)�̑��x�͍ő�X�s�[�h�Ɋ܂߂Ȃ��B
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

    public async void BoostDash()
    {
        Vector3 velocity = _playerParameter.CameraTransform.forward.normalized * _playerParameter.BoostDashPower;
        _rigidbody.velocity = velocity;

        for (int i = 0; i < _playerParameter.BoostFrame; i++)
        {
            await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
            _rigidbody.velocity = velocity;
        }

        _rigidbody.velocity = Vector3.zero;
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
        _rigidbody.AddForce(Vector3.up * _playerParameter.BuoyancyExpand, ForceMode.Acceleration);
    }
}
