using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BPMEvent : MonoBehaviour
{
    [Header("�Đ�������������BPM���w�肵�Ă��������B\nintro,transition����v���Ă���K�v������܂�")]
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

        //���̃r�[�g�܂őҋ@
        while (timeSinceStart < nextBeatTime)
        {
            timeSinceStart += Time.deltaTime;

            await UniTask.Yield(token);
        }
    }
}
