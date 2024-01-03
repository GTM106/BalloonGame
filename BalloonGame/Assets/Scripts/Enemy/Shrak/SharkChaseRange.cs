using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SharkChaseRange : MonoBehaviour, IHittable
{
    [SerializeField] SharkController sharkController = default!;

    public void OnEnter(Collider playerCollider, BalloonState balloonState)
    {
        sharkController.OnChaseCheckRangeCollisionEnter();
    }

    public void OnExit(Collider playerCollider, BalloonState balloonState)
    {
        sharkController.OnChaseCheckRangeCollisionExit();
    }

    public void OnStay(Collider playerCollider, BalloonState balloonState)
    {

    }
}