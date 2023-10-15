using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AirVentController : MonoBehaviour, IHittable
{
    [SerializeField] AirventEvent _airventEvent = default!;
    [SerializeField] AirVentHandler _airVentHandler = default!;
    [SerializeField] List<AirVentInteractable> _airVentInteractables = default!;

    int _defaultLayer;

    private void Awake()
    {
        _defaultLayer = gameObject.layer;
    }

    private void OnRingconPush()
    {
        foreach (var item in _airVentInteractables)
        {
            item.Interact();
        }
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
