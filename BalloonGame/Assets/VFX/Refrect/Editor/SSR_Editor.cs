using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace UnityEditor.Rendering.Universal
{
    //ScreenSpaceReflections ボリュームコンポーネントのカスタムエディタ
    [CustomEditor(typeof(SSR))]
    sealed class SSR_Editor : VolumeComponentEditor
    {
        //シリアライズされたデータへのアクセスを提供するパラメータ
        SerializedDataParameter downsample;
        SerializedDataParameter steps;
        SerializedDataParameter stepSize;
        SerializedDataParameter thickness;
        SerializedDataParameter samples;
        SerializedDataParameter minSmoothness;

        public override void OnEnable()
        {
            //ScreenSpaceReflectionsクラスのプロパティへのアクセスを準備
            var propertyFetcher = new PropertyFetcher<SSR>(serializedObject);

            //各プロパティに関連するシリアライズされたデータパラメータを取得
            downsample = Unpack(propertyFetcher.Find(x => x.downsample));
            steps = Unpack(propertyFetcher.Find(x => x.steps));
            stepSize = Unpack(propertyFetcher.Find(x => x.stepSize));
            thickness = Unpack(propertyFetcher.Find(x => x.thickness));
            samples = Unpack(propertyFetcher.Find(x => x.samples));
            minSmoothness = Unpack(propertyFetcher.Find(x => x.minSmoothness));
        }

        //インスペクターで表示されるGUIを構築するメソッド
        public override void OnInspectorGUI()
        {
            //各プロパティに対するエディタフィールドを作成
            PropertyField(downsample);
            PropertyField(steps);
            PropertyField(stepSize);
            PropertyField(thickness);
            PropertyField(samples);
            PropertyField(minSmoothness);
        }
    }
}