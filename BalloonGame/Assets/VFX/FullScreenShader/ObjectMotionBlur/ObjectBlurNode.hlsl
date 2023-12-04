#ifndef OBJECTBLURNODE_H
#define OBJECTBLURNODE_H
//重複したヘッダーの読み込みを防ぐ

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"

//ShaderGraph用のカスタム関数の宣言
void MyFunction_float(UnityTexture2D colorTex,float maxSamples, float2 UV,float2 velocity, out float4 result)
{
    //マイナスの値はShaderGraph側で弾いてる
    //サンプル数の初期化
    //今後付加などの問題でサンプル数を変更最適化する処理等が追加されるかもなので変数に格納
    float samples = maxSamples;

    //サンプルを取得して格納
    result = SAMPLE_TEXTURE2D(colorTex.tex , colorTex.samplerstate ,UV);
    
    //指定されたサンプル数までのループ
    for (int i = 1; i < samples; ++i) {
        //サンプルごとのオフセットを計算
        float2 offset = velocity * (float(i) / float(samples - 1) - 0.5);
        //オフセットを加えてテクスチャをサンプリングし結果に加算
        result += SAMPLE_TEXTURE2D(colorTex.tex , colorTex.samplerstate,UV+offset);
        
    }
    //サンプル数で割って正規化
    result /= float(samples);
}
#endif
