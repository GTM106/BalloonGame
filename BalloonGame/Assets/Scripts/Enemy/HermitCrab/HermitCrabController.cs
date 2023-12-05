using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using VSCodeEditor;

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
    [Header("�ǐՎ��Ԃ𒴂����ꍇ�ɍēx�ǐՂ���܂ł̎���")]
    [SerializeField] private float coolTime = default!;
    [SerializeField] PlayerGameOverEvent _gameOverEvent = default!;
    [SerializeField] Animator animator = default!;
    [SerializeField] NavMeshAgent navMeshAgent = default!;


    private int currentPatrolNumber = 0;
    private float currentTime = 0;
    private float currentCoolTime = 0;
    private float distance = 0;
    private bool inArea = false;
    private bool coolTimeCheck = false;

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

        CoolTimeCheck();

        if (!inArea)
        {
            ChaseCheack();
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
        if (distance <= chaseRadius && !coolTimeCheck)
        {
            Vector3 vector3 = player.transform.position - this.transform.position;
            vector3.y = 0f;
            Quaternion quaternion = Quaternion.LookRotation(vector3);
            transform.rotation = quaternion;
            //SE302 �Đ�
            animator.Play("AN02_discovery");
        }
    }
    void CoolTimeCheck()
    {
        if (coolTimeCheck)
        {
            currentCoolTime += Time.deltaTime;
        }

        if (currentCoolTime > coolTime)
        {
            coolTimeCheck = false;
            currentCoolTime = 0;
        }
    }
    void Chase()
    {
        animator.Play("AN02_dash");
        navMeshAgent.speed = chaseSpeed;
        navMeshAgent.destination = player.position;
        currentTime += Time.deltaTime;

        if (currentTime >= chaseTime)
        {
            inArea = false;
            coolTimeCheck = true; ;
            currentTime = 0;
            animator.Play("AN02_walk");
        }
    }

    public void DiscoveryEnd()
    {
        inArea = true;
    }
}