using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Renderer))] // Rendererも自動的にアタッチ
[RequireComponent(typeof(Volume))] // Rendererも自動的にアタッチ
public class SwitchSurfaceType : MonoBehaviour
{
    [SerializeField] private CameraPositionNotifier cameraPositionNotifier;
    [SerializeField] private Renderer objectRenderer;
    [SerializeField] private Collider cachedCollider; // コライダーコンポーネントのキャッシュ

    private void Awake()
    {
        if (objectRenderer == null)
        {
            Debug.LogError("Rendererが見つかりませんでした。このオブジェクトにRendererをつけてください");
            return; // 早期リターン
        }

        if (cachedCollider == null)
        {
            Debug.LogError("Colliderが見つかりませんでした。このオブジェクトにColliderをつけてください");
            return;
        }

        // カメラ位置変更イベントを監視
        if (cameraPositionNotifier == null)
        {
            Debug.LogError("CameraPositionNotifierがアタッチされていません。インスペクターからCameraPositionNotifierをアタッチしてください。");
            return; // 早期リターン
        }
        cameraPositionNotifier.CameraPositionChanged += OnCameraPositionChanged;
    }

    private void Reset()
    {
        objectRenderer = GetComponent<Renderer>();
        cachedCollider = GetComponent<Collider>();
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
        if (cachedCollider == null) return false;
        return cachedCollider.bounds.Contains(cameraPosition);
    }

    private void SetMaterialCullMode(CullMode cullMode)
    {
        objectRenderer.material.SetFloat("_Cull", (float)cullMode);
    }
}
