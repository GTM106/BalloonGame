using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class BalloonController : MonoBehaviour
{
    [SerializeField] float _scaleAnimationDuration = 0.1f;
    [SerializeField] Vector3 _scaleOffset = Vector3.one / 2f;

    bool _isAnimation = false;

    public void Expand()
    {
        ScaleAnimation().Forget();
    }

    private async UniTask ScaleAnimation()
    {
        if (_isAnimation) return;

        var token = this.GetCancellationTokenOnDestroy();
        float time = 0f;
        Vector3 startVec = transform.localScale;

        while (time < _scaleAnimationDuration)
        {
            _isAnimation = true;
            time += Time.deltaTime;
            float progress = Mathf.Clamp01(time / _scaleAnimationDuration);

            Vector3 scale = startVec;
            scale += _scaleOffset * progress;
            transform.localScale = scale;

            await UniTask.Yield(token);
        }

        _isAnimation = false;
    }
}
