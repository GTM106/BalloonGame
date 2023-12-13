#ifndef SSR_BLUR_H
#define SSR_BLUR_H

//ぼかしを実行する関数
void Blur_float(Texture2D tex, sampler samplertex, float strength, float radius, float iterations, float2 uv, float2 screenSize, out float4 color)
{
    //周囲のピクセルを5x5のカーネルでサンプリング
    color = 0.0;
    float weightSum = 0.0;

    //画面のアスペクト比による幅の補正
    float widthCorrection = screenSize.x / screenSize.y;

    //各ピクセルをサンプリング
    UNITY_LOOP//自動最適化
        for (int y = -iterations; y <= iterations; y++)
        {
            UNITY_LOOP
                for (int x = -iterations; x <= iterations; x++)
                {
                    //ガウシアン関数を使用して現在のピクセルの重みを計算
                    float weight = exp(-((x * x) + (y * y)) / (2 * (strength * strength))) / (2 * 3.141592 * (strength * strength));

                    //現在のオフセットでピクセルをサンプリング
                    float4 texSample = SAMPLE_TEXTURE2D(tex, samplertex, uv + (float2(x, y) * float2(radius / widthCorrection, radius)));

                    //重み付きのピクセル値を累積
                    color += texSample * weight;
                    weightSum += weight;
                }
        }

    //重みの合計で色を正規化
    color /= weightSum;

    //色を範囲 [0, 50000] にクランプ
    color = clamp(color, 0, 50000);

    return;
}

#endif
