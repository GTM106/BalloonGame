using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomRenderAdd_OutLineTex : MonoBehaviour
{
    public Material outlineMaterial;
    private RenderTexture renderTexture;

    void Start()
    {
        Camera camera = GetComponent<Camera>();

        // �J�X�^�������_�[�e�N�X�`���̍쐬
        renderTexture = new RenderTexture(Screen.width, Screen.height, 0);

        // �J�����ɃJ�X�^�������_�[�e�N�X�`����ݒ�
        camera.targetTexture = renderTexture;
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        // �J�X�^���V�F�[�_�[���g�p���āA�J�X�^�������_�[�e�N�X�`���ɕ`�悷��
        Graphics.Blit(source, renderTexture, outlineMaterial);

        // �J�X�^�������_�[�e�N�X�`������ʂɕ`�悷��
        Graphics.Blit(renderTexture, destination);
    }
}
