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

        //�Q�[���J�nUI�̕\��
        await _gameStartUIController.EnableUI();
        _gameStartUIController.DisableUI();

        if (_timeLimitController != null)
        {
            //�������Ԃ̊J�n
            _timeLimitController.Play();

            //��������UI�̕\��
            _timeLimitController.EnableUI();
        }
    }
}
