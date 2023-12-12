using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct BoostDashData
{
    int _value;
    float _brendShapeWeight;

    public BoostDashData(int value, float brendShapeWeight) { _value = value; _brendShapeWeight = brendShapeWeight; }

    public int Value => _value;
    public float BrendShapeWeight => _brendShapeWeight;

    public void Set(int value, float brendShapeWeight)
    {
        _value = value;
        _brendShapeWeight = brendShapeWeight;
    }
}

/// <summary>
/// ぶっ飛びダッシュのフレーム数をカプセル化したもの
/// </summary>
public class BoostDashEvent : MonoBehaviour
{
    [SerializeField, Required] BalloonController _balloonController = default!;
    [SerializeField, Required] SkinnedMeshRenderer _balloonMeshRenderer = default!;
    [Header("xがMin、yがMaxを表します")]
    [SerializeField] Vector2Int _boostFrames;

    public event Action<BoostDashData> OnBoostDash;
    BoostDashData frame = new();

    public void BoostDash()
    {
        //風船が膨らんでいることが条件
        if (_balloonController.State != BalloonState.Expands) return;

        float blendShapeWeight = _balloonMeshRenderer.GetBlendShapeWeight(0);

        frame.Set(CalcBoostFrame(blendShapeWeight), blendShapeWeight);
        OnBoostDash?.Invoke(frame);
    }

    private int CalcBoostFrame(float blendShapeWeight)
    {
        return _boostFrames.x + (int)((_boostFrames.y - _boostFrames.x) * (blendShapeWeight / 100f));
    }
}
