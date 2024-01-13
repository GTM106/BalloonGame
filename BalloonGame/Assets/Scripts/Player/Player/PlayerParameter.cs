using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BoostDashDirection
{
    CameraForward,
    PlayerForward
}

[Flags]
public enum EnvironmentStatus
{
    None = 0,

    Underwater = 1 << 0,//水中にいる
    WindAffected = 1 << 1,//風の影響を受けている
    GameOver = 1 << 2,//ゲームオーバーの状態になっている
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
    EnvironmentStatus _environmentStatus;

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

    public void ChangeRunAnimation()
    {
        //デフォルトのモーション
        E_Atii runAnimation = E_Atii.Run;

        //水中なら水中用モーション
        //最優先
        if (IsBitSetEnvironmentStatus(EnvironmentStatus.Underwater))
        {
            runAnimation = E_Atii.Swimming;
        }

        //風は変更なし

        _animationChanger.ChangeAnimation(runAnimation);
    }

    public void ChangeIdleAnimation()
    {
        //デフォルトのモーション
        E_Atii idleAnimation = E_Atii.Idle;

        //水中なら水中用モーション
        //最優先
        if (IsBitSetEnvironmentStatus(EnvironmentStatus.Underwater))
        {
            idleAnimation = E_Atii.Swim;
        }
        //風影響下なら風影響下用モーション
        else if (IsBitSetEnvironmentStatus(EnvironmentStatus.WindAffected))
        {
            idleAnimation = E_Atii.IdleWind;
        }

        _animationChanger.ChangeAnimation(idleAnimation);
    }

    public void ChangeFallAnimation()
    {
        //デフォルトのモーション
        E_Atii animation = E_Atii.Fall;

        //水中なら落下時モーションなし
        //最優先
        if (IsBitSetEnvironmentStatus(EnvironmentStatus.Underwater))
        {
            return;
        }

        //ゲームオーバーなら落下モーションなし
        if (IsBitSetEnvironmentStatus(EnvironmentStatus.GameOver))
        {
            return;
        }

        //風は変更なし

        _animationChanger.ChangeAnimation(animation);
    }

    public void ChangeBFallAnimation()
    {
        //デフォルトのモーション
        E_Atii animation = E_Atii.BFall;

        //水中なら落下時モーションなし
        //最優先
        if (IsBitSetEnvironmentStatus(EnvironmentStatus.Underwater))
        {
            return;
        }        
        
        //ゲームオーバーなら落下モーションなし
        if (IsBitSetEnvironmentStatus(EnvironmentStatus.GameOver))
        {
            return;
        }

        //風は変更なし

        _animationChanger.ChangeAnimation(animation);
    }

    public void BitSetEnvironmentStatus(EnvironmentStatus addType)
    {
        _environmentStatus |= addType;
    }

    public void BitClearEnvironmentStatus(EnvironmentStatus delType)
    {
        _environmentStatus &= ~delType;
    }

    public bool IsBitSetEnvironmentStatus(EnvironmentStatus type)
    {
        return type == (_environmentStatus & type);
    }
}
