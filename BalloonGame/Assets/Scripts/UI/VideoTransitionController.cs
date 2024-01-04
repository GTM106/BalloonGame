using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoTransitionController : MonoBehaviour, ITransition
{
    [SerializeField, Required] Canvas _canvas = default!;
    [SerializeField, Required] PlayableDirector _transitionInTimeline = default!;
    [SerializeField, Required] PlayableDirector _transitionOutTimeline = default!;

    private void Awake()
    {
        _canvas.enabled = true;
    }

    public async UniTask StartTransition(TransitionData trantisionData)
    {
        var token = this.GetCancellationTokenOnDestroy();

        bool isTransitionTypeIn = trantisionData.type == TransitionData.TransitionType.In;

        var timeline = isTransitionTypeIn ? _transitionInTimeline : _transitionOutTimeline;

        timeline.Play();

        //1フレーム分待機せずに先に進みます。
        //Timelineからアクティブを直すために1フレーム余分なフレームを入れているためこうしています。
        await UniTask.Delay(TimeSpan.FromSeconds(timeline.duration - 0.02d), false, PlayerLoopTiming.FixedUpdate, token);
    }
}
