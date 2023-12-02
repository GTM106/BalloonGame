using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class HermitCrabController : MonoBehaviour, IHittable
{
    [Header("�ǐՑΏ�")]
    [SerializeField] private Transform player = default!;
    [Header("���񂷂�n�_")]
    [SerializeField] private Transform[] wayPoints = default!;
    [Header("���񑬓x")]
    [SerializeField] private float patrolSpeed = default!;
    [Header("�ǐՔ͈�")]
    [SerializeField] private float chaseRadius = default!;
    [Header("�ǐՑ��x")]
    [SerializeField] private float chaseSpeed = default!;
    [Header("�ǐՎ���")]
    [SerializeField] private float chaseTime = default!;
    [SerializeField] PlayerGameOverEvent _gameOverEvent = default!;
    [SerializeField] NavMeshAgent navMeshAgent = default;

    private int currentPatrolNumber = 0;
    private float currentTime = 0;
    private float distance = 0;
    private bool InArea = false;

    void Awake()
    {
        navMeshAgent.SetDestination(wayPoints[0].position);
    }

    void Update()
    {
        Patrol();
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

    void Patrol()
    {
        distance = Vector3.Distance(transform.position, player.transform.position);

        ChaseCheack();

        if (!InArea)
        {
            navMeshAgent.speed = patrolSpeed;

            if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
            {
                currentPatrolNumber = (currentPatrolNumber + 1) % wayPoints.Length;

                navMeshAgent.SetDestination(wayPoints[currentPatrolNumber].position);
            }
        }
        else
        {
            Chase();
        }
    }

    void ChaseCheack()
    {
        if (distance <= chaseRadius)
        {
            //SE302 �Đ�
            //AN303 �Đ�
            InArea = true;
        }
    }
    void Chase()
    {
        //AN301�@�Đ�
        navMeshAgent.speed = chaseSpeed;
        navMeshAgent.destination = player.position;

        currentTime += Time.deltaTime;

        if (currentTime >= chaseTime)
        {
            InArea = false;
            currentTime = 0;
        }
    }
}