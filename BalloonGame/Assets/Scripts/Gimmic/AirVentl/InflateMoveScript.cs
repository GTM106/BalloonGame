using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InflateMoveScript : AirVentInteractable
{
    [Header("�i�s�����̍��W")]
    [SerializeField] Transform targetPoint;
    [Header("�M�~�b�N�����̈ʒu�ɖ߂邩�ǂ���")]
    [SerializeField] bool rollBack = default!;
    [Header("�M�~�b�N���ő�ړ��������猳�̈ʒu�ɖ߂邩�ǂ���")]
    [SerializeField] bool arrivalBack = default!;
    [Header("�v�b�V�����ɐi�ޒP��")]
    [SerializeField, Min(0f)] float moveSpeed = default!;
    [Header("�M�~�b�N�����̈ʒu�ɖ߂�P��")]
    [SerializeField] float returnSpeed = default!;
    [Header("�M�~�b�N�����̈ʒu�ɖ߂�n�߂�܂ł̒P��")]
    [SerializeField] float stopTime = default!;

    Vector3 initialPoint;

    bool hasMovedFromStartPosition = false;
    float rollBackTime = 0f;
    const float MIN_DISTANCE = 0.01f;

    private void Awake()
    {
        initialPoint = transform.position;
    }

    private void Update()
    {
        if (rollBackTime >= stopTime)
        {
            RollBack();
        }
    }
    private void FixedUpdate()
    {
        RoleBackChecker();
    }

    private void RoleBackChecker()
    {
        if (!rollBack) return;
        if (!hasMovedFromStartPosition) return;

        //�߂�܂ł̎��Ԍv��
        rollBackTime += Time.fixedDeltaTime;
    }

    private void RollBack()
    {
        //�����n�_�܂ňړ�
        transform.position = Vector3.MoveTowards(transform.position, initialPoint, returnSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, initialPoint) <= MIN_DISTANCE)
        {
            hasMovedFromStartPosition = false;
            rollBackTime = 0f;
        }
    }

    public override void Interact()
    {
        //�w��n�_�ֈړ�
        transform.position = Vector3.MoveTowards(transform.position, targetPoint.position, moveSpeed * Time.deltaTime);

        hasMovedFromStartPosition = true;
        rollBackTime = 0f;

        if (Vector3.Distance(transform.position, targetPoint.position) <= MIN_DISTANCE)
        {
            if (!arrivalBack)
            {
                //�M�~�b�N���ő�ړ������ɓ��B�����ꍇRollBack���I�t�ɂ���
                rollBack = false;
            }
        }
    }
}