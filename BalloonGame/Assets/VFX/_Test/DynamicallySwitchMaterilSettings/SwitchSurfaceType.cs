using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Renderer))] // Renderer�������I�ɃA�^�b�`
[RequireComponent(typeof(Volume))] // Renderer�������I�ɃA�^�b�`
public class SwitchSurfaceType : MonoBehaviour
{
    [SerializeField] private CameraPositionNotifier cameraPositionNotifier;
    [SerializeField] private Renderer objectRenderer;
    [SerializeField] private Collider cachedCollider; // �R���C�_�[�R���|�[�l���g�̃L���b�V��

    private void Awake()
    {
        if (objectRenderer == null)
        {
            Debug.LogError("Renderer��������܂���ł����B���̃I�u�W�F�N�g��Renderer�����Ă�������");
            return; // �������^�[��
        }

        if (cachedCollider == null)
        {
            Debug.LogError("Collider��������܂���ł����B���̃I�u�W�F�N�g��Collider�����Ă�������");
            return;
        }

        // �J�����ʒu�ύX�C�x���g���Ď�
        if (cameraPositionNotifier == null)
        {
            Debug.LogError("CameraPositionNotifier���A�^�b�`����Ă��܂���B�C���X�y�N�^�[����CameraPositionNotifier���A�^�b�`���Ă��������B");
            return; // �������^�[��
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
