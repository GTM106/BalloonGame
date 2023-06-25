using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterJumping : MonoBehaviour
{
    static readonly Vector3 power = new(0f, 15f, 0f);

    private void OnTriggerStay(Collider other)
    {
        Balloon balloon = other.GetComponentInChildren<Balloon>();

        if (balloon == null) return;
        if (balloon.state == BalloonState.Normal) return;

        other.attachedRigidbody.velocity = power;
    }
}
