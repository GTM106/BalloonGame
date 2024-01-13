using System;

namespace UnityEngine.Rendering.Universal
{
    [Serializable, VolumeComponentMenuForRenderPipeline("Screen Space Reflections", typeof(UniversalRenderPipeline))]
    public sealed class SSR : VolumeComponent, IPostProcessComponent
    {
        [Tooltip("���ˌv�Z�̉𑜓x�B�l�𑝂₷�ƃp�t�H�[�}���X����B")]
        public ClampedIntParameter downsample = new(2, 1, 8);

        [Tooltip("���˂̊��炩���B�l�𑝂₷�Ɗ��炩�ɂȂ�܂��B")]
        public MinIntParameter samples = new(8, 0);

        [Tooltip("���˂̏ڍדx�B�l�𑝂₷�ƏڍׂɂȂ�܂��B")]
        public MinIntParameter steps = new(125, 0);

        [Tooltip("���˂̏ڍדx�B�l�𑝂₷�ƏڍׂɂȂ�܂��B")]
        public ClampedFloatParameter stepSize = new(0.1f, 0f, 10f);

        [Tooltip("���̂̋��E��G�b�W�̔��˂̉e���B")]
        public ClampedFloatParameter thickness = new(0.1f, 0f, 10f);

        [Tooltip("���˂̕������B�l�𑝂₷�ƌv�Z�R�X�g���ቺ���܂��B")]
        public ClampedFloatParameter minSmoothness = new(0.1f, 0, 1);
        
        //�C���^�[�t�F�[�X
        public bool IsActive() => true;

        public bool IsTileCompatible() => false;
    }
}
