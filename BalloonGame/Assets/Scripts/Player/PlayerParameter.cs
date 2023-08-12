using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerParameter
{
    [SerializeField] Rigidbody _rb = default!;
    [SerializeField] Transform _cameraTransform = default!;
    [SerializeField] JoyconHandler _joyconHandler = default!;
    [Header("ジャンプ時のパワー")]
    [SerializeField, Min(0f)] float _jumpPower = default!;
    [Header("移動速度")]
    [SerializeField, Min(0f)] float _moveSpeed = default!;

    public Rigidbody Rb => _rb;
    public Transform CameraTransform => _cameraTransform;
    public JoyconHandler JoyconHandler => _joyconHandler;
    public float JumpPower => _jumpPower;
    public float MoveSpeed => _moveSpeed;
}
