using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirVentController : MonoBehaviour, IHittable
{
    [SerializeField] Canvas _ui1 = default!;

    private void Awake()
    {
        _ui1.enabled = false;
    }

    public void OnEnter(Collider playerCollider, BalloonState balloonState)
    {
        _ui1.enabled = true;
    }

    public void OnExit(Collider playerCollider, BalloonState balloonState)
    {
        _ui1.enabled = false;
    }

    public void OnStay(Collider playerCollider, BalloonState balloonState)
    {
        //DoNothing
    }
}
