using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class HermitCrabController : MonoBehaviour, IHittable
{
    [Header("追跡対象")]
    [SerializeField] private Transform player = default!;
    [Header("巡回する地点")]
    [SerializeField] private Transform[] wayPoints = default!;
    [Header("巡回速度")]
    [SerializeField] private float patrolSpeed = default!;
    [Header("追跡範囲")]
    [SerializeField] private float chaseRadius = default!;
    [Header("追跡速度")]
    [SerializeField] private float chaseSpeed = default!;
    [Header("追跡時間")]
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
            //SE302 再生
            //AN303 再生
            InArea = true;
        }
    }
    void Chase()
    {
        //AN301　再生
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