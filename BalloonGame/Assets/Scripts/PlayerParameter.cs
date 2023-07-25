using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]

public class PlayerParameter
{
    [SerializeField] Rigidbody _rb = default!;
    [SerializeField] Transform cameraTransform = default!;
    [SerializeField] JoyconHandler _joyconHandler = default!;
    [Header("�W�����v���̃p���[")]
    [SerializeField, Min(0f)] float _jumpPower = default!;
    [Header("�ړ����x")]
    [SerializeField] float _moveSpeed = default!;


    public Rigidbody Rb => _rb;
    public Transform CameraTransform => cameraTransform;
    public JoyconHandler JoyconHandler => _joyconHandler;
    public float JumpPower => _jumpPower;
    public float MoveSpeed => _moveSpeed;

}
