using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionDetector : MonoBehaviour,IHittable
{
    [SerializeField] InflateMoveScript inflatableMove = default!;

    public void OnEnter(Collider playerCollider, BalloonState balloonState)
    {
        inflatableMove.AttachChild();
        Debug.Log("ê⁄ë±");
    }

    public void OnStay(Collider playerCollider, BalloonState balloonState)
    {

    }

    public void OnExit(Collider playerCollider, BalloonState balloonState)
    {
        inflatableMove.DetachParent();
        Debug.Log("äOÇÍÇΩ");
    }
}
