using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InflateMoveScript : AirVentInteractable
{
    [Header("�i�s�����̍��W")]
    [SerializeField] Transform targetPoint;

    //�ϐ����ς���
    [Header("�M�~�b�N�����̈ʒu�ɖ߂邩�ǂ���")]
    [SerializeField] bool rollBack = default!;
    [Header("�M�~�b�N���ő�ړ��������猳�̈ʒu�ɖ߂邩�ǂ���")]
    [SerializeField] bool arrivalBack = default!;
    [Header("�v�b�V�����ɐi�ޒP��")]
    [SerializeField] float moveSpeed = default!;
    [Header("�M�~�b�N�����̈ʒu�ɖ߂�P��")]
    [SerializeField] float returnSpeed = default!;
    [Header("�M�~�b�N�����̈ʒu�ɖ߂�n�߂�܂ł̒P��")]
    [SerializeField] float stopTime = default!;

    Vector3 initialPoint = Vector3.zero;

    //�ϐ����ς���
    bool initjalMove = false;
    float rollBackTime = 0f;
    float minDistance = 0.1f;

    private void Awake()
    {
        initialPoint = transform.position;
    }

    private void FixedUpdate()
    {
        RoleBackChecker();
    }

    private void RoleBackChecker()
    {
        if (!rollBack) return;
        if (!initjalMove) return;

        rollBackTime += Time.deltaTime;

        if (rollBackTime >= stopTime)
        {
            RollBack();
        }
    }

    private void RollBack()
    {
        transform.position = Vector3.MoveTowards(transform.position, initialPoint, returnSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, initialPoint) <= 0.1f)
        {
            initjalMove = false;
            rollBackTime = 0f;
        }
    }

    public override void Interact()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPoint.position, moveSpeed * Time.deltaTime);

        initjalMove = true;
        rollBackTime = 0f;

        if (Vector3.Distance(transform.position, targetPoint.position) <= minDistance)
        {
            if (!arrivalBack)
            {
                rollBack = false;
            }
        }
    }
}