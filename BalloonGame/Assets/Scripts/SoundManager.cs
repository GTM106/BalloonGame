using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Rendering;
#endif

public enum SoundSource
{
    //���O��ύX����Ƃ��́A�E�N���b�N���Ė��O�̕ύX���炨�肢���܂�
    //BGM
    BGM1_SAMPLE,

    //SE
    SE1_SAMPLE,//���̍��ڂ�SE�̐擪�Œ�ł��肢���܂��B

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
        if (_clip == null) throw new NullReferenceException("�N���b�v��null�ł��B�ݒ�͊��p����܂����B");

        _volume = volume;
        _pitch = pitch;
    }
}

public class SoundManager : MonoBehaviour
{
    //�G�f�B�^�g�����Ŏg�p�B�V���A���C�Y���邪�A�C���X�y�N�^�ɂ͕\�������Ȃ��B
    [Obsolete("�G�f�B�^�g���ȊO�Ɏg�p���邱�Ƃ͑z�肳��Ă��܂���B")]
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
    /// BGM���Đ�����B
    /// </summary>
    /// <param name="sound">�Đ�������BGM</param>
    /// <param name="time">�Đ��ʒu</param>
    public void PlayBGM(SoundSource sound, float time = 0f)
    {
        ChangeBGM(_BGMLoop, sound);
        _BGMLoop.time = time;
        _BGMLoop.Play();
    }

    /// <summary>
    /// BGM���Đ�����B
    /// </summary>
    /// <param name="intro">�Đ��������C���g��</param>
    /// <param name="loop">�Đ����������[�vBGM</param>
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
            Debug.LogWarning("�ҋ@���Ԃ𕉂̒l�ɂ͂ł��܂���B");
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
            Debug.LogWarning("�ҋ@���Ԃ𕉂̒l�ɂ͂ł��܂���B");
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
    /// SE���Đ����܂��B
    /// </summary>
    /// <param name="sound">�Đ�������SE</param>
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
#pragma warning disable CS0618 // �^�܂��̓����o�[�����^���ł�
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

            //���݂̐ݒ�Ɠ����p�����[�^�[�ɃX���C�_�[��ݒ�
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

            EditorGUILayout.HelpBox("�����ݒ�E����/�s�b�`�ݒ�����L�ōs���܂��B", MessageType.Info);

            //debugSoundSource�ŃN���b�v��I������
            SelectSoundSource();

            EditorGUILayout.Separator();

            //�Y�������̃N���b�v��\���B
            DrawClip();

            EditorGUILayout.Separator();

            TuningParameter();

            EditorGUILayout.Separator();

            //�p�����[�^��������{�^��
            SetParameterButton();

