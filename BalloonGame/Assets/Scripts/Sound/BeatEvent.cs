using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeatEvent : MonoBehaviour
{
    int _beatCount = 0;

    public event Action OnBeat;

    void FixedUpdate()
    {
        var current = BeatDetector.CurrentTimeSamples;
        var nextBeat = BeatDetector.Instance.GetSamplesWithBeat(_beatCount);

        //���݂�timeSamles�����̃r�[�g�^�C�~���O���z���Ă��邩
        if (current >= nextBeat)
        {
            _beatCount++;
            OnBeat?.Invoke();
        }
    }
}
