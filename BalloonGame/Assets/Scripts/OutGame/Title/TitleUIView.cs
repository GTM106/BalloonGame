using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class TitleUIView : MonoBehaviour
{
    [SerializeField, Required] CanvasGroup _titleCanvasGroup = default!;
    [SerializeField, Required] Image _titleStartButtonImage = default!;

    bool isFlash = false;
    readonly CancellationTokenSource cancellationTokenSource = new();

    public async UniTask FadeIn(float duration, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            await UniTask.Yield(token);

            elapsedTime += Time.deltaTime;

            float progress = elapsedTime / duration;

            _titleCanvasGroup.alpha = Mathf.Clamp01(progress);
        }
    }

    public async UniTask FadeOut(float duration, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            await UniTask.Yield(token);

            elapsedTime += Time.deltaTime;

            float progress = elapsedTime / duration;

            _titleCanvasGroup.alpha = Mathf.Clamp01(1f - progress);
        }
    }

    public async void StartFlashAlphaStartButtonImage(float flashDuration, CancellationToken token)
    {
        if (isFlash) { return; }
        token.ThrowIfCancellationRequested();
        var source = CancellationTokenSource.CreateLinkedTokenSource(token, cancellationTokenSource.Token);

        isFlash = true;
        while (isFlash)
        {
            try
            {
                await _titleStartButtonImage.Flash(flashDuration, source.Token);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }

    public void StopFlashAlphaStartButtonImage()
    {
        isFlash = false;
        cancellationTokenSource.Cancel();
    }
}
