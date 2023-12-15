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
    [Header("ジャンプ時のパワー")]
    [SerializeField, Min(0f)] float _jumpPower = default!;
    [Header("移動速度")]
    [SerializeField, Min(0f)] float _moveSpeed = default!;
    [Header("最大移動速度。風などによりこれより大きくなる可能性はあります")]
    [SerializeField, Min(0f)] float _maxMoveSpeed = default!;
    [Header("通常時の重力")]
    [SerializeField] float _multiplierNormal = default!;
    [Header("膨張時の重力")]
    [SerializeField] float _multiplierExpand = default!;
    [Header("ぶっ飛びダッシュのY軸の力。高いほど高く跳ぶ")]
    [SerializeField, Min(0f)] float _boostDashPowerY = default!;
    [Header("膨張時における水に入っているときの浮力")]
    [SerializeField] float _buoyancyExpand = default!;
    [Header("通常時における水に入っているときの浮力")]
    [SerializeField] float _buoyancyNormal = default!;
    [Header("ゲームオーバーからの復活に必要なプッシュ数")]
    [SerializeField, Min(0)] int _requiredPushCount = default!;
    [Header("アニメーション")]
    [SerializeField] AnimationChanger<E_Atii> _animationChanger = default!;
    [Header("坂道の速度を調整します。-1から1の間で")]
    [SerializeField] AnimationCurve _slopeSpeed = default!;
    [Header("落下時の加速。inflatedがPlayer膨らみ時")]
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
