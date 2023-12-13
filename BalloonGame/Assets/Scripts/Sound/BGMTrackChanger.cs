using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGMTrackChanger : MonoBehaviour
{
    [Header("再生したい音源の指定")]
    [SerializeField] List<SoundSource> _transitionSource; // BGM_A, BGM_B, BGM_C, ...

    [SerializeField, Required] AudioSource _audioSource;

    [Header("再生したい音源のBPMを指定してください。\nintro,transitionが一致している必要があります")]
    [SerializeField] float _beatsPerMinute = 102f;

    int _currentTransitionIndex;

    float _secondsPerBeat;

    private void Awake()
    {
        //初期化
        //最初はIntroのため-1にしておく
        _currentTransitionIndex = -1;
        CalculateSecondsPerBeat();
    }

    private void CalculateSecondsPerBeat()
    {
        _secondsPerBeat = 60f / _beatsPerMinute;
    }

    public async void PlayNextTrack()
    {
        var token = this.GetCancellationTokenOnDestroy();

        //待機より前にインデックスを上げる
        _currentTransitionIndex++;

        //クリップ数より多く呼び出されたら処理しない
        //先頭に戻したいならここを変更してください
        if (_currentTransitionIndex >= _transitionSource.Count)
        {
            return;
        }

        float timeSinceStart = Time.time;
        float nextBeatTime = Mathf.Floor(timeSinceStart / _secondsPerBeat) * _secondsPerBeat + _secondsPerBeat;
        
        //次のビートまで待機
        while (timeSinceStart < nextBeatTime)
        {
            timeSinceStart += Time.deltaTime;

            await UniTask.Yield(token);
        }

        int timeSample = _audioSource.timeSamples;

        //再生したい音源を指定
        SoundSource currentSoundSource = _transitionSource[_currentTransitionIndex];

        //再生
        SoundManager.Instance.PlayBGM(currentSoundSource, timeSample);
    }
}
