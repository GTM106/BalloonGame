using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace TM.Easing.Management
{
    //----------------------------------------------------■
    //イージングの計算式参考サイト：https://easings.net/ja│
    //----------------------------------------------------│
    //
    //----------------------------------------------------■

    public class EasingManager
    {
        /// <summary>
        /// イージングの進行度を返す。
        /// </summary>
        /// <param name="easeType">イージングの種類</param>
        /// <param name="time">経過時間</param>
        /// <param name="duration">イージング終了時間</param>
        /// <param name="overshootOrAmplitude">オーバー度合い・振幅</param>
        /// <param name="period"></param>
        /// <returns>進行度（0-1）</returns>
        public static float EaseProgress(EaseType easeType, float time, float duration, float overshootOrAmplitude, float period)
        {
            //終了時間が0秒以下なら進行度をMaxにする
            if (duration <= 0f) return 1f;
            
            float x = (time / duration);

            switch (easeType)
            {
                case EaseType.Linear:
                    return x;
                case EaseType.InSine:
                    return 1f - Mathf.Cos(x * (Mathf.PI / 2f));
                case EaseType.OutSine:
                    return Mathf.Sin(x * (Mathf.PI / 2f));
                case EaseType.InOutSine:
                    return -(Mathf.Cos(Mathf.PI * x) - 1f) / 2f;
                case EaseType.InQuad:
                    return x * x;
                case EaseType.OutQuad:
                    return 1f - (1f - x) * (1f - x);
                case EaseType.InOutQuad:
                    return x < 0.5f ? 2f * x * x : 1f - Mathf.Pow(-2f * x + 2f, 2f) / 2f;
                case EaseType.InCubic:
                    return x * x * x;
                case EaseType.OutCubic:
                    return 1 - Mathf.Pow(1 - x, 3);
                case EaseType.InOutCubic:
                    return x < 0.5f ? 4f * x * x * x : 1f - Mathf.Pow(-2f * x + 2f, 3f) / 2f;
                case EaseType.InQuart:
                    return x * x * x * x;
                case EaseType.OutQuart:
                    return 1 - Mathf.Pow(1 - x, 4);
                case EaseType.InOutQuart:
                    return x < 0.5f ? 8 * x * x * x * x : 1f - Mathf.Pow(-2f * x + 2f, 4f) / 2f;
                case EaseType.InQuint:
                    return x * x * x * x * x;
                case EaseType.OutQuint:
                    return 1f - Mathf.Pow(1f - x, 5f);
                case EaseType.InOutQuint:
                    return x < 0.5f ? 16f * x * x * x * x * x : 1f - Mathf.Pow(-2f * x + 2f, 5f) / 2f;
                case EaseType.InExpo:
                    return x <= 0f ? 0f : Mathf.Pow(2f, 10f * x - 10f);
                case EaseType.OutExpo:
                    return 1f <= x ? 1f : 1f - Mathf.Pow(2f, -10f * x);
                case EaseType.InOutExpo:
                    return x <= 0f ? 0f : 
                        1f <= x ? 1f : 
                        x < 0.5f ? Mathf.Pow(2f, 20f * x - 10f) / 2f : (2f - Mathf.Pow(2f, -20f * x + 10f)) / 2f;
                case EaseType.InCirc:
                    return 1f - Mathf.Sqrt(1f - Mathf.Pow(x, 2f));
                case EaseType.OutCirc:
                    return Mathf.Sqrt(1f - Mathf.Pow(x - 1f, 2f));
                case EaseType.InOutCirc:
                    return x < 0.5f ? (1f - Mathf.Sqrt(1f - Mathf.Pow(2f * x, 2f))) / 2f : (Mathf.Sqrt(1f - Mathf.Pow(-2f * x + 2f, 2f)) + 1f) / 2f;
                case EaseType.InBack:
                    return x * x * ((overshootOrAmplitude + 1f) * x - overshootOrAmplitude);
                case EaseType.OutBack:
                    return (x - 1) * (x - 1) * ((overshootOrAmplitude + 1f) * (x - 1) + overshootOrAmplitude) + 1f;
                
                //ここから
                case EaseType.InOutBack:
                    if ((time /= duration * 0.5f) < 1f)
                    {
                        return 0.5f * (time * time * (((overshootOrAmplitude *= 1.525f) + 1f) * time - overshootOrAmplitude));
                    }
                    return 0.5f * ((time -= 2f) * time * (((overshootOrAmplitude *= 1.525f) + 1f) * time + overshootOrAmplitude) + 2f);

                case EaseType.InElastic:
                    {
                        if (time <= 0f)
                        {
                            return 0f;
                        }

                        if ((time /= duration) >= 1f)
                        {
                            return 1f;
                        }

                        if (period <= 0f)
                        {
                            period = duration * 0.3f;
                        }

                        float num3;
                        if (overshootOrAmplitude < 1f)
                        {
                            overshootOrAmplitude = 1f;
                            num3 = period / 4f;
                        }
                        else
                        {
                            num3 = period / (Mathf.PI * 2f) * (float)Mathf.Asin(1f / overshootOrAmplitude);
                        }

                        return 0f - overshootOrAmplitude * Mathf.Pow(2.0f, 10f * (time -= 1f)) * Mathf.Sin((time * duration - num3) * (Mathf.PI * 2f) / period);
                    }
                case EaseType.OutElastic:
                    {
                        if (time <= 0f)
                        {
                            return 0f;
                        }

                        if ((time /= duration) >= 1f)
                        {
                            return 1f;
                        }

                        if (period <= 0f)
                        {
                            period = duration * 0.3f;
                        }

                        float num2;
                        if (overshootOrAmplitude < 1f)
                        {
                            overshootOrAmplitude = 1f;
                            num2 = period / 4f;
                        }
                        else
                        {
                            num2 = period / ((float)Mathf.PI * 2f) * (float)Mathf.Asin(1f / overshootOrAmplitude);
                        }

                        return overshootOrAmplitude * (float)Math.Pow(2.0, -10f * time) * (float)Mathf.Sin((time * duration - num2) * ((float)Mathf.PI * 2f) / period) + 1f;
                    }
                case EaseType.InOutElastic:
                    {
                        if (time <= 0f)
                        {
                            return 0f;
                        }

                        if ((time /= duration * 0.5f) >= 2f)
                        {
                            return 1f;
                        }

                        if (period <= 0f)
                        {
                            period = duration * 0.450000018f;
                        }

                        float num;
                        if (overshootOrAmplitude < 1f)
                        {
                            overshootOrAmplitude = 1f;
                            num = period / 4f;
                        }
                        else
                        {
                            num = period / ((float)Mathf.PI * 2f) * (float)Mathf.Asin(1f / overshootOrAmplitude);
                        }

                        if (time < 1f)
                        {
                            return -0.5f * (overshootOrAmplitude * (float)Math.Pow(2.0, 10f * (time -= 1f)) * (float)Mathf.Sin((time * duration - num) * ((float)Mathf.PI * 2f) / period));
                        }

                        return overshootOrAmplitude * (float)Math.Pow(2.0, -10f * (time -= 1f)) * (float)Mathf.Sin((time * duration - num) * ((float)Mathf.PI * 2f) / period) * 0.5f + 1f;
                    }

                case EaseType.InBounce:
                    return Bounce.EaseIn(time, duration, overshootOrAmplitude, period);
                case EaseType.OutBounce:
                    return Bounce.EaseOut(time, duration, overshootOrAmplitude, period);
                case EaseType.InOutBounce:
                    return Bounce.EaseInOut(time, duration, overshootOrAmplitude, period);
                default:
                    return x;
            }
        }

        public static class Bounce
        {
            //後ろ2つ引数はEaseProgressの引数の数と一致させるためにつけています。

            public static float EaseIn(float time, float duration, float unusedOvershootOrAmplitude, float unusedPeriod)
            {
                return 1f - EaseOut(duration - time, duration, -1f, -1f);
            }

            public static float EaseOut(float time, float duration, float unusedOvershootOrAmplitude, float unusedPeriod)
            {
                const float n1 = 7.5625f;
                const float d1 = 2.75f;
                float x = time / duration;

                if (x < 1f / d1)
                {
                    return n1 * x * x;
                }
                else if (x < 2f / d1)
                {
                    return n1 * (x -= 1.5f / d1) * x + 0.75f;
                }
                else if (x < 2.5f / d1)
                {
                    return n1 * (x -= 2.25f / d1) * x + 0.9375f;
                }
                else
                {
                    return n1 * (x -= 2.625f / d1) * x + 0.984375f;
                }
            }

            public static float EaseInOut(float time, float duration, float unusedOvershootOrAmplitude, float unusedPeriod)
            {
                if (time < duration * 0.5f)
                {
                    return EaseIn(time * 2f, duration, -1f, -1f) * 0.5f;
                }

                return EaseOut(time * 2f - duration, duration, -1f, -1f) * 0.5f + 0.5f;
            }
        }

    }
}