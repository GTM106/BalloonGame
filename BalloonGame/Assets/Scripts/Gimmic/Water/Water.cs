using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : MonoBehaviour, IHittable
{
    [SerializeField] AudioSource _audioSource = default!;
    [SerializeField] AudioLowPassFilter _audioLowPassFilter = default!;
    [SerializeField] WaterEvent _waterEvent = default!;

    //���x���f�U�C���m��ł��̒l��const�l�ɕς��Ă�������������܂���B
    [Header("�v���C���[�����ɓ������Ƃ���C��R��ǉ�����l")]
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
        //TODO:�G�t�F�N�g�̍Đ�
    }

    private void PlaySE()
    {
        _audioSource.Play();
    }
}
