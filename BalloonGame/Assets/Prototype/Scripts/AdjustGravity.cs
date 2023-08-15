using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdjustGravity : MonoBehaviour
{
    [SerializeField] Rigidbody rb;

    [Header("�d�͂�ύX����B1���ʏ�̏d�́B0�͖��d��")]
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
        //1����Ƃ���l�����d�͂�ǉ��Ŋ|����
        rb.AddForce((multiplier - 1f) * Physics.gravity, ForceMode.Acceleration);
    }
}
