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
}

public class BalloonController : MonoBehaviour
{
    [Header("�c���A�j���[�V�����̎�������")]
    [SerializeField, Min(0f)] float _scaleAnimationDuration = 0.1f;
    [Header("�ǂ̂��炢�c�����邩�B�X�P�[���P��")]
    [SerializeField, Min(0f)] float _scaleOffset = 0.5f;
    [Header("1�b�Ԃɂǂ̂��炢�X�P�[�����k�ނ�")]
    [SerializeField, Min(0f)] float _scaleAmountDeflatingPerSecond;
    [Header("������у_�b�V���̎������ԁBPlayerController�Ɠ����l��ݒ肵�Ă�������")]
    [SerializeField, Min(0)] int _boostFlame = default!;

    float _defaultScaleValue;

    BalloonState _state;
    BalloonState State
    {
        get { return _state; }
        set
        {
            if (_state != value) OnStateChanged?.Invoke(value);
            _state = value;
        }
    }

    public event Action<BalloonState> OnStateChanged;

    private void Awake()
    {
        _defaultScaleValue = transform.localScale.x;
    }

    private void Update()
    {
        BalloonDeflation();
    }

    public void Expand()
    {
        if (State is not BalloonState.Normal and not BalloonState.Expands) return;

        ScaleAnimation().Forget();
    }

    public async void OnRingconPull()
    {
        if (State != BalloonState.Expands) return;

        State = BalloonState.BoostDash;
        transform.localScale = Vector3.one * _defaultScaleValue;

        await UniTask.DelayFrame(_boostFlame, PlayerLoopTiming.FixedUpdate);

        State = BalloonState.Normal;
    }

    private async UniTask ScaleAnimation()
    {
        if (State == BalloonState.ScaleAnimation) return;

        var token = this.GetCancellationTokenOnDestroy();
        float time = 0f;
        Vector3 startVec = transform.localScale;

        State = BalloonState.ScaleAnimation;

        while (time < _scaleAnimationDuration)
        {
            await UniTask.Yield(token);

            time += Time.deltaTime;
            float progress = Mathf.Clamp01(time / _scaleAnimationDuration);

            Vector3 scale = startVec;
            scale += Vector3.one * _scaleOffset * progress;
            transform.localScale = scale;
        }

        State = BalloonState.Expands;
    }

    private void BalloonDeflation()
    {
        if (State != BalloonState.Expands) return;

        float scaleDecrease = _scaleAmountDeflatingPerSecond * Time.deltaTime;
        Vector3 scale = transform.localScale;
        float scaleValue = Mathf.Max(transform.localScale.x - scaleDecrease, _defaultScaleValue);
        scale.Set(scaleValue, scaleValue, scaleValue);
        transform.localScale = scale;

        if (Mathf.Approximately(scaleValue, _defaultScaleValue))
        {
            State = BalloonState.Normal;
        }
    }
}
