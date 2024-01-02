using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SharkRange : MonoBehaviour, IHittable
{
    [SerializeField] SharkController sharkController = default!;
    public void OnEnter(Collider playerCollider, BalloonState balloonState)
    {
        sharkController.OnChaseCheckRangeCollsionEnter();
    }

    public void OnExit(Collider playerCollider, BalloonState balloonState)
    {
        sharkController.OnChaseCheckRangeCollsionExit();
    }

    public void OnStay(Collider playerCollider, BalloonState balloonState)
    {

    }
}