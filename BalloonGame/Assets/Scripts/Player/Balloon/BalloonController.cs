using Cinemachine;
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum BalloonState
{
    Normal,
    Expands,
    BoostDash,
}

public class BalloonController : MonoBehaviour
{
    [Flags]
    public enum BalloonBehaviorType
    {
        None = 0,
        Expands = 1 << 0, //膨らみ可能
        Deflation = 1 << 1, //萎み可能
        GameOver = 1 << 2, //ゲームオーバー中
        InAirVentArea = 1 << 3, //空気栓範囲内
    }

    //下記のInputActionReferenceは、Handlerの役割をもちます
    [SerializeField, Required] InputActionReference _ringPushAction = default!;

    [SerializeField, Required] WaterEvent _waterEvent = default!;
    [SerializeField, Required] CinemachineTargetGroup _cinemachineTargetGroup = default!;
    [SerializeField, Required] CinemachineController _cinemachineController = default!;
    [SerializeField, Required] AirventEvent _airVentEvent = default!;
    [SerializeField, Required] PlayerGameOverEvent _playerGameOverEvent = default!;
    [SerializeField, Required] Material _MAT_AtiiBalloon = default!;
    [SerializeField, Required] SkinnedMeshRenderer _skinnedMeshRenderer = default!;
    [SerializeField, Required] BoostDashEvent _boostDashEvent = default!;
    [SerializeField, Required] AudioSource _balloonExpandsAudioSource = default!;
    [SerializeField, Required] AudioSource _boostDashAudioSource = default!;

    [Header("膨張アニメーションの持続時間")]
    [SerializeField, Min(0f)] float _scaleAnimationDuration = 0.1f;
    [Header("1回プッシュでどのくらい膨張するか。\nBrendShapeの値を参考にしてください")]
    [SerializeField, Min(0f)] float _scaleOffset = 10f;
    [Header("1秒間にどのくらい風船が縮むか。\nBrendShapeの値を参考にしてください")]
    [SerializeField, Min(0f)] float _scaleAmountDeflatingPerSecond;
    [Header("水に入っているとき1秒間にどのくらい風船が縮むか。\nBrendShapeの値を参考にしてください")]
    [SerializeField, Min(0f)] float _scaleAmountDeflatingPerSecondInWater;
    [Header("CinemachineTargetGroupにおけるradiusの最大値")]
    [SerializeField, Min(1f)] float _cameraRadiusMax = 3.25f;
    [Header("風船のマテリアルのSmoothness値の最大値")]
    [SerializeField, Range(0.4f, 1f)] float _smoothnessMax = 1f;

    //風船の膨らみ具合の初期値。Awakeで初期化しています
    float _defaultBlendShapeWeight;

    bool _isScaleAnimation = false;

    //プロパティの方を使用してください
    BalloonState _state;
    public BalloonState State
    {
        get { return _state; }
        private set
        {
            if (_state != value) OnStateChanged?.Invoke(value);
            _state = value;
        }
    }

    BalloonBehaviorType _behaviorType;

    public event Action<BalloonState> OnStateChanged;

    static readonly float MaxBrandShapeValue = 100f;

    private void Awake()
    {
        _defaultBlendShapeWeight = GetBalloonBlendShapesValue();

        _waterEvent.OnStayAction += OnWaterStay;
        _ringPushAction.action.performed += OnRingconPushed;
        _playerGameOverEvent.OnGameOver += OnGameOver;
        _playerGameOverEvent.OnRevive += OnRevive;
        _airVentEvent.OnEnterAirVent += OnEnterAirVent;
        _airVentEvent.OnExitAirVent += OnExitAirVent;
        _boostDashEvent.OnBoostDash += StartBoostDash;

        State = Mathf.Approximately(_defaultBlendShapeWeight, 0f) ? BalloonState.Normal : BalloonState.Expands;
        BitSet(BalloonBehaviorType.Expands);
        BitSet(BalloonBehaviorType.Deflation);
    }

    private void Update()
    {
        BalloonDeflation(_scaleAmountDeflatingPerSecond);
    }

    private void OnDestroy()
    {
        _waterEvent.OnStayAction -= OnWaterStay;
        _ringPushAction.action.performed -= OnRingconPushed;
        _playerGameOverEvent.OnGameOver -= OnGameOver;
        _playerGameOverEvent.OnRevive -= OnRevive;
        _airVentEvent.OnEnterAirVent -= OnEnterAirVent;
        _airVentEvent.OnExitAirVent -= OnExitAirVent;
        _boostDashEvent.OnBoostDash -= StartBoostDash;
    }

    private void OnEnterAirVent()
    {
        BitClear(BalloonBehaviorType.Expands);
        BitSet(BalloonBehaviorType.InAirVentArea);
    }

    private void OnExitAirVent()
    {
        BitSet(BalloonBehaviorType.Expands);
        BitClear(BalloonBehaviorType.InAirVentArea);
    }

    private void OnRevive()
    {
        BitSet(BalloonBehaviorType.Expands);
        BitClear(BalloonBehaviorType.GameOver);
    }

    private void OnGameOver()
    {
        //風船の空気を抜く
        ChangeWeight(_defaultBlendShapeWeight);

        //カメラの視野角を変更
        _cinemachineTargetGroup.m_Targets[0].radius = BlendShapeWeight2CameraRadius(GetBalloonBlendShapesValue());

        State = BalloonState.Normal;
        BitClear(BalloonBehaviorType.Expands);
        BitSet(BalloonBehaviorType.GameOver);
    }

    private void OnRingconPushed(InputAction.CallbackContext obj)
    {
        Expand();
    }

