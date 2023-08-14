using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ���D���c�����Ă��鎞�̃v���C���[�̏���
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

        //Y�𖳎�
        Vector3 cameraForward = Vector3.Scale(_playerParameter.CameraTransform.forward, ignoreYCorrection).normalized;
        Vector3 cameraRight = Vector3.Scale(_playerParameter.CameraTransform.right, ignoreYCorrection).normalized;

        Vector3 moveVec = (axis.y * cameraForward + axis.x * cameraRight);
        Vector3 force = moveVec.normalized * (_playerParameter.MoveSpeed);

        _playerParameter.Rb.velocity = new(force.x, _playerParameter.Rb.velocity.y, force.z);
    }

    public async void BoostDash()
    {
        Vector3 velocity = _playerParameter.CameraTransform.forward.normalized * _playerParameter.BoostDashPower;
        _playerParameter.Rb.velocity = velocity;
        
        for (int i = 0; i < _playerParameter.BoostFrame; i++)
        {
            await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
            _playerParameter.Rb.velocity = velocity;
        }

        _playerParameter.Rb.velocity = Vector3.zero;
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
