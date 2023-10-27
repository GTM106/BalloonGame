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

    private void Awake()
    {
        objectRenderer = GetComponent<Renderer>();

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
        }
        else
        {
            SetMaterialCullMode(CullMode.Back);
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
}
