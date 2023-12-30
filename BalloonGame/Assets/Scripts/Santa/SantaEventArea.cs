using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SantaEventArea : MonoBehaviour, IHittable
{
    [SerializeField] SunglassController _sunglassController = default!;

    public void OnEnter(Collider playerCollider, BalloonState balloonState)
    {
        _sunglassController.Enable();
    }

    public void OnExit(Collider playerCollider, BalloonState balloonState)
    {
    }

    public void OnStay(Collider playerCollider, BalloonState balloonState)
    {
    }
}
