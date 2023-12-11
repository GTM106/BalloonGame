//�Q�l�T�C�g https://am1tanaka.hatenablog.com/entry/beat-calc

using UnityEngine;

public class BeatDetector : MonoBehaviour
{
    public static BeatDetector Instance { get; private set; } = default;

    [SerializeField] float _bpm = 102;

    [SerializeField, Required] AudioSource _bgmAudioSource = default;

    /// <summary>
    /// ���݂̃T���v������Ԃ��܂��B
    /// </summary>
    public static int CurrentTimeSamples
    {
        get
        {
            return Instance._bgmAudioSource.timeSamples;
        }
    }

    /// <summary>
    /// FixedUpdate���Ƃ̃T���v����
    /// </summary>
    public static float SamplesPerFixedFrame { get; private set; } = default;

    /// <summary>
    /// ���݂̋Ȃ̎��g��
    /// </summary>
    public static float Frequency
    {
        get
        {
            if (Instance._bgmAudioSource.clip == null) return 0f;
            return Instance._bgmAudioSource.clip.frequency;
        }
    }
    private void Awake()
    {
        Instance = this;
        SamplesPerFixedFrame = Frequency * Time.fixedDeltaTime;
    }

    /// <summary>
    /// �w��̃r�[�g�̎��̃T���v���l��Ԃ��܂��B
    /// </summary>
    /// <param name="beat"></param>
    /// <returns></returns>
    public float GetSamplesWithBeat(float beat)
    {
        return Frequency * (60f * beat / _bpm);
    }
}