using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Rendering;
#endif

public enum SoundSource
{
    //名前を変更するときは、右クリックして名前の変更からお願いします
    //BGM
    BGM1_SAMPLE,

    //SE
    SE1_SAMPLE,//この項目はSEの先頭固定でお願いします。

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
    //エディタ拡張側で使用。シリアライズするが、インスペクタには表示させない。
    [Obsolete("エディタ拡張以外に使用することは想定されていません。")]
    [SerializeField, HideInInspector] SoundSource debugSoundSource;

    public static SoundManager Instance { get; private set; }

    [SerializeField, Required] AudioSource _BGMLoop;
    [SerializeField, Required] AudioSource _BGMIntro;

    [SerializeField, HideInInspector] SoundSettings[] BGMClip;
    [SerializeField, HideInInspector] SoundSettings[] SEClip;

    private void Awake()
    {
        Instance = this;
    }

    private void Reset()
    {
        BGMClip = new SoundSettings[(int)SoundSource.SE1_SAMPLE];
        SEClip = new SoundSettings[(SoundSource.Max - SoundSource.SE1_SAMPLE)];
    }

    private void ChangeBGM(AudioSource audioSource, SoundSource sound)
    {
        int index = (int)sound;
        if (index < 0 || BGMClip.Length <= index) return;

        SoundSettings bgm = BGMClip[index];
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
        ChangeBGM(_BGMLoop, sound);
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
        ChangeBGM(_BGMIntro, intro);
        _BGMIntro.PlayScheduled(AudioSettings.dspTime);
        ChangeBGM(_BGMLoop, loop);
        _BGMLoop.PlayScheduled(AudioSettings.dspTime + (_BGMIntro.clip.samples / (float)_BGMIntro.clip.frequency));
    }

