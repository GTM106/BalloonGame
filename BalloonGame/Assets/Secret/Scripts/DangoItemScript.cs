using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//DangoItemScript
public class DangoItemScript : MonoBehaviour, IHittable
{
    [SerializeField, Required] CollectibleScript collectibleScript = default!;
    [SerializeField, Required] AudioSource _itemCollectionAudioSource = default!;
    [SerializeField, Required] Animator _animator = default!;
    [Header("収集アイテム取得時に上昇する値")]
    [SerializeField, Min(0)] int itemValue = default!;

    const double AnimationDuration = 4.06d;
    private bool _alreadyhit = false;

    public async void OnEnter(Collider playerCollider, BalloonState balloonState)
    {
        if(_alreadyhit) return;
        _alreadyhit = true;

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
