//参考サイト https://am1tanaka.hatenablog.com/entry/beat-calc

using UnityEngine;

public class BeatDetector : MonoBehaviour
{
    public static BeatDetector Instance { get; private set; } = default;

    [SerializeField] float _bpm = 102;

    [SerializeField, Required] AudioSource _bgmAudioSource = default;

    /// <summary>
    /// 現在のサンプル数を返します。
    /// </summary>
    public static int CurrentTimeSamples
    {
        get
        {
            return Instance._bgmAudioSource.timeSamples;
        }
    }

    /// <summary>
    /// FixedUpdateごとのサンプル数
    /// </summary>
    public static float SamplesPerFixedFrame { get; private set; } = default;

    /// <summary>
    /// 現在の曲の周波数
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
    /// 指定のビートの時のサンプル値を返します。
    /// </summary>
    /// <param name="beat"></param>
    /// <returns></returns>
    public float GetSamplesWithBeat(float beat)
    {
        return Frequency * (60f * beat / _bpm);
    }
}