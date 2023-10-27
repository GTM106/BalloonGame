using System;
using UnityEngine;

public class CameraPositionNotifier : MonoBehaviour
{
    // アクションの宣言
    public event Action<CameraPositionNotifier, CameraPositionChangedEventArgs> CameraPositionChanged;

    private Vector3 lastCameraPosition;
    //メインカメラのキャッシュ
    private Transform cameraTransform;

    private void Awake()
    {
        cameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        Vector3 currentCameraPosition = cameraTransform.position;
        if (currentCameraPosition != lastCameraPosition)
        {
            lastCameraPosition = currentCameraPosition;

            // カメラ位置が変更されたことを通知
            OnCameraPositionChanged(currentCameraPosition);
        }
    }

    private void OnCameraPositionChanged(Vector3 newPosition)
    {
        // カメラ位置が変更されたときにイベントをトリガー
        CameraPositionChanged?.Invoke(this, new CameraPositionChangedEventArgs(newPosition));
    }
}
