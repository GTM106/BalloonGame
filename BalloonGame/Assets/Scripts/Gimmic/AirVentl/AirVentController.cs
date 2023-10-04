using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirVentController : MonoBehaviour, IHittable
{
    [SerializeField] AirVentInteractable _airVentInteractable = default!;
    [SerializeField] AirVentHandler _airVentHandler = default!;

    int _defaultLayer;

    private void Awake()
    {
        _defaultLayer = gameObject.layer;
    }

    private void OnRingconPush()
    {
        _airVentInteractable.Interact();
    }

    public void OnEnter(Collider playerCollider, BalloonState balloonState)
    {
        gameObject.layer = LayerMask.NameToLayer("Outline");
        _airVentHandler.OnRingconPush += OnRingconPush;
    }

    public void OnExit(Collider playerCollider, BalloonState balloonState)
    {
        gameObject.layer = _defaultLayer;
        _airVentHandler.OnRingconPush -= OnRingconPush;
    }

    public void OnStay(Collider playerCollider, BalloonState balloonState)
    {
    }
}
