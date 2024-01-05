using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SuccessSceneController : MonoBehaviour
{
    [SerializeField, Required] SuccessSceneView _successSceneView = default!;

    [SerializeField, Required] VideoTransitionController _videoTransitionController = default!;
    [SerializeField, Required] InputSystemManager _inputSystemManager = default!;
    [SerializeField, Required] InputActionReference _ui_RingconPushAction = default!;
    [SerializeField, Required] ScoreManager _scoreManager = default!;
    [SerializeField, Required] CinemachineVirtualCamera _successSceneVirtualCamera = default!;
    [SerializeField, Required] Image _blackScreen = default!;

    [SerializeField, Required] AudioListener _playerAudioListener = default!;
    [SerializeField, Required] AudioListener _successSceneAudioListener = default!;

    [Header("�J�n���̃g�����W�V����")]
    [SerializeField] TransitionData _initTransition = default!;

    [Header("�^�C�g���J�ڎ��̃g�����W�V����")]
    [SerializeField] TransitionData _toTitleTransition = default!;

    bool _enableBackToTitle;

    private void Awake()
    {
        _ui_RingconPushAction.action.performed += BackToTitle;
        _enableBackToTitle = false;
        _blackScreen.enabled = false;
        _successSceneView.Disable();

        _playerAudioListener.enabled = true;
        _successSceneAudioListener.enabled = false;
    }

    private void OnDestroy()
    {
        _ui_RingconPushAction.action.performed -= BackToTitle;
    }

    public void Enable()
    {
        //�J�n���̏�����
        Init();
    }

    private async void BackToTitle(InputAction.CallbackContext obj)
    {
        if (!_enableBackToTitle) return;
        await _videoTransitionController.StartTransition(_toTitleTransition);
        _blackScreen.enabled = true;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private async void Init()
    {
        //�I�[�f�B�I���X�i�[�̐؂�ւ�
        _playerAudioListener.enabled = false;
        _successSceneAudioListener.enabled = true;

        //BGM�̍Đ�
        SoundManager.Instance.PlayBGM(SoundSource.BGM003_Succeed);

        //UI�p�ɐ؂�ւ�
        _inputSystemManager.ChangeMaps(InputSystemManager.ActionMaps.UI);

        //�J�����؂�ւ�
        _successSceneVirtualCamera.Priority = 30;

        //���݂̃X�R�A���擾
        int score = _scoreManager.GetScore();

        //���݂̃X�R�A��\��
        _successSceneView.SetScore(score);

        //��ʕ\��
        _successSceneView.Enable();

        await _videoTransitionController.StartTransition(_initTransition);

        _enableBackToTitle = true;
    }
}
