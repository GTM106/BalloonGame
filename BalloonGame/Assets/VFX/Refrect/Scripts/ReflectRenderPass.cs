using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//ScriptableRendererFeatureを継承したカスタムレンダーパスの機能を管理するクラス
public class ReflectRenderPass : ScriptableRendererFeature
{
    //カスタムパスで使用するマテリアルとレンダリングイベントに関連する変数
    private Material passMaterial;
    private Material compositeMaterial;
    public RenderPassEvent renderPass = RenderPassEvent.BeforeRenderingPostProcessing;
    private ScriptableRenderPassInput requirements = ScriptableRenderPassInput.Color;
    [HideInInspector]
    public int passIndex = 0;

    //スクリプトで使用するSSR_PassとComposite_Passのインスタンス
    private SSR_Pass ssrPass;
    private CompositePass compositePass;

    //カスタムパスの要件に関連する変数
    private bool requiresColor;
    private bool injectedBeforeTransparents;
    private bool isEnabled;

    //カスタムパスの初期設定を行うメソッド
    public override void Create()
    {
        ssrPass = new();
        ssrPass.renderPassEvent = renderPass;

        compositePass = new();
        compositePass.renderPassEvent = renderPass;

        //ヒューマンエラー防止のため直接指定
        if (passMaterial == null)
        {
            passMaterial = (Material)Resources.Load("SSR_Renderer");
        }
        if (compositeMaterial == null)
        {
            compositeMaterial = (Material)Resources.Load("SSR_Composite");
        }
        //マテリアルがロードできていない場合は警告を表示して終了
        if (passMaterial == null || compositeMaterial == null)
        {
            Debug.LogWarning("マテリアルが見つかりません");
            return;
        }

        requiresColor = (requirements & ScriptableRenderPassInput.Color) != 0;
        injectedBeforeTransparents = renderPass <= RenderPassEvent.BeforeRenderingTransparents;
        
        //要件を調整するための変数
        ScriptableRenderPassInput modifiedRequirements = requirements & (injectedBeforeTransparents ? requirements : ~ScriptableRenderPassInput.Color);
        ssrPass.ConfigureInput(modifiedRequirements);
        compositePass.ConfigureInput(modifiedRequirements);
    }

    //レンダラーに1回だけ呼び出され、カメラごとに設定を行うメソッド
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        //SSR機能がアクティブであるかどうかを確認
        SSR ssr = VolumeManager.instance.stack.GetComponent<SSR>();
        isEnabled = ssr.IsActive();

        //ポストプロセスが有効でない場合はSSRも無効にする
        if (!renderingData.postProcessingEnabled) isEnabled = false;

        //SSRが無効の場合は何もせずに終了
        if (!isEnabled) return;

        //SSRパラメータをマテリアルに設定
        passMaterial.SetFloat("_Samples", ssr.steps.value);
        passMaterial.SetFloat("_BinarySamples", ssr.samples.value);
        passMaterial.SetFloat("_StepSize", ssr.stepSize.value);
        passMaterial.SetFloat("_Thickness", ssr.thickness.value);
        passMaterial.SetFloat("_MinSmoothness", ssr.minSmoothness.value);

        //SSRパスおよびCompositeパスのセットアップ
        ssrPass.Setup(passMaterial, passIndex, requiresColor, injectedBeforeTransparents, "SSR", ssr.downsample.value, renderingData);
        compositePass.Setup(compositeMaterial, passIndex, requiresColor, injectedBeforeTransparents, "Comp", renderingData);

        //レンダラーにSSRパスとCompositeパスを追加
        renderer.EnqueuePass(ssrPass);
        renderer.EnqueuePass(compositePass);
    }

    //カスタムパスの破棄処理を行うメソッド
    protected override void Dispose(bool disposing)
    {
        ssrPass?.Dispose();
        compositePass?.Dispose();
    }
}