using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

public class CompositePass : ScriptableRenderPass
{
    //SSRパスで使用するマテリアルとパスに関連する変数
    private Material m_PassMaterial;
    private int m_PassIndex;
    private bool m_RequiresColor;
    private bool m_IsBeforeTransparents;
    private PassData m_PassData;

    private RTHandle m_CopiedColor;

    //Blit テクスチャ用のシェーダープロパティ ID
    private static readonly int m_BlitTextureShaderID = Shader.PropertyToID("_BlitTexture");

    //レンダーパスのセットアップを行うメソッド
    public void Setup(Material mat, int index, bool requiresColor, bool isBeforeTransparents, string featureName, in RenderingData renderingData)
    {
        //マテリアルやフラグなどのセットアップ
        m_PassMaterial = mat;
        m_PassIndex = index;
        m_RequiresColor = requiresColor;
        m_IsBeforeTransparents = isBeforeTransparents;

        //コピーされたカラーテクスチャのディスクリプタを作成
        var colorCopyDescriptor = renderingData.cameraData.cameraTargetDescriptor;
        colorCopyDescriptor.depthBufferBits = (int)DepthBits.None;

        //コピーされたカラーテクスチャを確保
        RenderingUtils.ReAllocateIfNeeded(ref m_CopiedColor, colorCopyDescriptor, name: "_FullscreenPassColorCopy");

        //レンダーパスのデータクラスの初期化
        m_PassData ??= new ();
    }

    //レンダーパスの解放を行うメソッド
    public void Dispose()
    {
        m_CopiedColor?.Release();
    }

    //レンダーパスの実行を行うメソッド
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        //レンダーパスのデータを渡して実際の描画処理を行う
        m_PassData.effectMaterial = m_PassMaterial;
        m_PassData.passIndex = m_PassIndex;
        m_PassData.requiresColor = m_RequiresColor;
        m_PassData.isBeforeTransparents = m_IsBeforeTransparents;
        m_PassData.copiedColor = m_CopiedColor;

        ExecutePass(m_PassData, ref renderingData, ref context);
    }

    //カスタム描画処理を行うメソッド
    private void ExecutePass(PassData passData, ref RenderingData renderingData, ref ScriptableRenderContext context)
    {
        //レンダーパスのデータから必要な情報を取得
        var passMaterial = passData.effectMaterial;
        var requiresColor = passData.requiresColor;
        var isBeforeTransparents = passData.isBeforeTransparents;
        var copiedColor = passData.copiedColor;

        //マテリアルが存在しない場合は描画せずに終了
        if (passMaterial == null) return;

        //プレビューカメラの場合は描画をスキップ
        if (renderingData.cameraData.isPreviewCamera) return;

        //コマンドバッファの取得
        CommandBuffer cmd = CommandBufferPool.Get();
        var cameraData = renderingData.cameraData;

        //カラーが必要な場合の処理
        if (requiresColor)
        {
            //透明オブジェクトの前の描画の場合、Blit で問題が発生する可能性があるため、中間の RTHandle を追加して解決
            var source = isBeforeTransparents ? cameraData.renderer.cameraColorTargetHandle : cameraData.renderer.cameraColorTargetHandle;

            //カメラのテクスチャをコピーしてマテリアルにセット
            Blitter.BlitCameraTexture(cmd, source, copiedColor);
            passMaterial.SetTexture(m_BlitTextureShaderID, copiedColor);
        }

        //レンダーターゲットを設定してフルスクリーン描画
        CoreUtils.SetRenderTarget(cmd, cameraData.renderer.cameraColorTargetHandle);
        CoreUtils.DrawFullScreen(cmd, passMaterial);

        //コマンドバッファを実行
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
    }

    //レンダーパスで使用するデータを格納するクラス
    private class PassData
    {
        internal Material effectMaterial;
        internal int passIndex;
        internal bool requiresColor;
        internal bool isBeforeTransparents;
        public RTHandle copiedColor;
    }
}