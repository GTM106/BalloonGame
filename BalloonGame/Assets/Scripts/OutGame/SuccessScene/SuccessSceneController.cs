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

    [Header("開始時のトランジション")]
    [SerializeField] TrantisionData _initTransition = default!;

    [Header("タイトル遷移時のトランジション")]
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
        //開始時の初期化
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
        //BGMの再生
        SoundManager.Instance.PlayBGM(SoundSource.BGM003_Succeed);

        //UI用に切り替え
        _inputSystemManager.ChangeMaps(InputSystemManager.ActionMaps.UI);

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
