using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : MonoBehaviour, IHittable
{
    [SerializeField] AudioSource _audioSource = default!;
    [SerializeField] AudioLowPassFilter _audioLowPassFilter = default!;
    [SerializeField] WaterEvent _waterEvent = default!;

    //レベルデザイン確定でこの値はconst値に変えてもいいかもしれません。
    [Header("プレイヤーが水に入ったとき空気抵抗を追加する値")]
    [SerializeField, Min(0f)] float _addDragValue = 3f;

    public void OnEnter(Collider playerCollider, BalloonState balloonState)
    {
        playerCollider.attachedRigidbody.drag += _addDragValue;

        PlayEffect();
        PlaySE();
        _audioLowPassFilter.enabled = true;
        _waterEvent.OnEnter();
    }

    public void OnExit(Collider playerCollider, BalloonState balloonState)
    {
        playerCollider.attachedRigidbody.drag -= _addDragValue;

        PlayEffect();
        PlaySE();
        _audioLowPassFilter.enabled = false;
        _waterEvent.OnExit();
    }

    public void OnStay(Collider playerCollider, BalloonState balloonState)
    {
        _waterEvent.OnStay();
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
