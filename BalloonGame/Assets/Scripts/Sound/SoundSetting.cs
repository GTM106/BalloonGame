using System;
using UnityEngine;

[Serializable]
//�Q�ƌ^�������̂ŃN���X�ɂ��܂�
public class SoundSettings
{
    [SerializeField] AudioClip _clip;
    [SerializeField, Range(0f, 1f)] float _volume;
    [SerializeField, Range(0f, 3f)] float _pitch;

    public SoundSettings()
    {
        _clip = null;
        _volume = 1f;
        _pitch = 1f;
    }

    public SoundSettings(AudioClip clip, float volume, float pitch)
    {
        _clip = clip;
        _volume = volume;
        _pitch = pitch;
    }

    public AudioClip Clip => _clip;
    public float Volume => _volume;
    public float Pitch => _pitch;

    public void SetParameter(float volume, float pitch)
    {
        if (_clip == null) throw new NullReferenceException("�N���b�v��null�ł��B�ݒ�͊��p����܂����B");

        _volume = volume;
        _pitch = pitch;
    }

    public void SetClip(AudioClip clip)
    {
        _clip = clip;
    }
}
