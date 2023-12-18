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

    [Header("開始時のトランジション")]
    [SerializeField] TrantisionData _initTransition = default!;

    [Header("タイトル遷移時のトランジション")]
    [SerializeField] TrantisionData _toTitleTransition = default!;

    bool _enableBackToTitle;

    private void Awake()
    {
        _ui_RingconPushAction.action.performed += BackToTitle;
        _enableBackToTitle = false;
    }

    public void Enable()
    {
        //開始時の初期化
        Init();
    }

    private async void BackToTitle(InputAction.CallbackContext obj)
    {
        if (!_enableBackToTitle) return;
        await _imageTransitionController.StartTransition(_toTitleTransition);
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

        await _imageTransitionController.StartTransition(_initTransition);
    }
}
