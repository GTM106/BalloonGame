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

    //Input系
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

        //BGMの再生
        SoundManager.Instance.PlayBGM(SoundSource.BGM002_Tutorial);

        //チュートリアルUIの表示
        _inputSystemManager.ChangeMaps(InputSystemManager.ActionMaps.UI);
        await _tutorialUIContoller.Popup();

        //数秒は必ず表示する
        await UniTask.Delay(1000, false, PlayerLoopTiming.Update, token);

        //任意キー入力まで待機
        await WaitForAnyKeyWasPressed(token);

        //任意の入力でUIがポップアウト
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
        //制限時間終了SE
        SoundManager.Instance.PlaySE(_endOfTimeLimitAudioSource, SoundSource.SE040_EndOfTimeLimit);

        //入力受付終了

        //ゲーム終了UIの表示

        //一定フレーム待機

        //全サウンドのフェードアウト

        //クリア画面に遷移
    }
}
