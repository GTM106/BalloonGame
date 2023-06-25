using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdjustGravity : MonoBehaviour
{
    [SerializeField] Rigidbody rb;
    [SerializeField] Balloon balloon;

    [Header("�d�͂�ύX����B1���ʏ�̏d�́B0�͖��d��")]
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
