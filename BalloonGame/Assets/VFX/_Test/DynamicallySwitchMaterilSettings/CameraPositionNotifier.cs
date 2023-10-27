using System;
using UnityEngine;

public class CameraPositionNotifier : MonoBehaviour
{
    // �C�x���g�f���Q�[�g�ƃC�x���g�̐錾
    public delegate void CameraPositionChangedEventHandler(object sender, CameraPositionChangedEventArgs e);
    public event CameraPositionChangedEventHandler CameraPositionChanged;

    private Vector3 lastCameraPosition;

    private void Update()
    {
        Vector3 currentCameraPosition = Camera.main.transform.position;
        if (currentCameraPosition != lastCameraPosition)
        {
            lastCameraPosition = currentCameraPosition;

            // �J�����ʒu���ύX���ꂽ���Ƃ�ʒm
            OnCameraPositionChanged(currentCameraPosition);
        }
    }

    private void OnCameraPositionChanged(Vector3 newPosition)
    {
        // �J�����ʒu���ύX���ꂽ�Ƃ��ɃC�x���g���g���K�[
        if (CameraPositionChanged != null)
        {
            CameraPositionChanged(this, new CameraPositionChangedEventArgs(newPosition));
        }
    }
}
