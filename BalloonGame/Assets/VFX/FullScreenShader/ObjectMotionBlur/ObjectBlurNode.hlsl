#ifndef OBJECTBLURNODE_H
#define OBJECTBLURNODE_H
//�d�������w�b�_�[�̓ǂݍ��݂�h��

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"

//ShaderGraph�p�̃J�X�^���֐��̐錾
void MyFunction_float(UnityTexture2D colorTex,float maxSamples, float2 UV,float2 velocity, out float4 result)
{
    //�}�C�i�X�̒l��ShaderGraph���Œe���Ă�
    //�T���v�����̏�����
    //����t���Ȃǂ̖��ŃT���v������ύX�œK�����鏈�������ǉ�����邩���Ȃ̂ŕϐ��Ɋi�[
    float samples = maxSamples;

    //�T���v�����擾���Ċi�[
    result = SAMPLE_TEXTURE2D(colorTex.tex , colorTex.samplerstate ,UV);
    
    //�w�肳�ꂽ�T���v�����܂ł̃��[�v
    for (int i = 1; i < samples; ++i) {
        //�T���v�����Ƃ̃I�t�Z�b�g���v�Z
        float2 offset = velocity * (float(i) / float(samples - 1) - 0.5);
        //�I�t�Z�b�g�������ăe�N�X�`�����T���v�����O�����ʂɉ��Z
        result += SAMPLE_TEXTURE2D(colorTex.tex , colorTex.samplerstate,UV+offset);
        
    }
    //�T���v�����Ŋ����Đ��K��
    result /= float(samples);
}
#endif