    private void Expand()
    {
        if (!IsBitSet(BalloonBehaviorType.Expands)) return;

        SoundManager.Instance.PlaySE(_balloonExpandsAudioSource, SoundSource.SE004_PlayerBalloonExpands);
        ExpandScaleAnimation().Forget();
    }

    private async void StartBoostDash(BoostDashData frame)
    {
        var token = this.GetCancellationTokenOnDestroy();

        int boostFrame = frame.Value;

        SoundManager.Instance.PlaySE(_boostDashAudioSource, SoundSource.SE005_PlayerBoostDash);

        State = BalloonState.BoostDash;
        BitClear(BalloonBehaviorType.Expands);

        _cinemachineController.OnAfterBoostDash(boostFrame);

        float startValue = _cinemachineTargetGroup.m_Targets[0].radius;

        //スケールをゼロにする前に1フレーム待機してその他処理を終わらせる
        await UniTask.Yield(PlayerLoopTiming.FixedUpdate, token);

        //スケールを瞬時にゼロにする
        ChangeWeight(0f);

        int currentFrame = 0;

        while (currentFrame <= boostFrame)
        {
            await UniTask.Yield(PlayerLoopTiming.FixedUpdate, token);

            float progress = Mathf.Clamp01((float)currentFrame / boostFrame);

            //吹っ飛びダッシュだけは特例でスケールを無視してカメラの視野角を変更
            _cinemachineTargetGroup.m_Targets[0].radius = startValue * (1f - progress);

            currentFrame++;
        }

        State = BalloonState.Normal;

        //ゲームオーバー中と空気栓範囲内のときは膨らめないのでそれを弾く
        if (!IsBitSet(BalloonBehaviorType.GameOver) && !IsBitSet(BalloonBehaviorType.InAirVentArea))
        {
            BitSet(BalloonBehaviorType.Expands);
        }
    }

    private async UniTask ExpandScaleAnimation()
    {
        if (_isScaleAnimation) return;
        var token = this.GetCancellationTokenOnDestroy();
        float time = 0f;
        float startValue = GetBalloonBlendShapesValue();
        _isScaleAnimation = true;

        while (time < _scaleAnimationDuration)
        {
            await UniTask.Yield(token);

            time += Time.deltaTime;
            float progress = Mathf.Clamp01(time / _scaleAnimationDuration);
            float scaleValue = Mathf.Min(startValue + _scaleOffset * progress, MaxBrandShapeValue);

            ChangeWeight(scaleValue);

            //カメラの視野角を変更
            _cinemachineTargetGroup.m_Targets[0].radius = BlendShapeWeight2CameraRadius(GetBalloonBlendShapesValue());

            //膨らみ途中に膨らめない状態になったら処理終了
            if (!IsBitSet(BalloonBehaviorType.Expands)) break;

            //最大まで膨らんだら処理膨らみアニメーションを終了
            if (Mathf.Approximately(scaleValue, MaxBrandShapeValue)) break;
        }

        _isScaleAnimation = false;
        State = BalloonState.Expands;
    }

    private void BalloonDeflation(float scaleAmountDeflatingPerSecond)
    {
        if (State != BalloonState.Expands) return;
        if (!IsBitSet(BalloonBehaviorType.Deflation)) return;

        float scaleDecrease = scaleAmountDeflatingPerSecond * Time.deltaTime;
        float scaleValue = Mathf.Max(GetBalloonBlendShapesValue() - scaleDecrease, _defaultBlendShapeWeight);
        ChangeWeight(scaleValue);

        //カメラの視野角を変更
        _cinemachineTargetGroup.m_Targets[0].radius = BlendShapeWeight2CameraRadius(GetBalloonBlendShapesValue());

        if (Mathf.Approximately(scaleValue, _defaultBlendShapeWeight))
        {
            State = BalloonState.Normal;
        }
    }

    private void OnWaterStay()
    {
        BalloonDeflation(_scaleAmountDeflatingPerSecondInWater);
    }

    private void ChangeWeight(float weight)
    {
        SetBalloonBlendShapesValue(weight);

        //スペキュラーを変更
        _MAT_AtiiBalloon.SetFloat("_Smoothness", BlendShapeWeight2Smoothness(weight));
    }

    private float BlendShapeWeight2CameraRadius(float blendShapeWeight)
    {
        //radiousの最低値。0~MaxBrandShapeValue を Offset~cameraRadiusMaxに
        //調整するために、最低値をあわせるためのOffset
        const float Offset = 1f;

        //現在の進行度(膨らみ度(0~MaxBrandShapeValue))を変換
        float progress = blendShapeWeight / MaxBrandShapeValue * (_cameraRadiusMax - Offset);

        return progress + Offset;
    }

    private float BlendShapeWeight2Smoothness(float blendShapeWeight)
    {
        //radiousの最低値。0~Max を Offset~Max+Offsetに
        //調整するために、最低値をあわせるためのOffset
        const float Offset = 0.4f;

        //現在の進行度(膨らみ度(0~MaxBrandShapeValue))を変換
        float progress = blendShapeWeight / MaxBrandShapeValue * (_smoothnessMax - Offset);

        return progress + Offset;
    }

    private void BitSet(BalloonBehaviorType addType)
    {
        _behaviorType |= addType;
    }

    private void BitClear(BalloonBehaviorType delType)
    {
        _behaviorType &= ~delType;
    }

    private bool IsBitSet(BalloonBehaviorType type)
    {
        return type == (_behaviorType & type);
    }

    private float GetBalloonBlendShapesValue()
    {
        return _skinnedMeshRenderer.GetBlendShapeWeight(1);
    }

    private void SetBalloonBlendShapesValue(float value)
    {
        _skinnedMeshRenderer.SetBlendShapeWeight(1, value);
    }
}
