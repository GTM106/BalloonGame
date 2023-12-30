using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BPMEvent : MonoBehaviour
{
    [Header("再生したい音源のBPMを指定してください。\nintro,transitionが一致している必要があります")]
    [SerializeField] float _beatsPerMinute = 102f;

    float _secondsPerBeat;

    private void Awake()
    {
        CalculateSecondsPerBeat();
    }

    private void CalculateSecondsPerBeat()
    {
        _secondsPerBeat = 60f / _beatsPerMinute;
    }

    public async UniTask WaitForBeat()
    {
        var token = this.GetCancellationTokenOnDestroy();

        float timeSinceStart = Time.time;
        float nextBeatTime = Mathf.Floor(timeSinceStart / _secondsPerBeat) * _secondsPerBeat + _secondsPerBeat;

        //次のビートまで待機
        while (timeSinceStart < nextBeatTime)
        {
            timeSinceStart += Time.deltaTime;

            await UniTask.Yield(token);
        }
    }
}
