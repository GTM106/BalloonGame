using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleUIView : MonoBehaviour
{
    [SerializeField, Required] CanvasGroup _titleCanvasGroup = default!;

    public async UniTask FadeIn(float duration)
    {
        var token = this.GetCancellationTokenOnDestroy();

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            await UniTask.Yield(token);

            elapsedTime += Time.deltaTime;

            float progress = elapsedTime / duration;

            _titleCanvasGroup.alpha = Mathf.Clamp01(progress);
        }
    }    
    
    public async UniTask FadeOut(float duration)
    {
        var token = this.GetCancellationTokenOnDestroy();

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            await UniTask.Yield(token);

            elapsedTime += Time.deltaTime;

            float progress = elapsedTime / duration;

            _titleCanvasGroup.alpha = Mathf.Clamp01(1f - progress);
        }
    }
}
