using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirVentController : MonoBehaviour, IHittable
{
    [SerializeField] AirVentInteractable _airVentInteractable = default!;
    [SerializeField] Canvas _ui1 = default!;
    [SerializeField] AirVentHandler _airVentHandler = default!;

    private void Awake()
    {
        _ui1.enabled = false;
    }

    private void OnRingconPush()
    {
        _airVentInteractable.Interact();
    }

    public void OnEnter(Collider playerCollider, BalloonState balloonState)
    {
        _ui1.enabled = true;
        _airVentHandler.OnRingconPush += OnRingconPush;
    }

    public void OnExit(Collider playerCollider, BalloonState balloonState)
    {
        _ui1.enabled = false;
        _airVentHandler.OnRingconPush -= OnRingconPush;
    }

    public void OnStay(Collider playerCollider, BalloonState balloonState)
    {
    }
}
