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
    [SerializeField, Required] VideoTransitionController _videoTransitionController = default!;
    [SerializeField, Required] IngameStartEvent _ingameStartEvent = default!;
    [SerializeField, Required] CollectibleScript _collectibleScript = default!;
    [SerializeField, Required] GameOverUIController _gameOverUIController = default!;

    [Header("�Q�[���I�������n")]
    [SerializeField, Required] SuccessSceneController _successSceneController = default!;
    [SerializeField, Min(0)] int waitingFrameForGameFinish = 50;
    [SerializeField] TransitionData _toSuccessSceneTransition = default!;

    private void Awake()
    {
        _timeLimitController.OnTimeLimit += OnGameFinish;
        _ingameStartEvent.OnStart += OnGameStart;
    }

    private void OnDestroy()
    {
        _timeLimitController.OnTimeLimit -= OnGameFinish;
    }

    private void OnGameStart()
    {
        //BGM�̍Đ�
        SoundManager.Instance.PlayBGM(SoundSource.BGM002_Tutorial);

        _inputSystemManager.ChangeMaps(InputSystemManager.ActionMaps.Player);

        _tutorialUIContoller.StartTutorial();
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
        await _videoTransitionController.StartTransition(_toSuccessSceneTransition);

        //�e��UI�̃A�E�g
        _timeLimitController.DisableUI();
        _tutorialUIContoller.FinishTutorial();
        _collectibleScript.Disable();
        _gameFinishController.DisableUI();
        _gameOverUIController.Disable();

        _successSceneController.Enable();
    }
}
