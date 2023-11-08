using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

public enum SoundSource
{
    //名前を変更するときは、右クリックして名前の変更からお願いします
    //BGM
    BGM001_Title,
    BGM002_Ingame,
    BGM003_Succeed,
    BGS001_Sand,

    //SE
    SE001_PlayerWalking,//この項目はSEの先頭固定でお願いします。
    SE002_PlayerJumping,
    SE003_PlayerLanding,
    SE004_PlayerBalloonExpands,
    SE005_PlayerBoostDash,
    SE006_PlayerDamaged,
    SE007_PlayerGetsItem,
    SE010_,

    [InspectorName("")]
    Max,
}

[Serializable]
public struct SoundSettings
{
    [SerializeField] AudioClip _clip;
    [SerializeField, Range(0f, 1f)] float _volume;
    [SerializeField, Range(0f, 3f)] float _pitch;

    public AudioClip Clip => _clip;
    public float Volume => _volume;
    public float Pitch => _pitch;

    public void SetParameter(float volume, float pitch)
    {
        if (_clip == null) throw new NullReferenceException("クリップがnullです。設定は棄却されました。");

        _volume = volume;
        _pitch = pitch;
    }
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [SerializeField, Required] AudioSource _BGMLoop;
    [SerializeField, Required] AudioSource _BGMIntro;
    [SerializeField, Required] SoundSettingsData _SoundSettingsData;

    SoundSettings[] _soundDatas;

    private void Awake()
    {
        _soundDatas = _SoundSettingsData.Datas;
        Instance = this;

        PlayBGM(SoundSource.BGM001_Title);
    }

    //音源の切り替えを行う
    private void ChangeSound(AudioSource audioSource, SoundSource sound)
    {
        SoundSettings bgm = _soundDatas[(int)sound];
        audioSource.clip = bgm.Clip;
        audioSource.volume = bgm.Volume;
        audioSource.pitch = bgm.Pitch;
    }

    /// <summary>
    /// BGMを再生する。
    /// </summary>
    /// <param name="sound">再生したいBGM</param>
    /// <param name="time">再生位置</param>
    public void PlayBGM(SoundSource sound, float time = 0f)
    {
        if (_BGMLoop.outputAudioMixerGroup == null)
        {
            Debug.LogWarning(_BGMLoop.name + " の Output を null にすることは非推奨です。");
        }

        ChangeSound(_BGMLoop, sound);
        _BGMLoop.time = time;
        _BGMLoop.Play();
    }

    /// <summary>
    /// BGMを再生する。
    /// </summary>
    /// <param name="intro">再生したいイントロ</param>
    /// <param name="loop">再生したいループBGM</param>
    public void PlayBGM(SoundSource intro, SoundSource loop)
    {
        if (_BGMIntro.outputAudioMixerGroup == null)
        {
            Debug.LogWarning(_BGMIntro.name + " の Output を null にすることは非推奨です。");
        }
        if (_BGMLoop.outputAudioMixerGroup == null)
        {
            Debug.LogWarning(_BGMLoop.name + " の Output を null にすることは非推奨です。");
        }

        ChangeSound(_BGMIntro, intro);
        _BGMIntro.PlayScheduled(AudioSettings.dspTime);
        ChangeSound(_BGMLoop, loop);
        _BGMLoop.PlayScheduled(AudioSettings.dspTime + (_BGMIntro.clip.samples / (float)_BGMIntro.clip.frequency));
    }

    public void PlayBGM(SoundSource sound, float fadeTime, float time)
    {
        if (_BGMLoop.outputAudioMixerGroup == null)
        {
            Debug.LogWarning(_BGMLoop.name + " の Output を null にすることは非推奨です。");
        }

        ChangeSound(_BGMLoop, sound);
        float targetValue = _BGMLoop.volume;

        _BGMLoop.volume = 0f;
        _BGMLoop.time = time;
        _BGMLoop.Play();
        BGMFadein(fadeTime, targetValue).Forget();
    }

    public void StopBGM()
    {
        _BGMIntro.Stop();
        _BGMLoop.Stop();
    }

    public async void StopBGM(float fadeTime)
    {
        await BGMFadeout(fadeTime);

        StopBGM();
    }

    private async UniTask BGMFadein(float duration, float volume)
    {
        var token = this.GetCancellationTokenOnDestroy();

        if (duration <= 0)
        {
            Debug.LogWarning("待機時間を負の値にはできません。");
            return;
        }

        float fadeTime = 0f;

        while (fadeTime < duration)
        {
            await UniTask.Yield(token);
            fadeTime += Time.deltaTime;

            _BGMLoop.volume = Mathf.Min(volume * (fadeTime / duration), volume);
        }

        _BGMLoop.volume = volume;
    }

    private async UniTask BGMFadeout(float duration)
    {
        var token = this.GetCancellationTokenOnDestroy();

        if (duration < 0f)
        {
            Debug.LogWarning("待機時間を負の値にはできません。");
            return;
        }

        float fadeTime = 0f;
        float firstVolume = _BGMLoop.volume;

        while (fadeTime < duration)
        {
            await UniTask.Yield(token);
            fadeTime += Time.deltaTime;

            _BGMLoop.volume = Mathf.Max(firstVolume * (1f - (fadeTime / duration)), 0f);
        }

        _BGMLoop.volume = 0f;
    }

    /// <summary>
    /// SEを再生します。
    /// </summary>
    /// <param name="sound">再生したいSE</param>
    public void PlaySE(AudioSource audioSource, SoundSource sound, float fadeTime = 0f)
    {
        if (audioSource.outputAudioMixerGroup == null)
        {
            Debug.LogWarning(audioSource.name + " の Output を null にすることは非推奨です。");
        }

        ChangeSound(audioSource, sound);
        float targetVolume = audioSource.volume;

        SEFadein(audioSource, fadeTime, targetVolume).Forget();
        audioSource.Play();
    }

    public async void StopSE(AudioSource audioSource, float fadeTime = 0f)
    {
        await SEFadeout(audioSource, fadeTime);
        audioSource.Stop();
    }

    private async UniTask SEFadein(AudioSource source, float duration, float targetVolume)
    {
        var token = this.GetCancellationTokenOnDestroy();
        if (duration <= 0f)
        {
            source.volume = targetVolume;
            return;
        }

        float fadeTime = 0f;

        while (source.volume < targetVolume)
        {
            await UniTask.Yield(token);
            fadeTime += Time.deltaTime;

            source.volume = Mathf.Min(targetVolume * (fadeTime / duration), targetVolume);
        }

        source.volume = targetVolume;
    }

    private async UniTask SEFadeout(AudioSource source, float duration)
    {
        var token = this.GetCancellationTokenOnDestroy();

        if (duration <= 0f) return;

        float fadeTime = 0f;
        float firstVolume = source.volume;

        while (source.volume > 0f)
        {
            await UniTask.Yield(token);
            fadeTime += Time.deltaTime;

            source.volume = Mathf.Max(firstVolume * (1f - (fadeTime / duration)), 0f);
        }
    }
}