using System;
using UnityEngine;

public class CameraPositionNotifier : MonoBehaviour
{
    // �A�N�V�����̐錾
    public event Action<CameraPositionNotifier, CameraPositionChangedEventArgs> CameraPositionChanged;

    private Vector3 lastCameraPosition;
    //���C���J�����̃L���b�V��
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

            // �J�����ʒu���ύX���ꂽ���Ƃ�ʒm
            OnCameraPositionChanged(currentCameraPosition);
        }
    }

    private void OnCameraPositionChanged(Vector3 newPosition)
    {
        // �J�����ʒu���ύX���ꂽ�Ƃ��ɃC�x���g���g���K�[
        CameraPositionChanged?.Invoke(this, new CameraPositionChangedEventArgs(newPosition));
    }
}
