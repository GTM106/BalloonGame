using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FunController : AirVentInteractable, IHittable
{
    //�L���b�V��
    [SerializeField] Transform _transform;
    [SerializeField] Animator _animator;

    [Header("�d���̏������")]
    [SerializeField] bool _isPoweredOn = false;

    [Header("���̋���")]
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

        //�A�j���[�^�[�̃X�s�[�h��d���ɂ���ē����Đ�����~�ɂ���B
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
        //�d����ONOFF��؂�ւ���
        _isPoweredOn = !_isPoweredOn;

        //�A�j���[�^�[�̃X�s�[�h��d���ɂ���ē����Đ�����~�ɂ���B
        if (_animator != null)
        {
            _animator.speed = _isPoweredOn ? 1f : 0f;
        }
    }
}
