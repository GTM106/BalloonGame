using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoTransitionController : MonoBehaviour, ITransition
{
    [SerializeField, Required] Canvas _canvas = default!;
    [SerializeField, Required] RawImage _transitionInImage = default!;
    [SerializeField, Required] RawImage _transitionOutImage = default!;
    [SerializeField, Required] VideoPlayer _transitionIn = default!;
    [SerializeField, Required] VideoPlayer _transitionOut = default!;
    [SerializeField, Required] Image _blackImage = default!;
    
    private void Awake()
    {
        _canvas.enabled = false;
        _transitionInImage.enabled = false;
        _transitionOutImage.enabled = false;
    }

    public bool IsTransitionComplete()
    {
        return _transitionIn.isPlaying || _transitionOut.isPlaying;
    }

    public async UniTask StartTransition(TrantisionData trantisionData)
    {
        var token = this.GetCancellationTokenOnDestroy();
        _canvas.enabled = true;

        bool isTransitionTypeIn = trantisionData.type == TrantisionData.TransitionType.In;

        RawImage rawImage = isTransitionTypeIn ? _transitionInImage : _transitionOutImage;
        rawImage.enabled = true;

        VideoPlayer video = isTransitionTypeIn ? _transitionIn : _transitionOut;

        //以前再生して残っている情報を消す
        video.targetTexture.Release();
        video.Play();

        BlackDisable(isTransitionTypeIn, token);

        await UniTask.Delay(TimeSpan.FromSeconds(video.length), false, PlayerLoopTiming.FixedUpdate, token);

        rawImage.enabled = false;

        if (!isTransitionTypeIn)
        {
            _blackImage.enabled = true;
        }
    }

    private async void BlackDisable(bool isTransitionTypeIn, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        if (isTransitionTypeIn)
        {
            //最初の数フレームが途切れるため、動画背景色で埋める
            await UniTask.DelayFrame(10, PlayerLoopTiming.FixedUpdate, token);
            _blackImage.enabled = false;
        }
    }
}
