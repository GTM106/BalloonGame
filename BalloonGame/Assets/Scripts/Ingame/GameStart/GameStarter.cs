using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStarter : MonoBehaviour, IHittable
{
    [SerializeField, Required] TimeLimitController _timeLimitController = default!;
    [SerializeField, Required] GameStartUIController _gameStartUIController = default!;
    [SerializeField, Required] BeatEvent _beatEvent = default!;

    bool _canStartGame = false;

    private void Awake()
    {
        _beatEvent.OnBeat += OnStartGame;
    }

    private void Reset()
    {
        _timeLimitController = FindAnyObjectByType<TimeLimitController>();
    }

    void IHittable.OnEnter(Collider playerCollider, BalloonState balloonState)
    {
        _canStartGame = true;
    }

    void IHittable.OnExit(Collider playerCollider, BalloonState balloonState)
    {
    }

    void IHittable.OnStay(Collider playerCollider, BalloonState balloonState)
    {
    }

    private async void OnStartGame()
    {
        if (!_canStartGame) return;
        _canStartGame = false;

        //ゲーム開始UIの表示
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
