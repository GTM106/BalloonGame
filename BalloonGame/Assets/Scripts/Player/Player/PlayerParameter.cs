using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BoostDashDirection
{
    CameraForward,
    PlayerForward
}

[System.Serializable]
public class PlayerParameter
{
    [SerializeField, Required] Rigidbody _rb = default!;
    [SerializeField, Required] PhysicMaterial _physicMaterial = default!;
    [SerializeField, Required] Transform _cameraTransform = default!;
    [SerializeField, Required] JoyconHandler _joyconRight = default!;
    [SerializeField, Required] JoyconHandler _joyconLeft = default!;
    [Header("�W�����v���̃p���[")]
    [SerializeField, Min(0f)] float _jumpPower = default!;
    [Header("�ړ����x")]
    [SerializeField, Min(0f)] float _moveSpeed = default!;
    [Header("�ő�ړ����x�B���Ȃǂɂ�肱����傫���Ȃ�\���͂���܂�")]
    [SerializeField, Min(0f)] float _maxMoveSpeed = default!;
    [Header("�ʏ펞�̏d��")]
    [SerializeField] float _multiplierNormal = default!;
    [Header("�c�����̏d��")]
    [SerializeField] float _multiplierExpand = default!;
    [Header("�Ԃ���у_�b�V����Y���̗́B�����قǍ�������")]
    [SerializeField, Min(0f)] float _boostDashPowerY = default!;
    [Header("�c�����ɂ����鐅�ɓ����Ă���Ƃ��̕���")]
    [SerializeField] float _buoyancyExpand = default!;
    [Header("�ʏ펞�ɂ����鐅�ɓ����Ă���Ƃ��̕���")]
    [SerializeField] float _buoyancyNormal = default!;
    [Header("�Q�[���I�[�o�[����̕����ɕK�v�ȃv�b�V����")]
    [SerializeField, Min(0)] int _requiredPushCount = default!;
    [Header("�A�j���[�V����")]
    [SerializeField] AnimationChanger<E_Atii> _animationChanger = default!;
    [Header("�⓹�̑��x�𒲐����܂��B-1����1�̊Ԃ�")]
    [SerializeField] AnimationCurve _slopeSpeed = default!;
    [Header("�������̉����Binflated��Player�c��ݎ�")]
    [SerializeField, Min(0)] float _inflatedFallSpeed = default!;
    [SerializeField, Min(0)] float _deflatedFallSpeed = default!;

    public Rigidbody Rb => _rb;
    public PhysicMaterial PhysicMaterial => _physicMaterial;
    public Transform CameraTransform => _cameraTransform;
    public JoyconHandler JoyconRight => _joyconRight;
    public JoyconHandler JoyconLeft => _joyconLeft;
    public float JumpPower => _jumpPower;
    public float MoveSpeed => _moveSpeed;
    public float MaxMoveSpeed => _maxMoveSpeed;
    public float MultiplierNormal => _multiplierNormal;
    public float MultiplierExpand => _multiplierExpand;
    public float BoostDashPowerY => _boostDashPowerY;
    public float BuoyancyExpand => _buoyancyExpand;
    public float BuoyancyNormal => _buoyancyNormal;
    public int RequiredPushCount => _requiredPushCount;
    public AnimationChanger<E_Atii> AnimationChanger => _animationChanger;
    public float SloopSpeed(float angle) => _slopeSpeed.Evaluate(angle);
    public float InflatedFallSpeed => _inflatedFallSpeed;
    public float DeflatedFallSpeed => _deflatedFallSpeed;
}
