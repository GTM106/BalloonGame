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

    [Header("タイトルBGMのフェードアウト時間[sec]")]
    [SerializeField, Min(0f)] float _titleBGMFadeoutTime = 1f;
    [Header("タイトルUIのフェードアウト時間[sec]")]
    [SerializeField, Min(0f)] float _titleUIFadeoutTime = 1f;
    [Header("ゲーム開始ボタンUIのフラッシュ時間[sec]")]
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

        //セレクトSEの再生
        SoundManager.Instance.PlaySE(_selectSEAudioSorce, SoundSource.SE050_Select);

        //BGMのフェードアウト
        SoundManager.Instance.StopBGM(_titleBGMFadeoutTime);

        //並列待機
        await UniTask.WhenAll(
        //BGMフェードアウトの待機
        UniTask.Delay(TimeSpan.FromSeconds(_titleBGMFadeoutTime), false, PlayerLoopTiming.FixedUpdate, token),
        //タイトルUIのフェードアウト
        _titleUIView.FadeOut(_titleUIFadeoutTime, this.GetCancellationTokenOnDestroy())
        );

        //フラッシュの停止
        _titleUIView.StopFlashAlphaStartButtonImage();

        //固定カメラをインゲームのカメラの位置に移動
        //優先度を最下位にすることで再現
        _titleCamera.enabled = false;

        float blendTime = _cinemachineBlenderSettings.GetBlendForVirtualCameras(_titleCamera.name, _freeLookCamera.name, _cinemachineBrain.m_DefaultBlend).BlendTime;
        await UniTask.Delay(TimeSpan.FromSeconds(blendTime), false, PlayerLoopTiming.FixedUpdate, token);

        Debug.LogWarning("画面のぼかしのフェードアウトが未実装です");

        //インゲーム開始処理を実行する
        _ingameStartEvent.CalledStartEvent();
    }
}
