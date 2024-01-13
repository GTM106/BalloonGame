using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//ScriptableRendererFeature���p�������J�X�^�������_�[�p�X�̋@�\���Ǘ�����N���X
public class ReflectRenderPass : ScriptableRendererFeature
{
    //�J�X�^���p�X�Ŏg�p����}�e���A���ƃ����_�����O�C�x���g�Ɋ֘A����ϐ�
    private Material passMaterial;
    private Material compositeMaterial;
    public RenderPassEvent renderPass = RenderPassEvent.BeforeRenderingPostProcessing;
    private ScriptableRenderPassInput requirements = ScriptableRenderPassInput.Color;
    [HideInInspector]
    public int passIndex = 0;

    //�X�N���v�g�Ŏg�p����SSR_Pass��Composite_Pass�̃C���X�^���X
    private SSR_Pass ssrPass;
    private CompositePass compositePass;

    //�J�X�^���p�X�̗v���Ɋ֘A����ϐ�
    private bool requiresColor;
    private bool injectedBeforeTransparents;
    private bool isEnabled;

    //�J�X�^���p�X�̏����ݒ���s�����\�b�h
    public override void Create()
    {
        ssrPass = new();
        ssrPass.renderPassEvent = renderPass;

        compositePass = new();
        compositePass.renderPassEvent = renderPass;

        //�q���[�}���G���[�h�~�̂��ߒ��ڎw��
        if (passMaterial == null)
        {
            passMaterial = (Material)Resources.Load("SSR_Renderer");
        }
        if (compositeMaterial == null)
        {
            compositeMaterial = (Material)Resources.Load("SSR_Composite");
        }
        //�}�e���A�������[�h�ł��Ă��Ȃ��ꍇ�͌x����\�����ďI��
        if (passMaterial == null || compositeMaterial == null)
        {
            Debug.LogWarning("�}�e���A����������܂���");
            return;
        }

        requiresColor = (requirements & ScriptableRenderPassInput.Color) != 0;
        injectedBeforeTransparents = renderPass <= RenderPassEvent.BeforeRenderingTransparents;
        
        //�v���𒲐����邽�߂̕ϐ�
        ScriptableRenderPassInput modifiedRequirements = requirements & (injectedBeforeTransparents ? requirements : ~ScriptableRenderPassInput.Color);
        ssrPass.ConfigureInput(modifiedRequirements);
        compositePass.ConfigureInput(modifiedRequirements);
    }

    //�����_���[��1�񂾂��Ăяo����A�J�������Ƃɐݒ���s�����\�b�h
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        //SSR�@�\���A�N�e�B�u�ł��邩�ǂ������m�F
        SSR ssr = VolumeManager.instance.stack.GetComponent<SSR>();
        isEnabled = ssr.IsActive();

        //�|�X�g�v���Z�X���L���łȂ��ꍇ��SSR�������ɂ���
        if (!renderingData.postProcessingEnabled) isEnabled = false;

        //SSR�������̏ꍇ�͉��������ɏI��
        if (!isEnabled) return;

        //SSR�p�����[�^���}�e���A���ɐݒ�
        passMaterial.SetFloat("_Samples", ssr.steps.value);
        passMaterial.SetFloat("_BinarySamples", ssr.samples.value);
        passMaterial.SetFloat("_StepSize", ssr.stepSize.value);
        passMaterial.SetFloat("_Thickness", ssr.thickness.value);
        passMaterial.SetFloat("_MinSmoothness", ssr.minSmoothness.value);

        //SSR�p�X�����Composite�p�X�̃Z�b�g�A�b�v
        ssrPass.Setup(passMaterial, passIndex, requiresColor, injectedBeforeTransparents, "SSR", ssr.downsample.value, renderingData);
        compositePass.Setup(compositeMaterial, passIndex, requiresColor, injectedBeforeTransparents, "Comp", renderingData);

        //�����_���[��SSR�p�X��Composite�p�X��ǉ�
        renderer.EnqueuePass(ssrPass);
        renderer.EnqueuePass(compositePass);
    }

    //�J�X�^���p�X�̔j���������s�����\�b�h
    protected override void Dispose(bool disposing)
    {
        ssrPass?.Dispose();
        compositePass?.Dispose();
    }
}