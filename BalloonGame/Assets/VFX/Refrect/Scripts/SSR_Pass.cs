using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

public class SSR_Pass : ScriptableRenderPass
{
    //SSR�p�X�Ŏg�p����}�e���A���ƃp�X�Ɋ֘A����ϐ�
    private Material m_PassMaterial;
    private int m_PassIndex;
    private bool m_RequiresColor;
    private bool m_IsBeforeTransparents;
    private PassData m_PassData;
    private RTHandle m_CopiedColor;
    private static readonly int m_BlitTextureShaderID = Shader.PropertyToID("_BlitTexture");
    private RTHandle ScreenSpaceRelfectionsTex;
    private int downSample;

    //SSR�p�X�̃Z�b�g�A�b�v���s�����\�b�h
    public void Setup(Material mat, int index, bool requiresColor, bool isBeforeTransparents, string featureName, int ds, in RenderingData renderingData)
    {
        m_PassMaterial = mat;
        m_PassIndex = index;
        m_RequiresColor = requiresColor;
        m_IsBeforeTransparents = isBeforeTransparents;

        //�J���[�R�s�[�p��RTHandle���m��
        var colorCopyDescriptor = renderingData.cameraData.cameraTargetDescriptor;
        colorCopyDescriptor.depthBufferBits = (int)DepthBits.None;
        RenderingUtils.ReAllocateIfNeeded(ref m_CopiedColor, colorCopyDescriptor, name: "_FullscreenPassColorCopy");

        //SSR�e�N�X�`���̊m��
        ScreenSpaceRelfectionsTex = RTHandles.Alloc("SSRT", name: "SSRT");
        downSample = ds;

        m_PassData ??= new ();
    }

    //SSR�p�X�Ŏg�p�������\�[�X�̉�����s�����\�b�h
    public void Dispose()
    {
        m_CopiedColor?.Release();
        ScreenSpaceRelfectionsTex?.Release();
    }

    //SSR�p�X�̃����_�[�^�[�Q�b�g�̐ݒ���s�����\�b�h
    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    {
        cmd.SetGlobalTexture("_ScreenSpaceRelfectionsTex", Shader.PropertyToID(ScreenSpaceRelfectionsTex.name));
    }

    //�J�����̃Z�b�g�A�b�v���ɌĂяo����郁�\�b�h
    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        RenderTextureDescriptor cameraTargetDescriptor = renderingData.cameraData.cameraTargetDescriptor;

        RenderTextureDescriptor descriptor = cameraTargetDescriptor;
        descriptor.msaaSamples = 1;
        descriptor.depthBufferBits = 0;

        cameraTargetDescriptor = descriptor;

        //�_�E���T���v���ݒ�
        cameraTargetDescriptor.width /= downSample;
        cameraTargetDescriptor.height /= downSample;
        cameraTargetDescriptor.colorFormat = RenderTextureFormat.DefaultHDR;

        //SSR�e�N�X�`���̈ꎞ�I�Ȋm��
        cmd.GetTemporaryRT(Shader.PropertyToID(ScreenSpaceRelfectionsTex.name), cameraTargetDescriptor, FilterMode.Bilinear);
    }

    //SSR�p�X�̎��s���\�b�h
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (m_PassMaterial == null)
        {
            Debug.Log("�}�e���A������������Ă��܂���B");
            return;
        }
        m_PassData.effectMaterial = m_PassMaterial;
        m_PassData.passIndex = m_PassIndex;
        m_PassData.requiresColor = m_RequiresColor;
        m_PassData.isBeforeTransparents = m_IsBeforeTransparents;
        m_PassData.copiedColor = m_CopiedColor;

        //SSR�p�X�̎��s
        ExecutePass(m_PassData, ref renderingData, ref context);
    }

    //SSR�p�X�̌㏈�����s�����\�b�h
    public override void FrameCleanup(CommandBuffer cmd)
    {
        cmd.ReleaseTemporaryRT(Shader.PropertyToID(ScreenSpaceRelfectionsTex.name));
    }

    //�p�X�̎��s�������s�����\�b�h
    private void ExecutePass(PassData passData, ref RenderingData renderingData, ref ScriptableRenderContext context)
    {
        var passMaterial = passData?.effectMaterial;
        var requiresColor = passData?.requiresColor ?? false;
        var isBeforeTransparents = passData?.isBeforeTransparents ??false;
        var copiedColor = passData?.copiedColor;

        if (passMaterial == null) return;
        if (renderingData.cameraData.isPreviewCamera) return;

        CommandBuffer cmd = CommandBufferPool.Get();
        var cameraData = renderingData.cameraData;

        if (requiresColor)
        {
            //BlitCameraTexture�̃V�i���I�ɂ����̉��
            var source = isBeforeTransparents ? cameraData.renderer.cameraColorTargetHandle : cameraData.renderer.cameraColorTargetHandle;

            Blitter.BlitCameraTexture(cmd, source, copiedColor);
            passMaterial.SetTexture(m_BlitTextureShaderID, copiedColor);
        }

        //SSR�e�N�X�`���ւ̕`��
        CoreUtils.SetRenderTarget(cmd, ScreenSpaceRelfectionsTex);
        CoreUtils.DrawFullScreen(cmd, passMaterial);
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
    }

    //�p�X�̃f�[�^���i�[����N���X
    private class PassData
    {
        internal Material effectMaterial;
        internal int passIndex;
        internal bool requiresColor;
        internal bool isBeforeTransparents;
        public RTHandle copiedColor;
    }
}