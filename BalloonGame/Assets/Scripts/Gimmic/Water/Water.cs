using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Water : MonoBehaviour, IHittable
{
    [SerializeField] AudioSource _audioSource = default!;
    [SerializeField] List<AudioLowPassFilter> _audioLowPassFilter = default!;
    [SerializeField] WaterEvent _waterEvent = default!;

    //���x���f�U�C���m��ł��̒l��const�l�ɕς��Ă�������������܂���B
    [Header("�v���C���[�����ɓ������Ƃ���C��R��ύX����l")]
    [SerializeField, Min(0f)] float _dragValueAfterChange = 3f;

    [Header("���ɓ������v���C���[�̕��ʐ������L���l�𒴂����琅�ɓ���������")]
    [SerializeField, Min(0f)] int _playerSitesInWaterCountMin = 3;

    int _playerSitesInWaterCount = 0;

    //�v���C���[�̋�C��R�̏����l�B
    float _defaultPlayerDrag = InvalidValue;
    static readonly float InvalidValue = -1f;

    private void Reset()
    {
        _audioLowPassFilter.Clear();
        _audioLowPassFilter.AddRange(FindObjectsByType<AudioLowPassFilter>(FindObjectsSortMode.None));
        _waterEvent = FindAnyObjectByType<WaterEvent>();
    }

    public void OnEnter(Collider playerCollider, BalloonState balloonState)
    {
        if (Mathf.Approximately(_defaultPlayerDrag, InvalidValue))
        {
            _defaultPlayerDrag = playerCollider.attachedRigidbody.drag;
        }

        _playerSitesInWaterCount++;
        PlayEffect();
        PlaySE();

        foreach (var audioLowPassFilter in _audioLowPassFilter)
        {
            audioLowPassFilter.enabled = true;
        }

        if (_playerSitesInWaterCount >= _playerSitesInWaterCountMin)
        {
            _waterEvent.OnEnter();
        }
    }

    public void OnExit(Collider playerCollider, BalloonState balloonState)
    {
        playerCollider.attachedRigidbody.drag = _defaultPlayerDrag;

        PlayEffect();
        PlaySE();
        foreach (var audioLowPassFilter in _audioLowPassFilter)
        {
            audioLowPassFilter.enabled = false;
        }

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
