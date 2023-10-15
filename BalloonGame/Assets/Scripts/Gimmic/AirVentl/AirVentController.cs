using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirVentController : MonoBehaviour, IHittable
{
    [SerializeField] AirventEvent _airventEvent = default!;
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
        _airventEvent.OnEnter();
        _airVentHandler.OnRingconPush += OnRingconPush;
    }

    public void OnExit(Collider playerCollider, BalloonState balloonState)
    {
        gameObject.layer = _defaultLayer;
        _airventEvent.OnExit();
        _airVentHandler.OnRingconPush -= OnRingconPush;
    }

    public void OnStay(Collider playerCollider, BalloonState balloonState)
    {

    }
}
