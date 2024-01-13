using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

public class SSR_Pass : ScriptableRenderPass
{
    //SSRパスで使用するマテリアルとパスに関連する変数
    private Material m_PassMaterial;
    private int m_PassIndex;
    private bool m_RequiresColor;
    private bool m_IsBeforeTransparents;
    private PassData m_PassData;
    private RTHandle m_CopiedColor;
    private static readonly int m_BlitTextureShaderID = Shader.PropertyToID("_BlitTexture");
    private RTHandle ScreenSpaceRelfectionsTex;
    private int downSample;

    //SSRパスのセットアップを行うメソッド
    public void Setup(Material mat, int index, bool requiresColor, bool isBeforeTransparents, string featureName, int ds, in RenderingData renderingData)
    {
        m_PassMaterial = mat;
        m_PassIndex = index;
        m_RequiresColor = requiresColor;
        m_IsBeforeTransparents = isBeforeTransparents;

        //カラーコピー用のRTHandleを確保
        var colorCopyDescriptor = renderingData.cameraData.cameraTargetDescriptor;
        colorCopyDescriptor.depthBufferBits = (int)DepthBits.None;
        RenderingUtils.ReAllocateIfNeeded(ref m_CopiedColor, colorCopyDescriptor, name: "_FullscreenPassColorCopy");

        //SSRテクスチャの確保
        ScreenSpaceRelfectionsTex = RTHandles.Alloc("SSRT", name: "SSRT");
        downSample = ds;

        m_PassData ??= new ();
    }

    //SSRパスで使用したリソースの解放を行うメソッド
    public void Dispose()
    {
        m_CopiedColor?.Release();
        ScreenSpaceRelfectionsTex?.Release();
    }

    //SSRパスのレンダーターゲットの設定を行うメソッド
    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    {
        cmd.SetGlobalTexture("_ScreenSpaceRelfectionsTex", Shader.PropertyToID(ScreenSpaceRelfectionsTex.name));
    }

    //カメラのセットアップ時に呼び出されるメソッド
    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        RenderTextureDescriptor cameraTargetDescriptor = renderingData.cameraData.cameraTargetDescriptor;

        RenderTextureDescriptor descriptor = cameraTargetDescriptor;
        descriptor.msaaSamples = 1;
        descriptor.depthBufferBits = 0;

        cameraTargetDescriptor = descriptor;

        //ダウンサンプル設定
        cameraTargetDescriptor.width /= downSample;
        cameraTargetDescriptor.height /= downSample;
        cameraTargetDescriptor.colorFormat = RenderTextureFormat.DefaultHDR;

        //SSRテクスチャの一時的な確保
        cmd.GetTemporaryRT(Shader.PropertyToID(ScreenSpaceRelfectionsTex.name), cameraTargetDescriptor, FilterMode.Bilinear);
    }

    //SSRパスの実行メソッド
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (m_PassMaterial == null)
        {
            Debug.Log("マテリアルが生成されていません。");
            return;
        }
        m_PassData.effectMaterial = m_PassMaterial;
        m_PassData.passIndex = m_PassIndex;
        m_PassData.requiresColor = m_RequiresColor;
        m_PassData.isBeforeTransparents = m_IsBeforeTransparents;
        m_PassData.copiedColor = m_CopiedColor;

        //SSRパスの実行
        ExecutePass(m_PassData, ref renderingData, ref context);
    }

    //SSRパスの後処理を行うメソッド
    public override void FrameCleanup(CommandBuffer cmd)
    {
        cmd.ReleaseTemporaryRT(Shader.PropertyToID(ScreenSpaceRelfectionsTex.name));
    }

    //パスの実行処理を行うメソッド
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
            //BlitCameraTextureのシナリオによる問題の回避
            var source = isBeforeTransparents ? cameraData.renderer.cameraColorTargetHandle : cameraData.renderer.cameraColorTargetHandle;

            Blitter.BlitCameraTexture(cmd, source, copiedColor);
            passMaterial.SetTexture(m_BlitTextureShaderID, copiedColor);
        }

        //SSRテクスチャへの描画
        CoreUtils.SetRenderTarget(cmd, ScreenSpaceRelfectionsTex);
        CoreUtils.DrawFullScreen(cmd, passMaterial);
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
    }

    //パスのデータを格納するクラス
    private class PassData
    {
        internal Material effectMaterial;
        internal int passIndex;
        internal bool requiresColor;
        internal bool isBeforeTransparents;
        public RTHandle copiedColor;
    }
}