using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemScript : MonoBehaviour, IHittable
{
    [SerializeField] CollectibleScript collectibleScript = default!;
    [SerializeField] Animator _animator = default!;

    [Header("���W�A�C�e���擾���ɏ㏸����l")]
    [SerializeField, Min(0)] int itemValue = default!;

    const double AnimationDuration = 1.06d;

    public async void OnEnter(Collider playerCollider, BalloonState balloonState)
    {
        var token = this.GetCancellationTokenOnDestroy();

        //SE700�Đ�
        collectibleScript.Add(itemValue);

        _animator.SetBool("IsHitPlayer", true);

        await UniTask.Delay(TimeSpan.FromSeconds(AnimationDuration),false, PlayerLoopTiming.FixedUpdate, token);

        gameObject.SetActive(false);
    }
    public void OnExit(Collider playerCollider, BalloonState balloonState)
    {

    }

    public void OnStay(Collider playerCollider, BalloonState balloonState)
    {
    }
}
