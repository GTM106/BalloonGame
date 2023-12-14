using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;

public class IngameController : MonoBehaviour
{
    [SerializeField, Required] InputSystemManager _inputSystemManager = default!;

    [SerializeField, Required] AudioSource _endOfTimeLimitAudioSource = default!;
    [SerializeField, Required] TutorialUIContoller _tutorialUIContoller = default!;
    [SerializeField, Required] TimeLimitController _timeLimitController = default!;
    [SerializeField, Required] GameFinishController _gameFinishController = default!;
    [SerializeField, Required] ImageTransitionController _imageTransitionController = default!;

    //Input�n
    [SerializeField, Required] InputActionReference _UIAnykeyAction = default;
    [SerializeField, Required] JoyconHandler _JoyconLeftUI = default!;
    [SerializeField, Required] JoyconHandler _JoyconRightUI = default!;

    [SerializeField, Min(0)] int waitingFrameForGameFinish = 50;
    [SerializeField] TrantisionData _trantisionData = default!;

    private void Awake()
    {
        _timeLimitController.OnTimeLimit += OnGameFinish;
    }

    private void Start()
    {
        OnGameStart();
    }

    private void OnDestroy()
    {
        _timeLimitController.OnTimeLimit -= OnGameFinish;
    }

    private async void OnGameStart()
    {
        var token = this.GetCancellationTokenOnDestroy();

        //BGM�̍Đ�
        SoundManager.Instance.PlayBGM(SoundSource.BGM002_Tutorial);

        //�`���[�g���A��UI�̕\��
        _inputSystemManager.ChangeMaps(InputSystemManager.ActionMaps.UI);
        await _tutorialUIContoller.Popup();

        //���b�͕K���\������
        await UniTask.Delay(1000, false, PlayerLoopTiming.Update, token);

        //�C�ӃL�[���͂܂őҋ@
        await WaitForAnyKeyWasPressed(token);

        //�C�ӂ̓��͂�UI���|�b�v�A�E�g
        await _tutorialUIContoller.Popout();

        _inputSystemManager.ChangeMaps(InputSystemManager.ActionMaps.Player);
    }

    private async UniTask WaitForAnyKeyWasPressed(CancellationToken token)
    {
        while (!_UIAnykeyAction.action.WasPressedThisFrame() && !_JoyconLeftUI.WasPressedAnyKeyThisFrame && !_JoyconRightUI.WasPressedAnyKeyThisFrame)
        {
            await UniTask.Yield(token);
        }
    }

    private async void OnGameFinish()
    {
        var token = this.GetCancellationTokenOnDestroy();

        //�������ԏI��SE
        SoundManager.Instance.PlaySE(_endOfTimeLimitAudioSource, SoundSource.SE040_EndOfTimeLimit);

        //���͎�t�I��
        _inputSystemManager.ChangeMaps(InputSystemManager.ActionMaps.UI);

        //�Q�[���I��UI�̕\��
        _gameFinishController.EnableUI();

        //���t���[���ҋ@
        await UniTask.DelayFrame(waitingFrameForGameFinish, PlayerLoopTiming.FixedUpdate, token);

        //�S�T�E���h�̃t�F�[�h�A�E�g
        //�d�ߏ����Ȃ̂ő΍�l����
        var audioSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (var audioSource in audioSources)
        {
            //SE�Ƃ��Ă��邪�S���̃I�[�f�B�I�\�[�X���������Ă��邽��BGM���t�F�[�h�A�E�g�����
            SoundManager.Instance.StopSE(audioSource, 0.5f);
        }

        //�N���A��ʂɑJ��
        _imageTransitionController.StartTransition(_trantisionData);
    }
}