    public void PlayBGM(SoundSource sound, float fadeTime, float time)
    {
        ChangeBGM(_BGMLoop, sound);
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

    private void ChangeSE(AudioSource audioSource, SoundSource sound)
    {
        int index = (int)sound - BGMClip.Length;
        if (index < 0 || SEClip.Length <= index) return;

        SoundSettings se = SEClip[index];
        audioSource.clip = se.Clip;
        audioSource.volume = se.Volume;
        audioSource.pitch = se.Pitch;
    }

    /// <summary>
    /// SEを再生します。
    /// </summary>
    /// <param name="sound">再生したいSE</param>
    public void PlaySE(AudioSource audioSource, SoundSource sound, float fadeTime = 0f)
    {
        ChangeSE(audioSource, sound);
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
        if (duration <= 0)
        {
            source.volume = targetVolume;
            return;
        }

        float fadeTime = 0;

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

#if UNITY_EDITOR
#pragma warning disable CS0618 // 型またはメンバーが旧型式です
    [CustomEditor(typeof(SoundManager))]
    public class SoundManagerEditor : Editor
    {
        private SoundManager soundManager;

        private AudioSource previewAudioSource;
        private bool isPreviewing;
        private bool isPausing;

        private const float VolumeSliderMin = 0f;
        private const float VolumeSliderMax = 1f;
        private const float PitchSliderMin = 0f;
        private const float PitchSliderMax = 3f;

        SerializedProperty debugSoundSourceProperty;
        SerializedProperty seClipProperty;
        SerializedProperty bgmClipProperty;

        private float previewVolume;
        private float previewPitch;

        float prebVolume;
        float prebPitch;

        bool hasChanged = false;

        SoundSource currentSoundSource;
        AudioClip currentAudioClip;

        private void OnEnable()
        {
            soundManager = (SoundManager)target;

            previewAudioSource = EditorUtility.CreateGameObjectWithHideFlags("Sound Preview", HideFlags.HideAndDontSave, typeof(AudioSource)).GetComponent<AudioSource>();
            previewAudioSource.playOnAwake = false;
            previewAudioSource.loop = true;

            debugSoundSourceProperty = serializedObject.FindProperty("debugSoundSource");
            seClipProperty = serializedObject.FindProperty("SEClip");
            bgmClipProperty = serializedObject.FindProperty("BGMClip");

            currentSoundSource = soundManager.debugSoundSource;
            SerializedProperty clip = GetClip();
            currentAudioClip = clip.objectReferenceValue as AudioClip;

            //現在の設定と同じパラメーターにスライダーを設定
            SliderSetting();

            prebVolume = previewVolume;
            prebPitch = previewPitch;
        }

        private void OnDisable()
        {
            DestroyImmediate(previewAudioSource.gameObject);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDefaultInspector();

            GUILayout.Space(10f);

            EditorGUILayout.Separator();

            EditorGUILayout.HelpBox("音源設定・音量/ピッチ設定を下記で行えます。", MessageType.Info);

            //debugSoundSourceでクリップを選択する
            SelectSoundSource();

            EditorGUILayout.Separator();

            //該当音源のクリップを表示。
            DrawClip();

            EditorGUILayout.Separator();

            TuningParameter();

            EditorGUILayout.Separator();

            //パラメータを代入するボタン
            SetParameterButton();

            serializedObject.ApplyModifiedProperties();
        }

        private void TuningParameter()
        {
            //スライダー表示。
            EditorGUILayout.LabelField("STEP 3 : 音量・ピッチを調整します。（注：この変更はすぐに確定しません）", EditorStyles.boldLabel);

            if (currentAudioClip == null)
            {
                EditorGUILayout.HelpBox("調整するにはClipをアタッチしてください。", MessageType.Warning);
                return;
            }

            //音量・ピッチ調整
            VolumeControl();
            PreviewButton();
        }

        private void VolumeControl()
        {
            //変更の確認開始
            EditorGUI.BeginChangeCheck();

            float volume = VolumeField();
            float pitch = PitchField();

            //変更の確認後、変更があったら行う
            if (EditorGUI.EndChangeCheck())
            {
                previewVolume = volume;
                previewPitch = pitch;

                //プレビュー中は直接値を書き換える
                if (isPreviewing)
                {
                    previewAudioSource.volume = volume;
                    previewAudioSource.pitch = pitch;
                }
            }
        }

        private void SelectSoundSource()
        {
            EditorGUILayout.LabelField("STEP 1 : まずは設定したい音源を選択してください。", EditorStyles.boldLabel);

            if (isPreviewing)
            {
                EditorGUILayout.LabelField("※試聴中に再生するサウンドを変更することはできません。", EditorStyles.boldLabel);
                return;
            }

            if (HasChanges())
            {
                EditorGUILayout.LabelField("※変更点があります。変更点を確定するか、リセットしてください。", EditorStyles.boldLabel);
                return;
            }

            //変更の確認開始
            EditorGUI.BeginChangeCheck();

            // SerializeField属性のついた変数を表示する
            EditorGUILayout.PropertyField(debugSoundSourceProperty);

            //変更の確認後、変更があったら行う
            if (EditorGUI.EndChangeCheck())
            {
                currentSoundSource = debugSoundSourceProperty.GetEnumValue<SoundSource>();
                seClipProperty.arraySize = SoundSource.Max - SoundSource.SE1_SAMPLE;
                bgmClipProperty.arraySize = (int)SoundSource.SE1_SAMPLE;

                SerializedProperty clip = GetClip();

                currentAudioClip = clip.objectReferenceValue as AudioClip;

                //現在の設定と同じパラメーターにスライダーを設定
                SliderSetting();

                prebVolume = previewVolume;
                prebPitch = previewPitch;
            }
        }

        private void SliderSetting()
        {
            //選曲
            if (currentSoundSource < SoundSource.SE1_SAMPLE)
            {
                soundManager.ChangeBGM(previewAudioSource, currentSoundSource);
            }
            else
            {
                soundManager.ChangeSE(previewAudioSource, currentSoundSource);
            }

            previewVolume = previewAudioSource.volume;
            previewPitch = previewAudioSource.pitch;
        }

        private void PreviewButton()
        {
            if (currentAudioClip == null) return;

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(isPreviewing ? "Stop" : "Play", GUILayout.Width(80f)))
            {
                if (isPreviewing)
                {
                    StopPreview();
                }
                else
                {
                    StartPreview();
                }
            }

            if (isPreviewing)
            {
                //一時停止コマンドを追加する
                if (GUILayout.Button(isPausing ? "Resume" : "Pause", GUILayout.Width(80f)))
                {
                    if (isPausing)
                    {
                        previewAudioSource.UnPause();
                        isPausing = false;
                    }
                    else
                    {
                        previewAudioSource.Pause();
                        isPausing = true;
                    }
                }
            }

            EditorGUILayout.LabelField(" ←試聴しながら調整可能です。");

            EditorGUILayout.EndHorizontal();

            if (isPreviewing)
            {
                if (isPausing)
                {
                    EditorGUILayout.HelpBox("一時停止中です・・・", MessageType.Info);
                }
                else
                {
                    if (Mathf.Approximately(previewVolume, 0f))
                    {
                        EditorGUILayout.HelpBox("ボリュームが 0 に設定されています。", MessageType.Warning);
                    }
                    if (Mathf.Approximately(previewPitch, 0f))
                    {
                        EditorGUILayout.HelpBox("ピッチが 0 に設定されています。\nピッチは 1 で通常速度です。再生速度がn倍になります。", MessageType.Warning);
                    }
                }
            }
        }

        private float PitchField()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Pitch");
            float pitch = EditorGUILayout.Slider(previewPitch, PitchSliderMin, PitchSliderMax);

            if (currentAudioClip != null)
            {
                //空白埋め
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                {
                    hasChanged = false;
                    previewPitch = prebPitch;
                    pitch = previewPitch;
                }
            }

            EditorGUILayout.EndHorizontal();
            return pitch;
        }

        private float VolumeField()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Volume");
            float volume = EditorGUILayout.Slider(previewVolume, VolumeSliderMin, VolumeSliderMax);

            if (currentAudioClip != null)
            {
                //空白埋め
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                {
                    hasChanged = false;
                    previewVolume = prebVolume;
                    volume = previewVolume;
                }
            }

            EditorGUILayout.EndHorizontal();
            return volume;
        }

