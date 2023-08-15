using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerParameter
{
    [SerializeField] Rigidbody _rb = default!;
    [SerializeField] Transform _cameraTransform = default!;
    [SerializeField] JoyconHandler _joyconRight = default!;
    [SerializeField] JoyconHandler _joyconLeft = default!;
    [Header("ジャンプ時のパワー")]
    [SerializeField, Min(0f)] float _jumpPower = default!;
    [Header("移動速度")]
    [SerializeField, Min(0f)] float _moveSpeed = default!;
    [Header("通常時の重力")]
    [SerializeField] float _multiplierNormal = default!;
    [Header("膨張時の重力")]
    [SerializeField] float _multiplierExpand = default!;
    [Header("ぶっ飛びジャンプの力。frameはBalloonと同じ値にしてください")]
    [SerializeField, Min(0f)] float _boostDashPower = default!;
    [SerializeField, Min(0)] int _boostFrame = default!;
    public Rigidbody Rb => _rb;
    public Transform CameraTransform => _cameraTransform;
    public JoyconHandler JoyconRight => _joyconRight;
    public JoyconHandler JoyconLeft => _joyconLeft;
    public float JumpPower => _jumpPower;
    public float MoveSpeed => _moveSpeed;
    public float MultiplierNormal => _multiplierNormal;
    public float MultiplierExpand => _multiplierExpand;
    public float BoostDashPower => _boostDashPower;
    public int BoostFrame => _boostFrame;
}
