using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Rendering;
#endif
using UnityEngine;

[CreateAssetMenu(fileName = "SoundSettingsData", menuName = "ScriptableObject/Sound Settings Data")]
public class SoundSettingsData : ScriptableObject
{    
    //エディタ拡張側で使用。シリアライズするが、インスペクタには表示させない。
    [Obsolete("エディタ拡張以外に使用することは想定されていません。")]
    [SerializeField, HideInInspector] SoundSource debugSoundSource;

    [SerializeField] SoundSettings[] datas;

    public SoundSettings[] Datas => datas;

#if UNITY_EDITOR
    [CustomEditor(typeof(SoundSettingsData))]
    public class SoundSettingsEditor : Editor
    {
        //エディタ拡張の元
        private SoundSettingsData soundSettingsData;

        //プレビュー用
        private AudioSource previewAudioSource;
        private bool isPreviewing;
        private bool isPausing;
        private float previewVolume;
        private float previewPitch;

        //スライダーの定数
        private const float VolumeSliderMin = 0f;
        private const float VolumeSliderMax = 1f;
        private const float PitchSliderMin = 0f;
        private const float PitchSliderMax = 3f;

        //プロパティ
        SerializedProperty debugSoundSourceProperty;
        SerializedProperty soundDataProperty;

        //以前保存してあったボリューム
        float prebVolume;
        float prebPitch;

        //変更点の有無
        bool hasChanged = false;

        SoundSource currentSoundSource;
        AudioClip currentAudioClip;

        private void OnEnable()
        {
            soundSettingsData = (SoundSettingsData)target;

            //エディタ上でAudioSourceを作成
            previewAudioSource = EditorUtility.CreateGameObjectWithHideFlags("Sound Preview", HideFlags.HideAndDontSave, typeof(AudioSource)).GetComponent<AudioSource>();
            previewAudioSource.playOnAwake = false;
            previewAudioSource.loop = true;

            //プロパティの取得
            debugSoundSourceProperty = serializedObject.FindProperty("debugSoundSource");
            soundDataProperty = serializedObject.FindProperty("datas");
            soundDataProperty.arraySize = (int)SoundSource.Max;
            serializedObject.ApplyModifiedProperties(); // プロパティの変更を適用
#pragma warning disable CS0618 // 型またはメンバーが旧型式です
            currentSoundSource = soundSettingsData.debugSoundSource;
#pragma warning restore CS0618 // 型またはメンバーが旧型式です

            //現在のクリップを取得
            SerializedProperty clip = GetClip();
            currentAudioClip = clip.objectReferenceValue as AudioClip;

            //現在の設定と同じパラメーターにスライダーを設定
            SliderSetting();

            //リセット用にボリュームを保存
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
                soundDataProperty.arraySize = (int)SoundSource.Max;
                serializedObject.ApplyModifiedProperties(); // プロパティの変更を適用

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

            //ボタンの表示。変更を確定する
            if (GUILayout.Button("SetParameter", GUILayout.Width(160f)))
            {
                soundSettingsData.datas[(int)currentSoundSource].SetParameter(previewVolume, previewPitch);

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
            ChangeSound(previewAudioSource, currentSoundSource);

            //音源がセットされていなかったら再生しない
            if (currentAudioClip == null)
            {
                string location = currentSoundSource >= SoundSource.SE001_PlayerWalking ? "SE Clip : Element " + (currentSoundSource - SoundSource.SE001_PlayerWalking) : "BGM Clip : Element " + (int)currentSoundSource;
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
            int index = (int)currentSoundSource;

            SerializedProperty clipPropertyElement = soundDataProperty.GetArrayElementAtIndex(index);
            SerializedProperty clip = clipPropertyElement.FindPropertyRelative("_clip");
            return clip;
        }

        private bool HasChanges()
        {
            //変更点があるか判定。一度変更点があるならリセットされるか確定されるまでtrue
            hasChanged = hasChanged || (!Mathf.Approximately(previewVolume, prebVolume) || !Mathf.Approximately(previewPitch, prebPitch));

            return hasChanged;
        }

        //音源の切り替えを行う
        private void ChangeSound(AudioSource audioSource, SoundSource sound)
        {
            SoundSettings bgm = soundSettingsData.datas[(int)sound];
            audioSource.clip = bgm.Clip;
            audioSource.volume = bgm.Volume;
            audioSource.pitch = bgm.Pitch;
        }
    }
#endif

}
