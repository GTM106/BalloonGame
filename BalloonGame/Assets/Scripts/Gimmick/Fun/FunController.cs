using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FunController : AirVentInteractable, IHittable
{
    //�L���b�V��
    [SerializeField, Required] Transform _transform = default!;
    [SerializeField, Required] Animator _animator = default!;
    [SerializeField, Required] AudioSource _audioSource = default!;
    [SerializeField, Required] FunEvent _funEvent = default!;

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
        //�d����ONOFF��؂�ւ���
        _isPoweredOn = !_isPoweredOn;

        //�A�j���[�^�[�̃X�s�[�h��d���ɂ���ē����Đ�����~�ɂ���B
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
