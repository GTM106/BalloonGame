using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Water : MonoBehaviour, IHittable
{
    [SerializeField] AudioSource _audioSource = default!;
    [SerializeField] AudioLowPassFilter _audioLowPassFilter = default!;
    [SerializeField] WaterEvent _waterEvent = default!;

    //���x���f�U�C���m��ł��̒l��const�l�ɕς��Ă�������������܂���B
    [Header("�v���C���[�����ɓ������Ƃ���C��R��ύX����l")]
    [SerializeField, Min(0f)] float _dragValueAfterChange = 3f;

    [Header("���ɓ������v���C���[�̕��ʐ������L���l�𒴂�����C�x���g����")]
    [SerializeField, Min(0f)] int _playerSitesInWaterCountMin;

    int _playerSitesInWaterCount = 0;

    public void OnEnter(Collider playerCollider, BalloonState balloonState)
    {
        _playerSitesInWaterCount++;
        PlayEffect();
        PlaySE();
        _audioLowPassFilter.enabled = true;
        
        if (_playerSitesInWaterCount >= _playerSitesInWaterCountMin)
        {
            _waterEvent.OnEnter();
        }
    }

    public void OnExit(Collider playerCollider, BalloonState balloonState)
    {
        //������0�ŌŒ�Ƃ��܂�
        playerCollider.attachedRigidbody.drag = 0f;

        PlayEffect();
        PlaySE();
        _audioLowPassFilter.enabled = false;

        if (_playerSitesInWaterCount >= _playerSitesInWaterCountMin)
        {
            _waterEvent.OnExit();
        }

        _playerSitesInWaterCount--;
    }

    public void OnStay(Collider playerCollider, BalloonState balloonState)
    {
        playerCollider.attachedRigidbody.drag = _dragValueAfterChange;

        if (_playerSitesInWaterCount >= _playerSitesInWaterCountMin)
        {
            _waterEvent.OnStay();
        }
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
