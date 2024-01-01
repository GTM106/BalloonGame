using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//ItemScript
public class ItemScript : MonoBehaviour, IHittable
{
    [SerializeField, Required] CollectibleScript collectibleScript = default!;
    [SerializeField, Required] AudioSource _itemCollectionAudioSource = default!;
    [SerializeField, Required] Animator _animator = default!;
    [Header("収集アイテム取得時に上昇する値")]
    [SerializeField, Min(0)] int itemValue = default!;

    const double AnimationDuration = 1.06d;

    public async void OnEnter(Collider playerCollider, BalloonState balloonState)
    {
        var token = this.GetCancellationTokenOnDestroy();

        SoundManager.Instance.PlaySE(_itemCollectionAudioSource, SoundSource.SE007_PlayerGetsItem);
        collectibleScript.Add(itemValue);

        _animator.SetBool("IsHitPlayer", true);

        await UniTask.Delay(TimeSpan.FromSeconds(AnimationDuration), false, PlayerLoopTiming.FixedUpdate, token);

        gameObject.SetActive(false);
    }

    public void OnExit(Collider playerCollider, BalloonState balloonState)
    {

    }

    public void OnStay(Collider playerCollider, BalloonState balloonState)
    {
    }
}
