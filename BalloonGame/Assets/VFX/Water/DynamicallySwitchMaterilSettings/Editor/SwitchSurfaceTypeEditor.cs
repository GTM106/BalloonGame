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

            // Volumeの設定
            SetupVolume(switchSurfaceType);

            // シーンビューをリフレッシュ
            EditorUtility.SetDirty(switchSurfaceType);
        }
    }

    private void SetupVolume(SwitchSurfaceType switchSurFaceType)
    {
        // Volumeの設定
        Volume volume = switchSurFaceType.GetComponent<Volume>();

        if (volume == null) return;

        // Volumeプロファイルを探し、存在すれば設定
        VolumeProfile inWaterProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>("Assets/VFX/Water/PostEffect/InWaterProfile.asset");

        if (inWaterProfile != null)
        {
            volume.sharedProfile = inWaterProfile;
        }
        else
        {
            Debug.LogWarning("InWater Volume Profile が見つかりませんでした。空のProfileを使います。");
        }

        volume.isGlobal = false;
    }
}
