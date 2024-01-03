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

    [Header("ゲーム終了処理系")]
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
        //BGMの再生
        SoundManager.Instance.PlayBGM(SoundSource.BGM002_Tutorial);

        _inputSystemManager.ChangeMaps(InputSystemManager.ActionMaps.Player);

        _tutorialUIContoller.StartTutorial();
    }

    private async void OnGameFinish()
    {
        var token = this.GetCancellationTokenOnDestroy();

        //制限時間終了SE
        SoundManager.Instance.PlaySE(_endOfTimeLimitAudioSource, SoundSource.SE040_EndOfTimeLimit);

        //入力受付終了
        _inputSystemManager.ChangeMaps(InputSystemManager.ActionMaps.UI);

        //ゲーム終了UIの表示
        _gameFinishController.EnableUI();

        //一定フレーム待機
        await UniTask.DelayFrame(waitingFrameForGameFinish, PlayerLoopTiming.FixedUpdate, token);

        //全サウンドのフェードアウト
        //重め処理なので対策考え中
        var audioSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (var audioSource in audioSources)
        {
            //SEとしているが全部のオーディオソースを検索しているためBGMもフェードアウトされる
            SoundManager.Instance.StopSE(audioSource, 0.5f);
        }

        //クリア画面に遷移
        await _videoTransitionController.StartTransition(_toSuccessSceneTransition);

        //各種UIのアウト
        _timeLimitController.DisableUI();
        _tutorialUIContoller.FinishTutorial();
        _collectibleScript.Disable();
        _gameFinishController.DisableUI();
        _gameOverUIController.Disable();

        _successSceneController.Enable();
    }
}
