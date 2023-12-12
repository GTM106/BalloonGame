#ifndef SSR_BLUR_H
#define SSR_BLUR_H

//�ڂ��������s����֐�
void Blur_float(Texture2D tex, sampler samplertex, float strength, float radius, float iterations, float2 uv, float2 screenSize, out float4 color)
{
    //���͂̃s�N�Z����5x5�̃J�[�l���ŃT���v�����O
    color = 0.0;
    float weightSum = 0.0;

    //��ʂ̃A�X�y�N�g��ɂ�镝�̕␳
    float widthCorrection = screenSize.x / screenSize.y;

    //�e�s�N�Z�����T���v�����O
    UNITY_LOOP//�����œK��
        for (int y = -iterations; y <= iterations; y++)
        {
            UNITY_LOOP
                for (int x = -iterations; x <= iterations; x++)
                {
                    //�K�E�V�A���֐����g�p���Č��݂̃s�N�Z���̏d�݂��v�Z
                    float weight = exp(-((x * x) + (y * y)) / (2 * (strength * strength))) / (2 * 3.141592 * (strength * strength));

                    //���݂̃I�t�Z�b�g�Ńs�N�Z�����T���v�����O
                    float4 texSample = SAMPLE_TEXTURE2D(tex, samplertex, uv + (float2(x, y) * float2(radius / widthCorrection, radius)));

                    //�d�ݕt���̃s�N�Z���l��ݐ�
                    color += texSample * weight;
                    weightSum += weight;
                }
        }

    //�d�݂̍��v�ŐF�𐳋K��
    color /= weightSum;

    //�F��͈� [0, 50000] �ɃN�����v
    color = clamp(color, 0, 50000);

    return;
}

#endif
