using Cinemachine;
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TitleUIController : MonoBehaviour
{
    [SerializeField, Required] TitleUIView _titleUIView = default!;

    [SerializeField, Required] InputActionReference _ringconPushUIAction = default!;
    [SerializeField, Required] AudioSource _selectSEAudioSorce = default!;
    [SerializeField, Required] AudioSource _beachBGSAudioSorce = default!;
    [SerializeField, Required] IngameStartEvent _ingameStartEvent = default!;
    [SerializeField, Required] CinemachineVirtualCamera _titleCamera = default!;
    [SerializeField, Required] CinemachineBlenderSettings _cinemachineBlenderSettings = default!;
    [SerializeField, Required] CinemachineFreeLook _freeLookCamera = default!;
    [SerializeField, Required] CinemachineBrain _cinemachineBrain = default!;
    [SerializeField, Required] InputSystemManager _inputSystemManager = default!;

    [Header("�^�C�g��BGM�̃t�F�[�h�A�E�g����[sec]")]
    [SerializeField, Min(0f)] float _titleBGMFadeoutTime = 1f;
    [Header("�^�C�g��UI�̃t�F�[�h�A�E�g����[sec]")]
    [SerializeField, Min(0f)] float _titleUIFadeoutTime = 1f;
    [Header("�Q�[���J�n�{�^��UI�̃t���b�V������[sec]")]
    [SerializeField, Min(0f)] float _gameStartButtonImageFlashTime = 1f;

    bool _isPlayedIngame;

    private void Awake()
    {
        _ringconPushUIAction.action.performed += OnRingconPushed;
        _titleCamera.enabled = true;
        _isPlayedIngame = false;

        _inputSystemManager.ChangeMaps(InputSystemManager.ActionMaps.UI);
        _titleUIView.StartFlashAlphaStartButtonImage(_gameStartButtonImageFlashTime, this.GetCancellationTokenOnDestroy());
    }

    private void Start()
    {
        SoundManager.Instance.PlaySE(_beachBGSAudioSorce, SoundSource.BGS001_Sand);
    }

    private void OnDestroy()
    {
        _ringconPushUIAction.action.performed -= OnRingconPushed;
    }

    private void OnRingconPushed(InputAction.CallbackContext obj)
    {
        GameStart();
    }

    private async void GameStart()
    {
        if (_isPlayedIngame) { return; }
        _isPlayedIngame = true;

        var token = this.GetCancellationTokenOnDestroy();

        //�Z���N�gSE�̍Đ�
        SoundManager.Instance.PlaySE(_selectSEAudioSorce, SoundSource.SE050_Select);

        //BGM�̃t�F�[�h�A�E�g
        SoundManager.Instance.StopBGM(_titleBGMFadeoutTime);

        //����ҋ@
        await UniTask.WhenAll(
        //BGM�t�F�[�h�A�E�g�̑ҋ@
        UniTask.Delay(TimeSpan.FromSeconds(_titleBGMFadeoutTime), false, PlayerLoopTiming.FixedUpdate, token),
        //�^�C�g��UI�̃t�F�[�h�A�E�g
        _titleUIView.FadeOut(_titleUIFadeoutTime, this.GetCancellationTokenOnDestroy())
        );

        //�t���b�V���̒�~
        _titleUIView.StopFlashAlphaStartButtonImage();

        //�Œ�J�������C���Q�[���̃J�����̈ʒu�Ɉړ�
        //�D��x���ŉ��ʂɂ��邱�ƂōČ�
        _titleCamera.enabled = false;

        float blendTime = _cinemachineBlenderSettings.GetBlendForVirtualCameras(_titleCamera.name, _freeLookCamera.name, _cinemachineBrain.m_DefaultBlend).BlendTime;
        await UniTask.Delay(TimeSpan.FromSeconds(blendTime), false, PlayerLoopTiming.FixedUpdate, token);

        Debug.LogWarning("��ʂ̂ڂ����̃t�F�[�h�A�E�g���������ł�");

        //�C���Q�[���J�n���������s����
        _ingameStartEvent.CalledStartEvent();
    }
}
