using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGMTrackChanger : MonoBehaviour
{
    [Header("�Đ������������̎w��")]
    [SerializeField] List<SoundSource> _transitionSource = default!; // BGM_A, BGM_B, BGM_C, ...

    [SerializeField, Required] AudioSource _audioSource = default!;
    [SerializeField, Required] BPMEvent _bpmEvent = default!;

    int _currentTransitionIndex;

    private void Awake()
    {
        //������
        //�ŏ���Intro�̂���-1�ɂ��Ă���

        //�C���g���������炵���̂�0�� OTOGIYA
        _currentTransitionIndex = 0;
    }

    public async void PlayNextTrack()
    {
        //�ҋ@���O�ɃC���f�b�N�X���グ��
        _currentTransitionIndex++;

        //�N���b�v����葽���Ăяo���ꂽ�珈�����Ȃ�
        //�擪�ɖ߂������Ȃ炱����ύX���Ă�������
        if (_currentTransitionIndex >= _transitionSource.Count)
        {
            return;
        }

        //�r�[�g�̃^�C�~���O�܂őҋ@
        await _bpmEvent.WaitForBeat();

        //���݂̍Đ��ʒu���擾
        int timeSample = _audioSource.timeSamples;

        //�Đ��������������w��
        SoundSource currentSoundSource = _transitionSource[_currentTransitionIndex];

        //�擾�����Đ��ʒu����Đ�
        SoundManager.Instance.PlayBGM(currentSoundSource, timeSample);
    }
}
