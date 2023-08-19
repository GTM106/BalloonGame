using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : MonoBehaviour, IHittable
{
    [SerializeField] AudioSource _audioSource = default!;
    [SerializeField] AudioLowPassFilter _audioLowPassFilter = default!;

    //レベルデザイン確定でこの値はconst値に変えてもいいかもしれません。
    [Header("プレイヤーが水に入ったとき空気抵抗を追加する値")]
    [SerializeField, Min(0f)] float _addDragValue = 3f;

    public void OnEnter(Collider other)
    {
        other.attachedRigidbody.drag += _addDragValue;

        PlayEffect();
        PlaySE();
        _audioLowPassFilter.enabled = true;
    }

    public void OnExit(Collider other)
    {
        other.attachedRigidbody.drag -= _addDragValue;

        PlayEffect();
        PlaySE();
        _audioLowPassFilter.enabled = false;
    }

    public void OnStay(Collider other)
    {
        //DoNothing
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
