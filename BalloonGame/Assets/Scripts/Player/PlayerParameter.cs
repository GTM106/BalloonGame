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
    [Header("�W�����v���̃p���[")]
    [SerializeField, Min(0f)] float _jumpPower = default!;
    [Header("�ړ����x")]
    [SerializeField, Min(0f)] float _moveSpeed = default!;
    [Header("�ʏ펞�̏d��")]
    [SerializeField] float _multiplierNormal = default!;
    [Header("�c�����̏d��")]
    [SerializeField] float _multiplierExpand = default!;
    [Header("�Ԃ���уW�����v�̗́Bframe��Balloon�Ɠ����l�ɂ��Ă�������")]
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
