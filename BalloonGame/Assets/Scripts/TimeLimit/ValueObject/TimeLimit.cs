using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeLimit
{
    //制限時間
    float _valueInSeconds;

    //制限時間の最小は当たり前だが0、変更は非推奨。
    const float MIN = 0f;

    public TimeLimit(float valueInSeconds)
    {
        if (valueInSeconds < MIN) throw new ArgumentException("制限時間は負の数にはなりません。値を見直してください。", nameof(valueInSeconds));

        _valueInSeconds = valueInSeconds;
    }

    /// <summary>
    /// 制限時間の追加を行う。
    /// </summary>
    /// <param name="addValue">追加したい時間</param>
    /// <returns>追加後のインスタンス</returns>
    public TimeLimit AddTimeLimit(TimeLimit addValue)
    {
        float newTimeLimit = _valueInSeconds + addValue._valueInSeconds;

        return new TimeLimit(newTimeLimit);
    }

    /// <summary>
    /// 制限時間の減少を行う。
    /// </summary>
    /// <param name="reduceValue">減少したい時間</param>
    /// <returns>減少後のインスタンス</returns>
    public TimeLimit ReduceTimeLimit(TimeLimit reduceValue)
    {
        float newTimeLimit = Mathf.Max(_valueInSeconds - reduceValue._valueInSeconds, MIN);

        return new TimeLimit(newTimeLimit);
    }

    /// <summary>
    /// 制限時間を毎フレーム減らす。Updateに配置を想定
    /// </summary>
    /// 本来の値オブジェクト的には、新たに制限時間を不変にしてインスタンスを生成すべきです。
    /// しかし、毎フレームインスタンスを生成する処理は動作速度的に避けたいのでこのようにします。
    public void DecreaseTimeDeltaTime()
    {
        _valueInSeconds -= Time.deltaTime;

        if (_valueInSeconds < MIN) _valueInSeconds = MIN;
    }

    /// <summary>
    /// 制限時間に到達したかを判定する
    /// </summary>
    /// <returns>到達した：true </returns>
    public bool IsTimeLimitReached()
    {
        return _valueInSeconds <= MIN;
    }

    public float CurrentTimeLimitValue => _valueInSeconds;
}
