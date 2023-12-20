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

    public async UniTask StartTransition(TransitionData trantisionData)
    {
        var token = this.GetCancellationTokenOnDestroy();
        _canvas.enabled = true;

        bool isTransitionTypeIn = trantisionData.type == TransitionData.TransitionType.In;

        RawImage rawImage = isTransitionTypeIn ? _transitionInImage : _transitionOutImage;
        rawImage.enabled = true;

        VideoPlayer video = isTransitionTypeIn ? _transitionIn : _transitionOut;

        //à»ëOçƒê∂ÇµÇƒécÇ¡ÇƒÇ¢ÇÈèÓïÒÇè¡Ç∑
        video.targetTexture.Release();
        video.Play();

        await UniTask.Delay(TimeSpan.FromSeconds(video.length), false, PlayerLoopTiming.FixedUpdate, token);

        rawImage.enabled = false;
    }
}