        private void SetParameterButton()
        {
            EditorGUILayout.LabelField("STEP 4 : 下のボタンを押して変更を確定させます。（変更を取り消すことは出来ません）", EditorStyles.boldLabel);

            //クリップがなかったらボタンを表示しない
            if (currentAudioClip == null)
            {
                EditorGUILayout.HelpBox("Clipに何もアタッチされていません。", MessageType.Warning);
                return;
            }

            if (HasChanges())
            {
                EditorGUILayout.HelpBox("確定前の変更があります。変更前の値に戻したい際はResetをクリックしてください。", MessageType.Warning);
            }

            GUILayout.BeginHorizontal();

            //ボタンの表示
            if (GUILayout.Button("SetParameter", GUILayout.Width(160f)))
            {
                bool isBGM = currentSoundSource < SoundSource.SE1_SAMPLE;
                int index = isBGM ? (int)currentSoundSource : currentSoundSource - SoundSource.SE1_SAMPLE;

                if (isBGM) soundManager.BGMClip[index].SetParameter(previewVolume, previewPitch);
                else soundManager.SEClip[index].SetParameter(previewVolume, previewPitch);

                prebVolume = previewVolume;
                prebPitch = previewPitch;

                hasChanged = false;

                Debug.Log(currentSoundSource + "のパラメータを確定しました。");
            }

            if (HasChanges())
            {
                if (currentAudioClip != null)
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Reset Changes", GUILayout.Width(100f)))
                    {
                        hasChanged = false;
                        previewVolume = prebVolume;
                        previewAudioSource.volume = previewVolume;
                        previewPitch = prebPitch;
                        previewAudioSource.pitch = previewPitch;
                    }
                }
            }

            GUILayout.EndHorizontal();
        }

        private void StartPreview()
        {
            isPreviewing = true;

            //選曲
            if (currentSoundSource < SoundSource.SE1_SAMPLE)
            {
                soundManager.ChangeBGM(previewAudioSource, currentSoundSource);
            }
            else
            {
                soundManager.ChangeSE(previewAudioSource, currentSoundSource);
            }

            //音源がセットされていなかったら再生しない
            if (currentAudioClip == null)
            {
                string location = currentSoundSource >= SoundSource.SE1_SAMPLE ? "SE Clip : Element " + (currentSoundSource - SoundSource.SE1_SAMPLE) : "BGM Clip : Element " + (int)currentSoundSource;
                Debug.LogWarning("試聴する音源がセットさせていません。\n" + location + "に再生したい音源をセットしてください。\n");
                StopPreview();
                return;
            }

            //設定してプレビューの開始
            previewAudioSource.volume = previewVolume;
            previewAudioSource.pitch = previewPitch;
            previewAudioSource.Play();
        }

        private void StopPreview()
        {
            isPreviewing = false;
            isPausing = false;
            previewAudioSource.Stop();
        }

        private void DrawClip()
        {
            EditorGUILayout.LabelField("STEP 2 : 音源をアタッチします。", EditorStyles.boldLabel);

            SerializedProperty clip = GetClip();

            if (isPreviewing)
            {
                EditorGUILayout.LabelField("※試聴中に再生するサウンドを変更することはできません。   試聴中の音源：" + (clip.objectReferenceValue != null ? ((AudioClip)clip.objectReferenceValue).name : "N/A"), EditorStyles.boldLabel);
                return;
            }

            if (HasChanges())
            {
                EditorGUILayout.LabelField("※変更点があります。変更点を確定するか、リセットしてください。", EditorStyles.boldLabel);
                return;
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(clip);
            if (EditorGUI.EndChangeCheck())
            {
                currentAudioClip = clip.objectReferenceValue as AudioClip;
            }
        }

        private SerializedProperty GetClip()
        {
            bool isBGM = currentSoundSource < SoundSource.SE1_SAMPLE;
            SerializedProperty clipProperty = isBGM ? bgmClipProperty : seClipProperty;
            int index = isBGM ? (int)currentSoundSource : currentSoundSource - SoundSource.SE1_SAMPLE;

            SerializedProperty clipPropertyElement = clipProperty.GetArrayElementAtIndex(index);
            SerializedProperty clip = clipPropertyElement.FindPropertyRelative("_clip");
            return clip;
        }

        private bool HasChanges()
        {
            //変更点があるか判定。一度変更点があるならリセットされるか確定されるまでtrue
            hasChanged = hasChanged || (!Mathf.Approximately(previewVolume, prebVolume) || !Mathf.Approximately(previewPitch, prebPitch));

            return hasChanged;
        }
    }
#pragma warning restore CS0618 // 型またはメンバーが旧型式です
#endif
}