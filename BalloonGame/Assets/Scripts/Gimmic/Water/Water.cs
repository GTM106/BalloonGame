using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : MonoBehaviour, IHittable
{
    [SerializeField] AudioSource _audioSource = default!;
    [SerializeField] AudioLowPassFilter _audioLowPassFilter = default!;

    //���x���f�U�C���m��ł��̒l��const�l�ɕς��Ă�������������܂���B
    [Header("�v���C���[�����ɓ������Ƃ���C��R��ǉ�����l")]
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
        //TODO:�G�t�F�N�g�̍Đ�
    }

    private void PlaySE()
    {
        _audioSource.Play();
    }
}
