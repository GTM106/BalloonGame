using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStarter : MonoBehaviour, IHittable
{
    [SerializeField] TimeLimitController _timeLimitController = default!;

    bool _canStartGame = false;

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

    private void OnStartGame()
    {
        if (!_canStartGame) return;
        _canStartGame = false;

        //制限時間の開始
        _timeLimitController.Play();

        //ゲーム開始UIの表示

        //制限時間UIの表示
    }
}
