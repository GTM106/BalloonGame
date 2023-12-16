using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FunController : AirVentInteractable, IHittable
{
    //キャッシュ
    [SerializeField, Required] Transform _transform = default!;
    [SerializeField, Required] Animator _animator = default!;
    [SerializeField, Required] AudioSource _audioSource = default!;
    [SerializeField, Required] FunEvent _funEvent = default!;

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

    private void Start()
    {
        if (_audioSource != null)
        {
            SoundManager.Instance.PlaySE(_audioSource, SoundSource.SE031_FunRunning);
        }
    }

    public void OnEnter(Collider playerCollider, BalloonState balloonState)
    {
        if (!_isPoweredOn) return;
        _funEvent.OnEnter(GetWindVector());
    }

    public void OnStay(Collider playerCollider, BalloonState balloonState)
    {
        if (!_isPoweredOn) return;
        _funEvent.OnStay(GetWindVector());
    }

    public void OnExit(Collider playerCollider, BalloonState balloonState)
    {
        if (!_isPoweredOn) return;
        _funEvent.OnExit(GetWindVector());
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

    private Vector3 GetWindVector()
    {
        return _funPower * _transform.forward;
    }
}
