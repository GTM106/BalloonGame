using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BalloonState
{
    Normal,
    Expands
}

public class BalloonController : MonoBehaviour
{
    [SerializeField, Min(0f)] float _scaleAnimationDuration = 0.1f;
    [SerializeField] Vector3 _scaleOffset = Vector3.one / 2f;
    [SerializeField, Min(0f)] float _scaleAmountDeflatingPerSecond;

    bool _isAnimation = false;
    float _defaultScaleValue = 0f;

    BalloonState _state;
    BalloonState State
    {
        get { return _state; }
        set
        {
            OnStateChanged?.Invoke(value);
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
        ScaleAnimation().Forget();
        if (State == BalloonState.Expands) return;
        State = BalloonState.Expands;
    }

    private async UniTask ScaleAnimation()
    {
        if (_isAnimation) return;

        var token = this.GetCancellationTokenOnDestroy();
        float time = 0f;
        Vector3 startVec = transform.localScale;

        _isAnimation = true;

        while (time < _scaleAnimationDuration)
        {
            await UniTask.Yield(token);

            time += Time.deltaTime;
            float progress = Mathf.Clamp01(time / _scaleAnimationDuration);

            Vector3 scale = startVec;
            scale += _scaleOffset * progress;
            transform.localScale = scale;
        }

        _isAnimation = false;
    }

    private void BalloonDeflation()
    {
        if (State == BalloonState.Normal) return;
        //‹ó‹C‚ð“ü‚ê‚Ä‚¢‚éŽž‚ÍŽÀs‚µ‚È‚¢
        if (_isAnimation) return;

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
