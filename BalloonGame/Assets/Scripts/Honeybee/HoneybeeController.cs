using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class HoneybeeController : MonoBehaviour, IHittable
{
    [SerializeField] Rigidbody _rigidbody;

    [SerializeField] List<Transform> _wayPoints;
    [SerializeField, Min(0f)] float _moveSpeed = 0.5f;
    [SerializeField, Min(0f)] float _wayPointDistance = 0.02f;

    int _currentPoint = 0;

    //transform�̃L���b�V��
    Transform _transform;

    private void Awake()
    {
        _transform = transform;
    }

    private void Reset()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        Move();
    }

    public void OnEnter(Collider playerCollider, BalloonState balloonState)
    {
        //TODO:�Q�[���I�[�o�[�����ɑJ��
        print("�Q�[���I�[�o�[�����ɑJ�ڂ��܂��B���͖������̂��ߑJ�ڂ��܂���");
    }

    public void OnExit(Collider playerCollider, BalloonState balloonState)
    {
        //DoNothing
    }

    public void OnStay(Collider playerCollider, BalloonState balloonState)
    {
        //DoNothing
    }

    private void Move()
    {
        Vector3 wayPoint = _wayPoints[_currentPoint].position;
        Vector3 honeybeePos = _transform.position;

        //���ݒn�ƖړI�n�̋��������l�ȉ��Ȃ�ړI�n�ɒB�����Ƃ���
        if (Vector3.Distance(honeybeePos, wayPoint) <= _wayPointDistance)
        {
            //���̒n�_��ڕW�ɂ���
            _currentPoint = (++_currentPoint) % _wayPoints.Count;
            return;
        }

        Vector3 pos = Vector3.MoveTowards(honeybeePos, wayPoint, _moveSpeed);
        _rigidbody.MovePosition(pos);

        //��̌�����ړI�n�ɂ���
        _transform.LookAt(_wayPoints[_currentPoint].position);
    }
}