            serializedObject.ApplyModifiedProperties();
        }

        private void TuningParameter()
        {
            //�X���C�_�[�\���B
            EditorGUILayout.LabelField("STEP 3 : ���ʁE�s�b�`�𒲐����܂��B�i���F���̕ύX�͂����Ɋm�肵�܂���j", EditorStyles.boldLabel);

            if (currentAudioClip == null)
            {
                EditorGUILayout.HelpBox("��������ɂ�Clip���A�^�b�`���Ă��������B", MessageType.Warning);
                return;
            }

            //���ʁE�s�b�`����
            VolumeControl();
            PreviewButton();
        }

        private void VolumeControl()
        {
            //�ύX�̊m�F�J�n
            EditorGUI.BeginChangeCheck();

            float volume = VolumeField();
            float pitch = PitchField();

            //�ύX�̊m�F��A�ύX����������s��
            if (EditorGUI.EndChangeCheck())
            {
                previewVolume = volume;
                previewPitch = pitch;

                //�v���r���[���͒��ڒl������������
                if (isPreviewing)
                {
                    previewAudioSource.volume = volume;
                    previewAudioSource.pitch = pitch;
                }
            }
        }

        private void SelectSoundSource()
        {
            EditorGUILayout.LabelField("STEP 1 : �܂��͐ݒ肵����������I�����Ă��������B", EditorStyles.boldLabel);

            if (isPreviewing)
            {
                EditorGUILayout.LabelField("���������ɍĐ�����T�E���h��ύX���邱�Ƃ͂ł��܂���B", EditorStyles.boldLabel);
                return;
            }

            if (HasChanges())
            {
                EditorGUILayout.LabelField("���ύX�_������܂��B�ύX�_���m�肷�邩�A���Z�b�g���Ă��������B", EditorStyles.boldLabel);
                return;
            }

            //�ύX�̊m�F�J�n
            EditorGUI.BeginChangeCheck();

            // SerializeField�����̂����ϐ���\������
            EditorGUILayout.PropertyField(debugSoundSourceProperty);

            //�ύX�̊m�F��A�ύX����������s��
            if (EditorGUI.EndChangeCheck())
            {
                currentSoundSource = debugSoundSourceProperty.GetEnumValue<SoundSource>();
                seClipProperty.arraySize = SoundSource.Max - SoundSource.SE1_SAMPLE;
                bgmClipProperty.arraySize = (int)SoundSource.SE1_SAMPLE;

                SerializedProperty clip = GetClip();

                currentAudioClip = clip.objectReferenceValue as AudioClip;

                //���݂̐ݒ�Ɠ����p�����[�^�[�ɃX���C�_�[��ݒ�
                SliderSetting();

                prebVolume = previewVolume;
                prebPitch = previewPitch;
            }
        }

        private void SliderSetting()
        {
            //�I��
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
                //�ꎞ��~�R�}���h��ǉ�����
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

            EditorGUILayout.LabelField(" ���������Ȃ��璲���\�ł��B");

            EditorGUILayout.EndHorizontal();

            if (isPreviewing)
            {
                if (isPausing)
                {
                    EditorGUILayout.HelpBox("�ꎞ��~���ł��E�E�E", MessageType.Info);
                }
                else
                {
                    if (Mathf.Approximately(previewVolume, 0f))
                    {
                        EditorGUILayout.HelpBox("�{�����[���� 0 �ɐݒ肳��Ă��܂��B", MessageType.Warning);
                    }
                    if (Mathf.Approximately(previewPitch, 0f))
                    {
                        EditorGUILayout.HelpBox("�s�b�`�� 0 �ɐݒ肳��Ă��܂��B\n�s�b�`�� 1 �Œʏ푬�x�ł��B�Đ����x��n�{�ɂȂ�܂��B", MessageType.Warning);
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
                //�󔒖���
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
                //�󔒖���
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
            EditorGUILayout.LabelField("STEP 4 : ���̃{�^���������ĕύX���m�肳���܂��B�i�ύX�����������Ƃ͏o���܂���j", EditorStyles.boldLabel);

            //�N���b�v���Ȃ�������{�^����\�����Ȃ�
            if (currentAudioClip == null)
            {
                EditorGUILayout.HelpBox("Clip�ɉ����A�^�b�`����Ă��܂���B", MessageType.Warning);
                return;
            }

            if (HasChanges())
            {
                EditorGUILayout.HelpBox("�m��O�̕ύX������܂��B�ύX�O�̒l�ɖ߂������ۂ�Reset���N���b�N���Ă��������B", MessageType.Warning);
            }

            GUILayout.BeginHorizontal();

            //�{�^���̕\��
            if (GUILayout.Button("SetParameter", GUILayout.Width(160f)))
            {
                bool isBGM = currentSoundSource < SoundSource.SE1_SAMPLE;
                int index = isBGM ? (int)currentSoundSource : currentSoundSource - SoundSource.SE1_SAMPLE;

                if (isBGM) soundManager.BGMClip[index].SetParameter(previewVolume, previewPitch);
                else soundManager.SEClip[index].SetParameter(previewVolume, previewPitch);

                prebVolume = previewVolume;
                prebPitch = previewPitch;

                hasChanged = false;

                Debug.Log(currentSoundSource + "�̃p�����[�^���m�肵�܂����B");
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

            //�I��
            if (currentSoundSource < SoundSource.SE1_SAMPLE)
            {
                soundManager.ChangeBGM(previewAudioSource, currentSoundSource);
            }
            else
            {
                soundManager.ChangeSE(previewAudioSource, currentSoundSource);
            }

            //�������Z�b�g����Ă��Ȃ�������Đ����Ȃ�
            if (currentAudioClip == null)
            {
                string location = currentSoundSource >= SoundSource.SE1_SAMPLE ? "SE Clip : Element " + (currentSoundSource - SoundSource.SE1_SAMPLE) : "BGM Clip : Element " + (int)currentSoundSource;
                Debug.LogWarning("�������鉹�����Z�b�g�����Ă��܂���B\n" + location + "�ɍĐ��������������Z�b�g���Ă��������B\n");
                StopPreview();
                return;
            }

            //�ݒ肵�ăv���r���[�̊J�n
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
            EditorGUILayout.LabelField("STEP 2 : �������A�^�b�`���܂��B", EditorStyles.boldLabel);

            SerializedProperty clip = GetClip();

            if (isPreviewing)
            {
                EditorGUILayout.LabelField("���������ɍĐ�����T�E���h��ύX���邱�Ƃ͂ł��܂���B   �������̉����F" + (clip.objectReferenceValue != null ? ((AudioClip)clip.objectReferenceValue).name : "N/A"), EditorStyles.boldLabel);
                return;
            }

            if (HasChanges())
            {
                EditorGUILayout.LabelField("���ύX�_������܂��B�ύX�_���m�肷�邩�A���Z�b�g���Ă��������B", EditorStyles.boldLabel);
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
            //�ύX�_�����邩����B��x�ύX�_������Ȃ烊�Z�b�g����邩�m�肳���܂�true
            hasChanged = hasChanged || (!Mathf.Approximately(previewVolume, prebVolume) || !Mathf.Approximately(previewPitch, prebPitch));

            return hasChanged;
        }
    }
#pragma warning restore CS0618 // �^�܂��̓����o�[�����^���ł�
#endif
}