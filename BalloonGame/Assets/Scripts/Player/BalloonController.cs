using Cinemachine;
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BalloonState
{
    Normal,
    Expands,
    ScaleAnimation,
    BoostDash,
    GameOver,
}

public class BalloonController : MonoBehaviour
{
    [SerializeField] WaterEvent _waterEvent = default!;
    [SerializeField] CinemachineTargetGroup _cinemachineTargetGroup = default!;
    [SerializeField] CinemachineController _cinemachineController = default!;
    [SerializeField] PlayerGameOverEvent _playerGameOverEvent = default!;

    [Header("膨張アニメーションの持続時間")]
    [SerializeField, Min(0f)] float _scaleAnimationDuration = 0.1f;
    [Header("どのくらい膨張するか。スケール単位")]
    [SerializeField, Min(0f)] float _scaleOffset = 0.5f;
    [Header("1秒間にどのくらいスケールが縮むか")]
    [SerializeField, Min(0f)] float _scaleAmountDeflatingPerSecond;
    [Header("水に入っているとき1秒間にどのくらいスケールが縮むか")]
    [SerializeField, Min(0f)] float _scaleAmountDeflatingPerSecondInWater;
    [Header("吹っ飛びダッシュの持続時間。PlayerControllerと同じ値を設定してください")]
    [SerializeField, Min(0)] int _boostFrame = default!;

    float _defaultScaleValue;

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

    private void Awake()
    {
        _defaultScaleValue = transform.localScale.x;
        _waterEvent.OnStayAction += OnWaterStay;
        _playerGameOverEvent.OnGameOver += OnGameOver;
        _playerGameOverEvent.OnRevive += OnRevive;
    }

    private void OnRevive()
    {
        State = BalloonState.Normal;
    }

    private void OnGameOver()
    {
        //風船の空気を抜く
        ChangeScale(_defaultScaleValue);
        State = BalloonState.GameOver;
    }

    private void Update()
    {
        BalloonDeflation(_scaleAmountDeflatingPerSecond);
    }

    private void OnDestroy()
    {
        _waterEvent.OnStayAction -= OnWaterStay;
        _playerGameOverEvent.OnGameOver -= OnGameOver;
        _playerGameOverEvent.OnRevive -= OnRevive;
    }

    public void Expand()
    {
        if (State is not BalloonState.Normal and not BalloonState.Expands) return;

        ExpandScaleAnimation().Forget();
    }

    public async void OnRingconPull()
    {
        if (State != BalloonState.Expands) return;
        var token = this.GetCancellationTokenOnDestroy();

        State = BalloonState.BoostDash;

        //この処理だけはChangeScaleでなく直接書き換える。
        transform.localScale = Vector3.one * _defaultScaleValue;

        _cinemachineController.OnAfterBoostDash(_boostFrame);

        int currentFrame = 0;

        while (currentFrame <= _boostFrame)
        {
            await UniTask.Yield(PlayerLoopTiming.FixedUpdate, token);

            float progress = currentFrame / _boostFrame;

            //吹っ飛びダッシュだけは特例でスケールを無視してカメラの視野角を変更
            _cinemachineTargetGroup.m_Targets[0].radius = _defaultScaleValue * progress;

            currentFrame++;
        }

        State = BalloonState.Normal;
    }

    private async UniTask ExpandScaleAnimation()
    {
        if (State == BalloonState.ScaleAnimation) return;
        var token = this.GetCancellationTokenOnDestroy();
        float time = 0f;
        float startValue = transform.localScale.x;

        State = BalloonState.ScaleAnimation;

        while (time < _scaleAnimationDuration)
        {
            //膨らみ途中にゲームオーバーになったら処理終了
            if (State == BalloonState.GameOver) return;

            await UniTask.Yield(token);

            time += Time.deltaTime;
            float progress = Mathf.Clamp01(time / _scaleAnimationDuration);

            float scaleValue = startValue + _scaleOffset * progress;
            ChangeScale(scaleValue);
        }

        State = BalloonState.Expands;
    }

    private void BalloonDeflation(float scaleAmountDeflatingPerSecond)
    {
        if (State != BalloonState.Expands) return;

        float scaleDecrease = scaleAmountDeflatingPerSecond * Time.deltaTime;
        float scaleValue = Mathf.Max(transform.localScale.x - scaleDecrease, _defaultScaleValue);
        ChangeScale(scaleValue);

        if (Mathf.Approximately(scaleValue, _defaultScaleValue))
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
        //カメラの視野角を変更
        _cinemachineTargetGroup.m_Targets[0].radius = newScale;
        transform.localScale = Vector3.one * newScale;
    }
}
