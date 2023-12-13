using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeLimit
{
    //��������
    float _valueInSeconds;

    //�������Ԃ̍ŏ��͓�����O����0�A�ύX�͔񐄏��B
    const float MIN = 0f;

    public TimeLimit(float valueInSeconds)
    {
        if (valueInSeconds < MIN) throw new ArgumentException("�������Ԃ͕��̐��ɂ͂Ȃ�܂���B�l���������Ă��������B", nameof(valueInSeconds));

        _valueInSeconds = valueInSeconds;
    }

    /// <summary>
    /// �������Ԃ̒ǉ����s���B
    /// </summary>
    /// <param name="addValue">�ǉ�����������</param>
    /// <returns>�ǉ���̃C���X�^���X</returns>
    public TimeLimit AddTimeLimit(TimeLimit addValue)
    {
        float newTimeLimit = _valueInSeconds + addValue._valueInSeconds;

        return new TimeLimit(newTimeLimit);
    }

    /// <summary>
    /// �������Ԃ̌������s���B
    /// </summary>
    /// <param name="reduceValue">��������������</param>
    /// <returns>������̃C���X�^���X</returns>
    public TimeLimit ReduceTimeLimit(TimeLimit reduceValue)
    {
        float newTimeLimit = Mathf.Max(_valueInSeconds - reduceValue._valueInSeconds, MIN);

        return new TimeLimit(newTimeLimit);
    }

    /// <summary>
    /// �������Ԃ𖈃t���[�����炷�BUpdate�ɔz�u��z��
    /// </summary>
    /// �{���̒l�I�u�W�F�N�g�I�ɂ́A�V���ɐ������Ԃ�s�ςɂ��ăC���X�^���X�𐶐����ׂ��ł��B
    /// �������A���t���[���C���X�^���X�𐶐����鏈���͓��쑬�x�I�ɔ��������̂ł��̂悤�ɂ��܂��B
    public void DecreaseTimeDeltaTime()
    {
        _valueInSeconds -= Time.deltaTime;

        if (_valueInSeconds < MIN) _valueInSeconds = MIN;
    }

    /// <summary>
    /// �������Ԃɓ��B�������𔻒肷��
    /// </summary>
    /// <returns>���B�����Ftrue </returns>
    public bool IsTimeLimitReached()
    {
        return _valueInSeconds <= MIN;
    }

    public float CurrentTimeLimitValue => _valueInSeconds;
}
