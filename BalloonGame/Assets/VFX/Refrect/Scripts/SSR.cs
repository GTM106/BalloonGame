using System;

namespace UnityEngine.Rendering.Universal
{
    [Serializable, VolumeComponentMenuForRenderPipeline("Screen Space Reflections", typeof(UniversalRenderPipeline))]
    public sealed class SSR : VolumeComponent, IPostProcessComponent
    {
        [Tooltip("反射計算の解像度。値を増やすとパフォーマンス向上。")]
        public ClampedIntParameter downsample = new(2, 1, 8);

        [Tooltip("反射の滑らかさ。値を増やすと滑らかになります。")]
        public MinIntParameter samples = new(8, 0);

        [Tooltip("反射の詳細度。値を増やすと詳細になります。")]
        public MinIntParameter steps = new(125, 0);

        [Tooltip("反射の詳細度。値を増やすと詳細になります。")]
        public ClampedFloatParameter stepSize = new(0.1f, 0f, 10f);

        [Tooltip("物体の境界やエッジの反射の影響。")]
        public ClampedFloatParameter thickness = new(0.1f, 0f, 10f);

        [Tooltip("反射の平滑化。値を増やすと計算コストが低下します。")]
        public ClampedFloatParameter minSmoothness = new(0.1f, 0, 1);
        
        //インターフェース
        public bool IsActive() => true;

        public bool IsTileCompatible() => false;
    }
}
