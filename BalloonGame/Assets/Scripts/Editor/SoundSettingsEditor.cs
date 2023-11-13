using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SoundSettingsData))]
public class SoundSettingsEditor : Editor
{
    //�G�f�B�^�g���̌�
    private SoundSettingsData soundSettingsData;

    //�v���r���[�p
    private AudioSource previewAudioSource;
    private bool isPreviewing;
    private bool isPausing;
    private float previewVolume;
    private float previewPitch;

    //�X���C�_�[�̒萔
    private const float VolumeSliderMin = 0f;
    private const float VolumeSliderMax = 1f;
    private const float PitchSliderMin = 0f;
    private const float PitchSliderMax = 3f;

    //�ȑO�ۑ����Ă������{�����[��
    float prebVolume;
    float prebPitch;

    //�ύX�_�̗L��
    bool hasChanged = false;

    SoundSource currentSoundSource;
    AudioClip currentAudioClip;

    private void OnEnable()
    {
        soundSettingsData = (SoundSettingsData)target;

        //���߂ɕύX�_�̊m�F
        soundSettingsData.Datas.CheckSourceChanges();

        //�G�f�B�^���AudioSource���쐬
        previewAudioSource = EditorUtility.CreateGameObjectWithHideFlags("Sound Preview", HideFlags.HideAndDontSave, typeof(AudioSource)).GetComponent<AudioSource>();
        previewAudioSource.playOnAwake = false;
        previewAudioSource.loop = true;

        //���݂̃N���b�v���擾
        AudioClip clip = GetClip();
        currentAudioClip = clip;

        //���݂̐ݒ�Ɠ����p�����[�^�[�ɃX���C�_�[��ݒ�
        SliderSetting();

        //���Z�b�g�p�Ƀ{�����[����ۑ�
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

        currentSoundSource = (SoundSource)EditorGUILayout.EnumPopup("Select SoundSource", currentSoundSource);

        //�ύX�̊m�F��A�ύX����������s��
        if (EditorGUI.EndChangeCheck())
        {
            AudioClip clip = GetClip();

            currentAudioClip = clip;

            //���݂̐ݒ�Ɠ����p�����[�^�[�ɃX���C�_�[��ݒ�
            SliderSetting();

            prebVolume = previewVolume;
            prebPitch = previewPitch;
        }
    }

    private void SliderSetting()
    {
        //�I��
        ChangeSound(previewAudioSource, currentSoundSource);

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

        //�{�^���̕\���B�ύX���m�肷��
        if (GUILayout.Button("SetParameter", GUILayout.Width(160f)))
        {
            soundSettingsData.Datas.TryGetValue(currentSoundSource, out SoundSettings soundSettings);
            soundSettings.SetParameter(previewVolume, previewPitch);
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
        //�������Z�b�g����Ă��Ȃ�������Đ����Ȃ�
        if (currentAudioClip == null)
        {
            string location = currentSoundSource.ToString();
            Debug.LogWarning("�������鉹�����Z�b�g�����Ă��܂���B\n" + location + "�ɍĐ��������������Z�b�g���Ă��������B\n");
            StopPreview();
            return;
        }

        //�I��
        ChangeSound(previewAudioSource, currentSoundSource);

        //�ݒ肵�ăv���r���[�̊J�n
        previewAudioSource.volume = previewVolume;
        previewAudioSource.pitch = previewPitch;
        previewAudioSource.Play();

        isPreviewing = true;
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

        if (isPreviewing)
        {
            EditorGUILayout.LabelField("���������ɍĐ�����T�E���h��ύX���邱�Ƃ͂ł��܂���B   �������̉����F" + (currentAudioClip != null ? currentAudioClip.name : "N/A"), EditorStyles.boldLabel);
            return;
        }

        if (HasChanges())
        {
            EditorGUILayout.LabelField("���ύX�_������܂��B�ύX�_���m�肷�邩�A���Z�b�g���Ă��������B", EditorStyles.boldLabel);
            return;
        }

        EditorGUI.BeginChangeCheck();
        currentAudioClip = (AudioClip)EditorGUILayout.ObjectField("Clip", currentAudioClip, typeof(AudioClip), false);
        if (EditorGUI.EndChangeCheck())
        {
            //�N���b�v�𔽉f
            if (soundSettingsData.Datas.TryGetValue(currentSoundSource, out SoundSettings soundSettings))
            {
                soundSettings.SetClip(currentAudioClip);
            }
        }
    }

    private AudioClip GetClip()
    {
        if (soundSettingsData.Datas.TryGetValue(currentSoundSource, out SoundSettings soundSettings))
        {
            return soundSettings.Clip;
        }

        return null;
    }

    private bool HasChanges()
    {
        //�ύX�_�����邩����B��x�ύX�_������Ȃ烊�Z�b�g����邩�m�肳���܂�true
        hasChanged = hasChanged || (!Mathf.Approximately(previewVolume, prebVolume) || !Mathf.Approximately(previewPitch, prebPitch));

        return hasChanged;
    }

    //�����̐؂�ւ����s��
    private void ChangeSound(AudioSource audioSource, SoundSource sound)
    {
        if (!soundSettingsData.Datas.TryGetValue(sound, out SoundSettings bgm))
        {
            soundSettingsData.Datas.Add(new(sound, new()));
        }

        audioSource.clip = bgm.Clip;
        audioSource.volume = bgm.Volume;
        audioSource.pitch = bgm.Pitch;
    }
}