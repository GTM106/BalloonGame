using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGMTrackChanger : MonoBehaviour
{
    [Header("�Đ������������̎w��")]
    [SerializeField] List<SoundSource> _transitionSource; // BGM_A, BGM_B, BGM_C, ...

    [SerializeField, Required] AudioSource _audioSource;

    [Header("�Đ�������������BPM���w�肵�Ă��������B\nintro,transition����v���Ă���K�v������܂�")]
    [SerializeField] float _beatsPerMinute = 102f;

    int _currentTransitionIndex;

    float _secondsPerBeat;

    private void Awake()
    {
        //������
        //�ŏ���Intro�̂���-1�ɂ��Ă���
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

        //�ҋ@���O�ɃC���f�b�N�X���グ��
        _currentTransitionIndex++;

        //�N���b�v����葽���Ăяo���ꂽ�珈�����Ȃ�
        //�擪�ɖ߂������Ȃ炱����ύX���Ă�������
        if (_currentTransitionIndex >= _transitionSource.Count)
        {
            return;
        }

        float timeSinceStart = Time.time;
        float nextBeatTime = Mathf.Floor(timeSinceStart / _secondsPerBeat) * _secondsPerBeat + _secondsPerBeat;
        
        //���̃r�[�g�܂őҋ@
        while (timeSinceStart < nextBeatTime)
        {
            timeSinceStart += Time.deltaTime;

            await UniTask.Yield(token);
        }

        int timeSample = _audioSource.timeSamples;

        //�Đ��������������w��
        SoundSource currentSoundSource = _transitionSource[_currentTransitionIndex];

        //�Đ�
        SoundManager.Instance.PlayBGM(currentSoundSource, timeSample);
    }
}
