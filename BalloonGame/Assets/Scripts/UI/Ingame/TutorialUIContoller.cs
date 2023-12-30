using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using TM.Easing;
using TM.Easing.Management;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class TutorialUIContoller : MonoBehaviour
{
    [Serializable]
    struct AnimationData
    {
        public EaseType easeType;
        [Min(0f)] public float duration;
        [Min(0f)] public float overshootOrAmplitude;
        [Min(0f)] public float period;
    }

    [SerializeField, Required] Canvas _tutorialCanvas = default!;
    [SerializeField, Required] PlayableDirector _timelime = default!;
    [SerializeField, Required] Transform _tutorialUIParent = default!;
    [SerializeField] AnimationData _popupAnimationData = default!;
    [SerializeField] AnimationData _popoutAnimationData = default!;

    private void Awake()
    {
        _tutorialCanvas.enabled = false;
    }

    public async UniTask Popup()
    {
        _tutorialCanvas.enabled = true;

        await PopupAnimation();

        _timelime.Play();
    }

    private async UniTask PopupAnimation()
    {
        var token = this.GetCancellationTokenOnDestroy();

        float elapsedTime = 0f;

        EaseType easeType = _popupAnimationData.easeType;
        float duration = _popupAnimationData.duration;
        float overshootOrAmplitude = _popupAnimationData.overshootOrAmplitude;
        float period = _popupAnimationData.period;

        while (elapsedTime <= duration)
        {
            await UniTask.Yield(token);

            elapsedTime += Time.deltaTime;

            float progress = EasingManager.EaseProgress(easeType, elapsedTime, duration, overshootOrAmplitude, period);

            _tutorialUIParent.localScale = Vector3.one * Mathf.Max(progress, 0f);
        }
    }

    public async UniTask Popout()
    {
        await PopoutAnimation();
        _tutorialCanvas.enabled = false;
        _timelime.Stop();
    }

    private async UniTask PopoutAnimation()
    {
        var token = this.GetCancellationTokenOnDestroy();

        float elapsedTime = 0f;

        EaseType easeType = _popoutAnimationData.easeType;
        float duration = _popoutAnimationData.duration;
        float overshootOrAmplitude = _popoutAnimationData.overshootOrAmplitude;
        float period = _popoutAnimationData.period;

        while (elapsedTime <= duration)
        {
            await UniTask.Yield(token);

            elapsedTime += Time.deltaTime;

            float progress = EasingManager.EaseProgress(easeType, elapsedTime, duration, overshootOrAmplitude, period);

            _tutorialUIParent.localScale = Vector3.one * Mathf.Max(1f - progress, 0f);
        }
    }
}
