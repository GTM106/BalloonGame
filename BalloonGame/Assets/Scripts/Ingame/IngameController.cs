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

    //Input�n
    [SerializeField, Required] InputActionReference _UIAnykeyAction = default;
    [SerializeField, Required] JoyconHandler _JoyconLeftUI = default!;
    [SerializeField, Required] JoyconHandler _JoyconRightUI = default!;

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

    private void OnGameFinish()
    {
        //�������ԏI��SE
        SoundManager.Instance.PlaySE(_endOfTimeLimitAudioSource, SoundSource.SE040_EndOfTimeLimit);

        //���͎�t�I��

        //�Q�[���I��UI�̕\��

        //���t���[���ҋ@

        //�S�T�E���h�̃t�F�[�h�A�E�g

        //�N���A��ʂɑJ��
    }
}
