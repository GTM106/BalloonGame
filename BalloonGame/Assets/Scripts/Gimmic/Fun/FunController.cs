using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FunController : AirVentInteractable, IHittable
{
    //キャッシュ
    [SerializeField] Transform _transform;
    [SerializeField] Animator _animator;

    [Header("電源の初期状態")]
    [SerializeField] bool _isPoweredOn = false;

    [Header("風の強さ")]
    [SerializeField, Min(0f)] float _funPower = default!;

    private void Reset()
    {
        _transform = transform;
        _animator = GetComponentInParent<Animator>();
    }

    private void Awake()
    {
        if (_transform == null)
        {
            _transform = transform;
        }

        //アニメーターのスピードを電源によって等速再生か停止にする。
        if (_animator != null)
        {
            _animator.speed = _isPoweredOn ? 1f : 0f;
        }
    }

    public void OnEnter(Collider playerCollider, BalloonState balloonState)
    {
        //if (!_isPoweredOn) return;
        //DoNothing
    }

    public void OnStay(Collider playerCollider, BalloonState balloonState)
    {
        if (!_isPoweredOn) return;

        playerCollider.attachedRigidbody.AddForce(_funPower * _transform.forward);
    }

    public void OnExit(Collider playerCollider, BalloonState balloonState)
    {
        //if (!_isPoweredOn) return;
        //DoNothing
    }

    public override void Interact()
    {
        //電源のONOFFを切り替える
        _isPoweredOn = !_isPoweredOn;

        //アニメーターのスピードを電源によって等速再生か停止にする。
        if (_animator != null)
        {
            _animator.speed = _isPoweredOn ? 1f : 0f;
        }
    }
}
