using Cinemachine;
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public enum BalloonState
{
    Normal,
    Expands,
    ScaleAnimation,
    BoostDash,
    Disabled,
    GameOver,
}

public class BalloonController : MonoBehaviour
{
    //下記のInputActionReferenceは、Handlerの役割をもちます
    [SerializeField, Required] InputActionReference _ringPushAction = default!;
    [SerializeField, Required] InputActionReference _ringPullAction = default!;

    [SerializeField, Required] WaterEvent _waterEvent = default!;
    [SerializeField, Required] CinemachineTargetGroup _cinemachineTargetGroup = default!;
    [SerializeField, Required] CinemachineController _cinemachineController = default!;
    [SerializeField, Required] AirventEvent _airVentEvent = default!;
    [SerializeField, Required] PlayerGameOverEvent _playerGameOverEvent = default!;
    [SerializeField, Required] Material _MAT_AtiiBalloon = default!;

    [SerializeField, Required] SkinnedMeshRenderer _skinnedMeshRenderer = default!;

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
    [Header("吹っ飛びダッシュの持続時間。PlayerControllerと同じ値を設定してください")]
    [SerializeField, Min(0)] Vector2Int _boostFrames = default!;
    int _boostFrame = 0;

    //風船の膨らみ具合の初期値。Awakeで初期化しています
    float _defaultBlendShapeWeight;

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

    public event Action<BalloonState> OnStateChanged;

    static readonly float MaxBrandShapeValue = 100f;

    private void Awake()
    {
        _defaultBlendShapeWeight = _skinnedMeshRenderer.GetBlendShapeWeight(0);
        _waterEvent.OnStayAction += OnWaterStay;
        _ringPushAction.action.performed += OnRingconPushed;
        _ringPullAction.action.performed += OnRingconPulled;
        _playerGameOverEvent.OnGameOver += OnGameOver;
        _playerGameOverEvent.OnRevive += OnRevive;
        _airVentEvent.OnEnterAirVent += OnEnterAirVent;
        _airVentEvent.OnExitAirVent += OnExitAirVent;

        State = Mathf.Approximately(_defaultBlendShapeWeight, 0f) ? BalloonState.Normal : BalloonState.Expands;
        _boostFrame = _boostFrames.x + (int)((_boostFrames.y - _boostFrames.x) * _skinnedMeshRenderer.GetBlendShapeWeight(0));
    }

    private void OnEnterAirVent()
    {
        State = BalloonState.Disabled;
    }

    private void OnExitAirVent()
    {
        State = BalloonState.Normal;
    }

    private void OnRevive()
    {
        State = BalloonState.Normal;
    }

    private void OnGameOver()
    {
        //風船の空気を抜く
        ChangeScale(_defaultBlendShapeWeight);

        //カメラの視野角を変更
        _cinemachineTargetGroup.m_Targets[0].radius = BlendShapeWeight2CameraRadius(_skinnedMeshRenderer.GetBlendShapeWeight(0));

        State = BalloonState.GameOver;
    }

    private void Update()
    {
        BalloonDeflation(_scaleAmountDeflatingPerSecond);
    }

    private void OnDestroy()
    {
        _waterEvent.OnStayAction -= OnWaterStay;
        _ringPushAction.action.performed -= OnRingconPushed;
        _ringPullAction.action.performed -= OnRingconPulled;
        _playerGameOverEvent.OnGameOver -= OnGameOver;
        _playerGameOverEvent.OnRevive -= OnRevive;
        _airVentEvent.OnEnterAirVent -= OnEnterAirVent;
        _airVentEvent.OnExitAirVent -= OnExitAirVent;
    }

    private void OnRingconPushed(InputAction.CallbackContext obj)
    {
        Expand();
    }

    private void OnRingconPulled(InputAction.CallbackContext obj)
    {
        OnRingconPull();
    }

    private void Expand()
    {
        if (State is not BalloonState.Normal and not BalloonState.Expands) return;

        ExpandScaleAnimation().Forget();
    }

    private async void OnRingconPull()
    {
        if (State != BalloonState.Expands) return;
        var token = this.GetCancellationTokenOnDestroy();

        State = BalloonState.BoostDash;
        
        //処理中に_boostFrameが変更されるおそれがあるため値を保存しておく
        int boostFrame = _boostFrame;

        _cinemachineController.OnAfterBoostDash(boostFrame);

        ChangeScale(_defaultBlendShapeWeight);

        float startValue = _cinemachineTargetGroup.m_Targets[0].radius;
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
    }

    private async UniTask ExpandScaleAnimation()
    {
        if (State == BalloonState.ScaleAnimation) return;
        var token = this.GetCancellationTokenOnDestroy();
        float time = 0f;
        float startValue = _skinnedMeshRenderer.GetBlendShapeWeight(0);
        State = BalloonState.ScaleAnimation;

        while (time < _scaleAnimationDuration)
        {
            await UniTask.Yield(token);

            //膨らみ途中にゲームオーバーになったら処理終了
            if (State == BalloonState.GameOver) return;

            time += Time.deltaTime;
            float progress = Mathf.Clamp01(time / _scaleAnimationDuration);
            float scaleValue = Mathf.Min(startValue + _scaleOffset * progress, MaxBrandShapeValue);

            ChangeScale(scaleValue);

            //カメラの視野角を変更
            _cinemachineTargetGroup.m_Targets[0].radius = BlendShapeWeight2CameraRadius(_skinnedMeshRenderer.GetBlendShapeWeight(0));

            //最大まで膨らんだら処理膨らみアニメーションを終了
            if (Mathf.Approximately(scaleValue, MaxBrandShapeValue)) break;
        }

        State = BalloonState.Expands;
    }

    private void BalloonDeflation(float scaleAmountDeflatingPerSecond)
    {
        if (State != BalloonState.Expands) return;

        float scaleDecrease = scaleAmountDeflatingPerSecond * Time.deltaTime;
        float scaleValue = Mathf.Max(_skinnedMeshRenderer.GetBlendShapeWeight(0) - scaleDecrease, _defaultBlendShapeWeight);
        ChangeScale(scaleValue);

        //カメラの視野角を変更
        _cinemachineTargetGroup.m_Targets[0].radius = BlendShapeWeight2CameraRadius(_skinnedMeshRenderer.GetBlendShapeWeight(0));

        if (Mathf.Approximately(scaleValue, _defaultBlendShapeWeight))
        {
            State = BalloonState.Normal;
        }
    }

    private void OnWaterStay()
    {
        if (State is not BalloonState.Expands and not BalloonState.ScaleAnimation) return;

        BalloonDeflation(_scaleAmountDeflatingPerSecondInWater);
    }

    private void ChangeScale(float newScale)
    {
        _skinnedMeshRenderer.SetBlendShapeWeight(0, newScale);

        float weight = _skinnedMeshRenderer.GetBlendShapeWeight(0);

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
}
