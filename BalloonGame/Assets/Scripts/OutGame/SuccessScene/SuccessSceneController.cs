using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SuccessSceneController : MonoBehaviour
{
    [SerializeField, Required] SuccessSceneView _successSceneView = default!;

    [SerializeField, Required] ImageTransitionController _imageTransitionController = default!;
    [SerializeField, Required] InputSystemManager _inputSystemManager = default!;
    [SerializeField, Required] InputActionReference _ui_RingconPushAction = default!;
    [SerializeField, Required] ScoreManager _scoreManager = default!;

    [Header("�J�n���̃g�����W�V����")]
    [SerializeField] TrantisionData _initTransition = default!;

    [Header("�^�C�g���J�ڎ��̃g�����W�V����")]
    [SerializeField] TrantisionData _toTitleTransition = default!;

    bool _enableBackToTitle;

    private void Awake()
    {
        _ui_RingconPushAction.action.performed += BackToTitle;
        _enableBackToTitle = false;
    }

    public void Enable()
    {
        //�J�n���̏�����
        Init();
    }

    private async void BackToTitle(InputAction.CallbackContext obj)
    {
        if (!_enableBackToTitle) return;
        await _imageTransitionController.StartTransition(_toTitleTransition);
    }

    private async void Init()
    {
        //BGM�̍Đ�
        SoundManager.Instance.PlayBGM(SoundSource.BGM003_Succeed);

        //UI�p�ɐ؂�ւ�
        _inputSystemManager.ChangeMaps(InputSystemManager.ActionMaps.UI);

        //���݂̃X�R�A���擾
        int score = _scoreManager.GetScore();

        //���݂̃X�R�A��\��
        _successSceneView.SetScore(score);

        //��ʕ\��
        _successSceneView.Enable();

        await _imageTransitionController.StartTransition(_initTransition);
    }
}
