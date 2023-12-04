using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[CustomEditor(typeof(SwitchSurfaceType))]
public class SwitchSurfaceTypeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Auto Setup Volume"))
        {
            SwitchSurfaceType switchSurfaceType = (SwitchSurfaceType)target;

            // Volume�̐ݒ�
            SetupVolume(switchSurfaceType);

            // �V�[���r���[�����t���b�V��
            EditorUtility.SetDirty(switchSurfaceType);
        }
    }

    private void SetupVolume(SwitchSurfaceType switchSurFaceType)
    {
        // Volume�̐ݒ�
        Volume volume = switchSurFaceType.GetComponent<Volume>();

        if (volume == null) return;

        // Volume�v���t�@�C����T���A���݂���ΐݒ�
        VolumeProfile inWaterProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>("Assets/VFX/Water/PostEffect/InWaterProfile.asset");

        if (inWaterProfile != null)
        {
            volume.sharedProfile = inWaterProfile;
        }
        else
        {
            Debug.LogWarning("InWater Volume Profile ��������܂���ł����B���Profile���g���܂��B");
        }

        volume.isGlobal = false;
    }
}
