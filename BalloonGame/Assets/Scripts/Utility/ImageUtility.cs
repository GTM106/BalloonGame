using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public static class ImageUtility
{
    /// <summary>
    /// �t�F�[�h�C��������
    /// </summary>
    /// <param name="duration">����[sec]</param>
    /// <returns></returns>
    public static async UniTask FadeIn(this Image image, float duration, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        await FadeTo(image, 1f, duration, token);
    }

    /// <summary>
    /// �t�F�[�h�A�E�g������
    /// </summary>
    /// <param name="duration">����[sec]</param>
    /// <returns></returns>
    public static async UniTask FadeOut(this Image image, float duration, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        await FadeTo(image, 0f, duration, token);
    }

    /// <summary>
    /// �t���b�V��������
    /// </summary>
    /// <param name="duration">�t���b�V���ɂ����鎞��[sec]</param>
    /// <returns></returns>
    public static async UniTask Flash(this Image image, float duration, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        await image.FadeOut(duration / 2f, token);
        await image.FadeIn(duration / 2f, token);
    }

    // �A���t�@�l��ύX���ăt�F�[�h����������\�b�h
    private static async UniTask FadeTo(Image image, float targetAlpha, float duration, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        if (image == null)
        {
            Debug.LogError("Image�R���|�[�l���g���w�肳��Ă��܂���B");
            return;
        }

        Color color = image.color;
        float startAlpha = color.a;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / duration);
            color.a = alpha;
            image.color = color;
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate, token);
            elapsedTime += Time.deltaTime;
        }

        color.a = targetAlpha;
        image.color = color;
    }
}
