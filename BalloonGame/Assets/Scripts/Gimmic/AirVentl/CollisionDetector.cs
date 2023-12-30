using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionDetector : MonoBehaviour, IHittable
{
    [SerializeField, Required] InflateMoveScript _inflatableMove = default!;
    [SerializeField, Required] PlayerController _player = default!;

    Transform _prebPlayerParent;

    private void Awake()
    {
        _prebPlayerParent = _player.transform.parent;
    }

    private void AttachChild()
    {
        _prebPlayerParent = _player.transform.parent;
        _player.transform.parent = _inflatableMove.transform;
    }

    private void DetachParent()
    {
        //�ړ������A�������ꍇ�ȂǂɃo�O�邱�Ƃ��l�����܂��B
        _player.transform.parent = _prebPlayerParent;
    }

    public void OnEnter(Collider playerCollider, BalloonState balloonState)
    {
        AttachChild();
    }

    public void OnStay(Collider playerCollider, BalloonState balloonState)
    {
    }

    public void OnExit(Collider playerCollider, BalloonState balloonState)
    {
        DetachParent();
    }
}
