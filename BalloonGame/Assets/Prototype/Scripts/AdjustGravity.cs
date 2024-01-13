using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdjustGravity : MonoBehaviour
{
    [SerializeField] Rigidbody rb;

    [Header("重力を変更する。1が通常の重力。0は無重力")]
    [SerializeField] float multiplier = 1f;

    private void Reset()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        AdjustingGravity();
    }

    private void AdjustingGravity()
    {
        //1を基準とする値だけ重力を追加で掛ける
        rb.AddForce((multiplier - 1f) * Physics.gravity, ForceMode.Acceleration);
    }
}
