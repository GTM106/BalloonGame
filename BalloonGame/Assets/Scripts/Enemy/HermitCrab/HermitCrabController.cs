using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class HermitCrabController : MonoBehaviour, IHittable
{
    [Header("追跡対象")]
    [SerializeField] private Transform player = default!;
    //[Header("追跡対象")]
    //[SerializeField] private GameObject player_ = default!;
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

    private NavMeshAgent navMeshAgent;
    private int currentPatrolNumber = 0;
    private float currentTime = 0;
    private float distance = 0;
    private bool InArea = false;

    void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.SetDestination(wayPoints[0].position);
    }
    void Update()
    {
        Patrol();
    }

    public void OnEnter(Collider playerCollider, BalloonState balloonState)
    {
        //TODO:ゲームオーバー処理に遷移
        Debug.Log("接触");
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
        Debug.Log(distance);
        if (distance <= chaseRadius)
        {
            //SE302 再生
            //AN303 再生
            InArea = true;
            Debug.Log("追跡");

        }

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
            Chace();
        }
    }

    void Chace()
    {
        //AN301　再生
        navMeshAgent.speed = chaseSpeed;
        navMeshAgent.destination = player.position;
        currentTime = Time.time;
        Debug.Log(currentTime);

        if (currentTime >= chaseTime)
        {
            InArea = false;
            currentTime = 0;
        }
    }
}