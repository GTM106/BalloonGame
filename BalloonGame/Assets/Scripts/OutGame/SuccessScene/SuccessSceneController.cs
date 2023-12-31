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

    [Header("開始時のトランジション")]
    [SerializeField] TransitionData _initTransition = default!;

    [Header("タイトル遷移時のトランジション")]
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
        //開始時の初期化
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
        //オーディオリスナーの切り替え
        _playerAudioListener.enabled = false;
        _successSceneAudioListener.enabled = true;

        //BGMの再生
        SoundManager.Instance.PlayBGM(SoundSource.BGM003_Succeed);

        //UI用に切り替え
        _inputSystemManager.ChangeMaps(InputSystemManager.ActionMaps.UI);

        //カメラ切り替え
        _successSceneVirtualCamera.Priority = 30;

        //現在のスコアを取得
        int score = _scoreManager.GetScore();

        //現在のスコアを表示
        _successSceneView.SetScore(score);

        //画面表示
        _successSceneView.Enable();

        await _videoTransitionController.StartTransition(_initTransition);

        _enableBackToTitle = true;
    }
}
