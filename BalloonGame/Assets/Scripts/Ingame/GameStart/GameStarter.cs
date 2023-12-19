using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStarter : MonoBehaviour, IHittable
{
    [SerializeField, Required] TimeLimitController _timeLimitController = default!;
    [SerializeField, Required] GameStartUIController _gameStartUIController = default!;
    [SerializeField, Required] BPMEvent _bpmEvent = default!;
    [SerializeField, Required] CollectibleScript _collectibleScript = default!;

    bool _isStartGame = false;

    private void Reset()
    {
        _timeLimitController = FindAnyObjectByType<TimeLimitController>();
    }

    async void IHittable.OnEnter(Collider playerCollider, BalloonState balloonState)
    {
        await _bpmEvent.WaitForBeat();
        OnStartGame();
    }

    void IHittable.OnExit(Collider playerCollider, BalloonState balloonState)
    {
    }

    void IHittable.OnStay(Collider playerCollider, BalloonState balloonState)
    {
    }

    private async void OnStartGame()
    {
        if (_isStartGame) return;
        _isStartGame = true;

        //ゲーム開始UIの表示
        _collectibleScript.Enable();
        await _gameStartUIController.EnableUI();
        _gameStartUIController.DisableUI();

        if (_timeLimitController != null)
        {
            //制限時間の開始
            _timeLimitController.Play();

            //制限時間UIの表示
            _timeLimitController.EnableUI();
        }
    }
}
