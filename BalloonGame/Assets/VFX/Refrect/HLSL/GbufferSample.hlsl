#ifndef GbufferSample_H
#define GbufferSample_H

//G-Bufferの各テクスチャへの参照が定義
TEXTURE2D_X_HALF(_GBuffer0);
TEXTURE2D_X_HALF(_GBuffer1);
TEXTURE2D_X_HALF(_GBuffer2);

//カメラの背面深度テクスチャが有効な場合は定義
#if defined(_BACKFACE_ENABLED)
TEXTURE2D_X(_CameraBackDepthTexture);
#endif

//サンプラーの宣言
SAMPLER(sampler_BlitTexture);
SAMPLER(my_point_clamp_sampler);
float4 _BlitTexture_TexelSize;

//マテリアルフラグやリフレクションの定義
#ifndef kMaterialFlagSpecularSetup
#define kMaterialFlagSpecularSetup 8 //リットマテリアルがメタリックセットアップではなくスペキュラセットアップを使用する場合のフラグ
#endif

//法線マップをアンパックする関数
#ifndef kDieletricSpec
#define kDieletricSpec half4(0.04, 0.04, 0.04, 1.0 - 0.04)
#endif

#define PACKED_MATERIAL_FLAGS_MULTIPLIER 255.0h
#define OCT_NORMAL_ENCODE_MULTIPLIER half(2.0)
#define OCT_NORMAL_ENCODE_OFFSET half(1.0)

uint UnpackMaterialFlags(float packedMaterialFlags)
{
    return uint((packedMaterialFlags * PACKED_MATERIAL_FLAGS_MULTIPLIER) + 0.5h);
}

#if defined(_GBUFFER_NORMALS_OCT)
half3 UnpackNormal(half3 pn)
{
    half2 remappedOctNormalWS = half2(Unpack888ToFloat2(pn));          
    half2 octNormalWS = remappedOctNormalWS.xy * OCT_NORMAL_ENCODE_MULTIPLIER - OCT_NORMAL_ENCODE_OFFSET;
    return half3(UnpackNormalOctQuadEncode(octNormalWS));              
}
#else
half3 UnpackNormal(half3 pn) { return pn; }
#endif

//G-Bufferから各情報を取得する関数
void GetGbuffer_float(float2 uv, sampler sample_GB, out float3 Diffuse, out float AO, out float3 Specular, out float Smoothness, out float Metallic, out float3 Normal)
{
    half4 gBuffer0 = SAMPLE_TEXTURE2D_X_LOD(_GBuffer0, sample_GB, uv, 0);
    half4 gBuffer1 = SAMPLE_TEXTURE2D_X_LOD(_GBuffer1, sample_GB, uv, 0);
    half4 gBuffer2 = SAMPLE_TEXTURE2D_X_LOD(_GBuffer2, sample_GB, uv, 0);

    Diffuse = gBuffer0.rgb;
    Specular = (UnpackMaterialFlags(gBuffer0.a) == kMaterialFlagSpecularSetup) ? gBuffer1.rgb : lerp(kDieletricSpec.rgb, max(Diffuse.rgb, kDieletricSpec.rgb), gBuffer1.r);
    Metallic = gBuffer1.r;
    AO = gBuffer1.a;
    Normal = UnpackNormal(gBuffer2.rgb);
    Smoothness = gBuffer2.a;
}

#endif