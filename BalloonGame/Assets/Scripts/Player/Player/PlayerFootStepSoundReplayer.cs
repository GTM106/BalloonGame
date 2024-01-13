using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFootStepSoundReplayer : MonoBehaviour
{
    [SerializeField, Required] AudioSource _footStepAudioSource = default!;

    [SerializeField, Range(0f, 3f)] float _pitchMin = 0.5f;
    [SerializeField, Range(0f, 3f)] float _pitchMax = 1.5f;

    private void Awake()
    {
        if (_pitchMin > _pitchMax)
        {
            (_pitchMin, _pitchMax) = (_pitchMax, _pitchMin);
        }
    }

    public void PlaySE()
    {
        SoundManager.Instance.PlaySE(_footStepAudioSource, SoundSource.SE001_PlayerWalking, 0.0f);

        //ピッチを強制的に変更する
        _footStepAudioSource.pitch = Random.Range(0.5f, 1.5f);
    }
}
