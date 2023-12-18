using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using TM.Easing;
using TM.Easing.Management;
using UnityEngine;

public class TrailTimeController : MonoBehaviour
{
    [SerializeField, Required] TrailRenderer _trailRenderer = default!;
    [SerializeField, Required] BoostDashEvent _boostDashEvent = default!;
    [SerializeField, Min(0f)] float _duration = default!;
    [SerializeField, Min(0f)] float _length = default!;
    [SerializeField] EaseType _easeType = EaseType.Linear;

    private void Awake()
    {
        _boostDashEvent.OnBoostDash += OnPlay;

        ChangeTrailRendererTime(0f);
    }

    private void OnDestroy()
    {
        _boostDashEvent.OnBoostDash -= OnPlay;
    }

    private async void OnPlay(BoostDashData obj)
    {
        var token = this.GetCancellationTokenOnDestroy();

        float elapsedTime = 0f;

        while (elapsedTime <= _duration)
        {
            await UniTask.Yield(token);
            elapsedTime += Time.deltaTime;

            float progress = EasingManager.EaseProgress(_easeType, elapsedTime, _duration, 0f, 0f);

            float time = _length * (1f - progress);
            ChangeTrailRendererTime(time);
        }

        ChangeTrailRendererTime(0f);
    }

    private void ChangeTrailRendererTime(float time)
    {
        if (time < 0f) time = 0f;
        _trailRenderer.time = time;
    }
}
