using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGMTrackChanger : MonoBehaviour
{
    [Header("再生したい音源の指定")]
    [SerializeField] List<SoundSource> _transitionSource = default!; // BGM_A, BGM_B, BGM_C, ...

    [SerializeField, Required] AudioSource _audioSource = default!;
    [SerializeField, Required] BPMEvent _bpmEvent = default!;

    int _currentTransitionIndex;

    private void Awake()
    {
        //初期化
        //最初はIntroのため-1にしておく

        //イントロが無いらしいので0に OTOGIYA
        _currentTransitionIndex = 0;
    }

    public async void PlayNextTrack()
    {
        //待機より前にインデックスを上げる
        _currentTransitionIndex++;

        //クリップ数より多く呼び出されたら処理しない
        //先頭に戻したいならここを変更してください
        if (_currentTransitionIndex >= _transitionSource.Count)
        {
            return;
        }

        //ビートのタイミングまで待機
        await _bpmEvent.WaitForBeat();

        //現在の再生位置を取得
        int timeSample = _audioSource.timeSamples;

        //再生したい音源を指定
        SoundSource currentSoundSource = _transitionSource[_currentTransitionIndex];

        //取得した再生位置から再生
        SoundManager.Instance.PlayBGM(currentSoundSource, timeSample);
    }
}
