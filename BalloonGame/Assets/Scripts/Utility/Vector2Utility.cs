using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vector2Utility
{
    /// <summary>
    /// 入力値を4方向にスナップします。
    /// </summary>
    public static Vector2 SnapToFourDirections(this Vector2 input)
    {
        if (input.magnitude <= 0.02f) return Vector2.zero;

        Vector2 snappedInput = Vector2.zero;

        if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
        {
            snappedInput.Set(Mathf.Sign(input.x), 0f);
        }
        else
        {
            snappedInput.Set(0f, Mathf.Sign(input.y));
        }

        return snappedInput;
    }
}
