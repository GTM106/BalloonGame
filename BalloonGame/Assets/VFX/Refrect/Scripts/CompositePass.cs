using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

public class CompositePass : ScriptableRenderPass
{
    //SSR�p�X�Ŏg�p����}�e���A���ƃp�X�Ɋ֘A����ϐ�
    private Material m_PassMaterial;
    private int m_PassIndex;
    private bool m_RequiresColor;
    private bool m_IsBeforeTransparents;
    private PassData m_PassData;

    private RTHandle m_CopiedColor;

    //Blit �e�N�X�`���p�̃V�F�[�_�[�v���p�e�B ID
    private static readonly int m_BlitTextureShaderID = Shader.PropertyToID("_BlitTexture");

    //�����_�[�p�X�̃Z�b�g�A�b�v���s�����\�b�h
    public void Setup(Material mat, int index, bool requiresColor, bool isBeforeTransparents, string featureName, in RenderingData renderingData)
    {
        //�}�e���A����t���O�Ȃǂ̃Z�b�g�A�b�v
        m_PassMaterial = mat;
        m_PassIndex = index;
        m_RequiresColor = requiresColor;
        m_IsBeforeTransparents = isBeforeTransparents;

        //�R�s�[���ꂽ�J���[�e�N�X�`���̃f�B�X�N���v�^���쐬
        var colorCopyDescriptor = renderingData.cameraData.cameraTargetDescriptor;
        colorCopyDescriptor.depthBufferBits = (int)DepthBits.None;

        //�R�s�[���ꂽ�J���[�e�N�X�`�����m��
        RenderingUtils.ReAllocateIfNeeded(ref m_CopiedColor, colorCopyDescriptor, name: "_FullscreenPassColorCopy");

        //�����_�[�p�X�̃f�[�^�N���X�̏�����
        m_PassData ??= new ();
    }

    //�����_�[�p�X�̉�����s�����\�b�h
    public void Dispose()
    {
        m_CopiedColor?.Release();
    }

    //�����_�[�p�X�̎��s���s�����\�b�h
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        //�����_�[�p�X�̃f�[�^��n���Ď��ۂ̕`�揈�����s��
        m_PassData.effectMaterial = m_PassMaterial;
        m_PassData.passIndex = m_PassIndex;
        m_PassData.requiresColor = m_RequiresColor;
        m_PassData.isBeforeTransparents = m_IsBeforeTransparents;
        m_PassData.copiedColor = m_CopiedColor;

        ExecutePass(m_PassData, ref renderingData, ref context);
    }

    //�J�X�^���`�揈�����s�����\�b�h
    private void ExecutePass(PassData passData, ref RenderingData renderingData, ref ScriptableRenderContext context)
    {
        //�����_�[�p�X�̃f�[�^����K�v�ȏ����擾
        var passMaterial = passData.effectMaterial;
        var requiresColor = passData.requiresColor;
        var isBeforeTransparents = passData.isBeforeTransparents;
        var copiedColor = passData.copiedColor;

        //�}�e���A�������݂��Ȃ��ꍇ�͕`�悹���ɏI��
        if (passMaterial == null) return;

        //�v���r���[�J�����̏ꍇ�͕`����X�L�b�v
        if (renderingData.cameraData.isPreviewCamera) return;

        //�R�}���h�o�b�t�@�̎擾
        CommandBuffer cmd = CommandBufferPool.Get();
        var cameraData = renderingData.cameraData;

        //�J���[���K�v�ȏꍇ�̏���
        if (requiresColor)
        {
            //�����I�u�W�F�N�g�̑O�̕`��̏ꍇ�ABlit �Ŗ�肪��������\�������邽�߁A���Ԃ� RTHandle ��ǉ����ĉ���
            var source = isBeforeTransparents ? cameraData.renderer.cameraColorTargetHandle : cameraData.renderer.cameraColorTargetHandle;

            //�J�����̃e�N�X�`�����R�s�[���ă}�e���A���ɃZ�b�g
            Blitter.BlitCameraTexture(cmd, source, copiedColor);
            passMaterial.SetTexture(m_BlitTextureShaderID, copiedColor);
        }

        //�����_�[�^�[�Q�b�g��ݒ肵�ăt���X�N���[���`��
        CoreUtils.SetRenderTarget(cmd, cameraData.renderer.cameraColorTargetHandle);
        CoreUtils.DrawFullScreen(cmd, passMaterial);

        //�R�}���h�o�b�t�@�����s
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
    }

    //�����_�[�p�X�Ŏg�p����f�[�^���i�[����N���X
    private class PassData
    {
        internal Material effectMaterial;
        internal int passIndex;
        internal bool requiresColor;
        internal bool isBeforeTransparents;
        public RTHandle copiedColor;
    }
}