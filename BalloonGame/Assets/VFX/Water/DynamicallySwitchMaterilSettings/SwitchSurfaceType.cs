using Cysharp.Threading.Tasks.Triggers;
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
    private Color inWaterColor;
    private Color defaultWaterColor;

    private void Awake()
    {
        objectRenderer = GetComponent<Renderer>();
        defaultWaterColor = objectRenderer.material.GetColor("_DefaultColor");
        inWaterColor = objectRenderer.material.GetColor("_InWaterColor"); ;

        if (inWaterColor == null)
        {
            Debug.LogError("_DefaultColor��������܂���ł����B���̃I�u�W�F�N�g�̃}�e���A����_DefaultColor�����邩�m�F���Ă��������B");
            return; // �������^�[��
        }

        if (defaultWaterColor == null)
        {
            Debug.LogError("_InWaterColor��������܂���ł����B���̃I�u�W�F�N�g�̃}�e���A����_InWaterColor�����邩�m�F���Ă��������B");
            return; // �������^�[��
        }

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
