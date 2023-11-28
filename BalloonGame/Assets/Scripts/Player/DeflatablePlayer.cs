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
        force = Vector3.ProjectOnPlane(force, groundNormal);

        if (currentVelocityIgnoreY.magnitude < _playerParameter.MaxMoveSpeed)
        {
            //指定したスピードから現在の速度を引いて加速力を求める
            float currentSpeed = _playerParameter.MoveSpeed - currentVelocityIgnoreY.magnitude;

            //調整された加速力で力を加える
            _rigidbody.AddForce(force * currentSpeed);
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

    public void BoostDash(BoostDashData boostFrame)
    {
        //ぶっ飛びダッシュ失敗時の処理を記述
        Debug.Log("ぶっ飛びダッシュをするには風船が膨らんでいる必要があります");
    }

    public void Jump(Rigidbody rb)
    {
        rb.AddForce(Vector3.up * _playerParameter.JumpPower, ForceMode.Impulse);

        _playerParameter.AnimationChanger.ChangeAnimation(E_Atii.Jump);
    }

    public void AdjustingGravity()
    {
        //1を基準とする値だけ重力を追加で掛ける
        _rigidbody.AddForce((Multiplier - 1f) * Physics.gravity, ForceMode.Acceleration);
    }

    public void OnWaterStay()
    {
        _rigidbody.AddForce(Vector3.up * _playerParameter.BuoyancyNormal, ForceMode.Acceleration);

        if (_rigidbody.velocity.magnitude < 0.01f)
        {
            _playerParameter.AnimationChanger.ChangeAnimation(E_Atii.Swim);
        }
    }

    public void Fall()
    {
        _playerParameter.AnimationChanger.ChangeAnimation(E_Atii.Fall);
    }

    private Vector3 GetGroundNormal()
    {
        float raycastDistance = 2f;

        if (Physics.Raycast(_rigidbody.position, Vector3.down, out RaycastHit hit, raycastDistance))
        {
            return hit.normal;
        }

        //ヒットしなかった場合、上向きの法線
        return Vector3.up;
    }
}
