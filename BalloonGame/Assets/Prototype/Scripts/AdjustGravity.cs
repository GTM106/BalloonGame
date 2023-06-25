using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdjustGravity : MonoBehaviour
{
    [SerializeField] Rigidbody rb;
    [SerializeField] Balloon balloon;

    [Header("重力を変更する。1が通常の重力。0は無重力")]
    [SerializeField] float multiplier;

    private void Reset()
    {
        rb = GetComponent<Rigidbody>();
        balloon = GetComponent<Balloon>();
    }

    void FixedUpdate()
    {
        AdjustingGravity();
    }

    private void AdjustingGravity()
    {
        float multiplierValue = balloon.state switch
        {
            BalloonState.Normal => multiplier,
            BalloonState.Expands => 0.5f,
            _ => throw new System.NotImplementedException(),
        };

        rb.AddForce((multiplierValue - 1f) * Physics.gravity, ForceMode.Acceleration);
    }
}
