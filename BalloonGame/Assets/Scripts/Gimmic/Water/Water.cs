using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Water : MonoBehaviour, IHittable
{
    [SerializeField] AudioSource _audioSource = default!;
    [SerializeField] AudioLowPassFilter _audioLowPassFilter = default!;
    [SerializeField] WaterEvent _waterEvent = default!;

    //レベルデザイン確定でこの値はconst値に変えてもいいかもしれません。
    [Header("プレイヤーが水に入ったとき空気抵抗を変更する値")]
    [SerializeField, Min(0f)] float _dragValueAfterChange = 3f;

    [Header("水に入ったプレイヤーの部位数が下記数値を超えたらイベント発火")]
    [SerializeField, Min(0f)] int _playerSitesInWaterCountMin;

    int _playerSitesInWaterCount = 0;

    public void OnEnter(Collider playerCollider, BalloonState balloonState)
    {
        _playerSitesInWaterCount++;
        PlayEffect();
        PlaySE();
        _audioLowPassFilter.enabled = true;
        
        if (_playerSitesInWaterCount >= _playerSitesInWaterCountMin)
        {
            _waterEvent.OnEnter();
        }
    }

    public void OnExit(Collider playerCollider, BalloonState balloonState)
    {
        //初期は0で固定とします
        playerCollider.attachedRigidbody.drag = 0f;

        PlayEffect();
        PlaySE();
        _audioLowPassFilter.enabled = false;

        if (_playerSitesInWaterCount >= _playerSitesInWaterCountMin)
        {
            _waterEvent.OnExit();
        }

        _playerSitesInWaterCount--;
    }

    public void OnStay(Collider playerCollider, BalloonState balloonState)
    {
        playerCollider.attachedRigidbody.drag = _dragValueAfterChange;

        if (_playerSitesInWaterCount >= _playerSitesInWaterCountMin)
        {
            _waterEvent.OnStay();
        }
    }

    private void PlayEffect()
    {
        //TODO:エフェクトの再生
    }

    private void PlaySE()
    {
        _audioSource.Play();
    }
}
