using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class HoneybeeController : MonoBehaviour, IHittable
{
    [SerializeField] Rigidbody _rigidbody = default!;
    [SerializeField] PlayerGameOverEvent _gameOverEvent = default!;

    [SerializeField] List<Transform> _wayPoints = default!;
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
        _gameOverEvent = FindAnyObjectByType<PlayerGameOverEvent>();
    }

    void FixedUpdate()
    {
        Move();
    }

    public void OnEnter(Collider playerCollider, BalloonState balloonState)
    {
        _gameOverEvent.GameOver();
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
        if (_wayPoints.Count <= 0) return;

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
