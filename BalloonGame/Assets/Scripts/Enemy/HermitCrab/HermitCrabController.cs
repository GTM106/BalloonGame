using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class HermitCrabController : MonoBehaviour, IHittable
{
    [Header("�ǐՑΏ�")]
    [SerializeField] private Transform player = default!;
    //[Header("�ǐՑΏ�")]
    //[SerializeField] private GameObject player_ = default!;
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
        //TODO:�Q�[���I�[�o�[�����ɑJ��
        Debug.Log("�ڐG");
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
            //SE302 �Đ�
            //AN303 �Đ�
            InArea = true;
            Debug.Log("�ǐ�");

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
        //AN301�@�Đ�
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