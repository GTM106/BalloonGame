using Cysharp.Threading.Tasks.Triggers;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Renderer))] // Rendererも自動的にアタッチ
[RequireComponent(typeof(Volume))] // Rendererも自動的にアタッチ
public class SwitchSurfaceType : MonoBehaviour
{
    private Renderer objectRenderer;
    private Color inWaterColor;
    private Color defaultWaterColor;

    private void Awake()
    {
        objectRenderer = GetComponent<Renderer>();
        defaultWaterColor = objectRenderer.material.GetColor("_DefaultColor");
        inWaterColor = objectRenderer.material.GetColor("_InWaterColor"); ;

        if (inWaterColor == null)
        {
            Debug.LogError("_DefaultColorが見つかりませんでした。このオブジェクトのマテリアルに_DefaultColorがあるか確認してください。");
            return; // 早期リターン
        }

        if (defaultWaterColor == null)
        {
            Debug.LogError("_InWaterColorが見つかりませんでした。このオブジェクトのマテリアルに_InWaterColorがあるか確認してください。");
            return; // 早期リターン
        }

        if (objectRenderer == null)
        {
            Debug.LogError("Rendererが見つかりませんでした。このオブジェクトにRendererをつけてください");
            return; // 早期リターン
        }

        // カメラ位置変更イベントを監視
        CameraPositionNotifier notifier = FindObjectOfType<CameraPositionNotifier>();
        if (notifier == null)
        {
            Debug.LogError("CameraPositionNotifierがシーン上にありません。どこでもいいので追加してください。");
            return; // 早期リターン
        }

        notifier.CameraPositionChanged += OnCameraPositionChanged;
    }

    private void OnCameraPositionChanged(object sender, CameraPositionChangedEventArgs e)
    {
        Vector3 cameraPosition = e.CameraPosition;

        if (IsInsideCollider(cameraPosition))
        {
            SetMaterialCullMode(CullMode.Front);
            SetMaterialColor(inWaterColor);
        }
        else
        {
            SetMaterialCullMode(CullMode.Back);
            SetMaterialColor(defaultWaterColor);
        }
    }

    private bool IsInsideCollider(Vector3 cameraPosition)
    {
        Collider col = GetComponent<Collider>();
        return col != null && col.bounds.Contains(cameraPosition);
    }

    private void SetMaterialCullMode(CullMode cullMode)
    {
        objectRenderer.material.SetFloat("_Cull", (float)cullMode);
    }

    private void SetMaterialColor(Color color)
    {
        if (objectRenderer.material.GetColor("_DefaultColor") == color) return;
        objectRenderer.material.SetColor("_DefaultColor", color);
    }
}
