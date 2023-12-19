using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SuccessSceneController : MonoBehaviour
{
    [SerializeField, Required] SuccessSceneView _successSceneView = default!;

    [SerializeField, Required] VideoTransitionController _videoTransitionController = default!;
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
        _successSceneView.Disable();
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

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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

        await _videoTransitionController.StartTransition(_initTransition);

        _enableBackToTitle = true;
    }
}
