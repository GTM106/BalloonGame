using System;
using UnityEngine;

public class CameraPositionChangedEventArgs : EventArgs
{
    public Vector3 CameraPosition { get; }

    public CameraPositionChangedEventArgs(Vector3 cameraPosition)
    {
        CameraPosition = cameraPosition;
    }
}
