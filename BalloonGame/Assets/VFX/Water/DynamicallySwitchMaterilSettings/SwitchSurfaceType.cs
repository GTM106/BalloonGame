using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Renderer))] // Renderer�������I�ɃA�^�b�`
[RequireComponent(typeof(Volume))] // Renderer�������I�ɃA�^�b�`
public class SwitchSurfaceType : MonoBehaviour
{
    private Renderer objectRenderer;

    private void Awake()
    {
        objectRenderer = GetComponent<Renderer>();

        if (objectRenderer == null)
        {
            Debug.LogError("Renderer��������܂���ł����B���̃I�u�W�F�N�g��Renderer�����Ă�������");
            return; // �������^�[��
        }

        // �J�����ʒu�ύX�C�x���g���Ď�
        CameraPositionNotifier notifier = FindObjectOfType<CameraPositionNotifier>();
        if (notifier == null)
        {
            Debug.LogError("CameraPositionNotifier���V�[����ɂ���܂���B�ǂ��ł������̂Œǉ����Ă��������B");
            return; // �������^�[��
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
