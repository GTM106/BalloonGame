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
    /// フェードインさせる
    /// </summary>
    /// <param name="duration">長さ[sec]</param>
    /// <returns></returns>
    public static async UniTask FadeIn(this Image image, float duration, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        await FadeTo(image, 1f, duration, token);
    }

    /// <summary>
    /// フェードアウトさせる
    /// </summary>
    /// <param name="duration">長さ[sec]</param>
    /// <returns></returns>
    public static async UniTask FadeOut(this Image image, float duration, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        await FadeTo(image, 0f, duration, token);
    }

    /// <summary>
    /// フラッシュさせる
    /// </summary>
    /// <param name="duration">フラッシュにかかる時間[sec]</param>
    /// <returns></returns>
    public static async UniTask Flash(this Image image, float duration, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        await image.FadeOut(duration / 2f, token);
        await image.FadeIn(duration / 2f, token);
    }

    // アルファ値を変更してフェードする内部メソッド
    private static async UniTask FadeTo(Image image, float targetAlpha, float duration, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        if (image == null)
        {
            Debug.LogError("Imageコンポーネントが指定されていません。");
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
