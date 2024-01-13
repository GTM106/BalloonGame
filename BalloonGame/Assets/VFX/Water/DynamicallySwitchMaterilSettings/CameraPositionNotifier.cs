using System;
using UnityEngine;

public class CameraPositionNotifier : MonoBehaviour
{
    // イベントデリゲートとイベントの宣言
    public delegate void CameraPositionChangedEventHandler(object sender, CameraPositionChangedEventArgs e);
    public event CameraPositionChangedEventHandler CameraPositionChanged;

    private Vector3 lastCameraPosition;

    private void Update()
    {
        Vector3 currentCameraPosition = Camera.main.transform.position;
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
        if (CameraPositionChanged != null)
        {
            CameraPositionChanged(this, new CameraPositionChangedEventArgs(newPosition));
        }
    }
}
