using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterJumping : MonoBehaviour
{
    [SerializeField] AudioSource audioSource;

    static readonly Vector3 power = new(0f, 15f, 0f);

    void OnTriggerEnter(Collider other)
    {
        other.attachedRigidbody.drag = 3f;
        audioSource.Play();

        IWater water = other.GetComponent<IWater>();
        if (water is null) return;
        water.Enter();

    }

    private void OnTriggerExit(Collider other)
    {
        other.attachedRigidbody.drag = 0f;

        IWater water = other.GetComponent<IWater>();
        if (water is null) return;
        water.Exit();

    }

    private void OnTriggerStay(Collider other)
    {
        Balloon balloon = other.GetComponentInChildren<Balloon>();

        if (balloon == null) return;
        if (balloon.state == BalloonState.Normal) return;

        other.attachedRigidbody.velocity = power;
    }
}
