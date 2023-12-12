using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace UnityEditor.Rendering.Universal
{
    //ScreenSpaceReflections �{�����[���R���|�[�l���g�̃J�X�^���G�f�B�^
    [CustomEditor(typeof(SSR))]
    sealed class SSR_Editor : VolumeComponentEditor
    {
        //�V���A���C�Y���ꂽ�f�[�^�ւ̃A�N�Z�X��񋟂���p�����[�^
        SerializedDataParameter downsample;
        SerializedDataParameter steps;
        SerializedDataParameter stepSize;
        SerializedDataParameter thickness;
        SerializedDataParameter samples;
        SerializedDataParameter minSmoothness;

        public override void OnEnable()
        {
            //ScreenSpaceReflections�N���X�̃v���p�e�B�ւ̃A�N�Z�X������
            var propertyFetcher = new PropertyFetcher<SSR>(serializedObject);

            //�e�v���p�e�B�Ɋ֘A����V���A���C�Y���ꂽ�f�[�^�p�����[�^���擾
            downsample = Unpack(propertyFetcher.Find(x => x.downsample));
            steps = Unpack(propertyFetcher.Find(x => x.steps));
            stepSize = Unpack(propertyFetcher.Find(x => x.stepSize));
            thickness = Unpack(propertyFetcher.Find(x => x.thickness));
            samples = Unpack(propertyFetcher.Find(x => x.samples));
            minSmoothness = Unpack(propertyFetcher.Find(x => x.minSmoothness));
        }

        //�C���X�y�N�^�[�ŕ\�������GUI���\�z���郁�\�b�h
        public override void OnInspectorGUI()
        {
            //�e�v���p�e�B�ɑ΂���G�f�B�^�t�B�[���h���쐬
            PropertyField(downsample);
            PropertyField(steps);
            PropertyField(stepSize);
            PropertyField(thickness);
            PropertyField(samples);
            PropertyField(minSmoothness);
        }
    }
}