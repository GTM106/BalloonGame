using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InflateMoveScript : AirVentInteractable
{
    [Header("進行方向の座標")]
    [SerializeField] Transform targetPoint;

    //変数名変えろ
    [Header("ギミックが元の位置に戻るかどうか")]
    [SerializeField] bool rollBack = default!;
    [Header("ギミックが最大移動距離から元の位置に戻るかどうか")]
    [SerializeField] bool arrivalBack = default!;
    [Header("プッシュ時に進む単位")]
    [SerializeField] float moveSpeed = default!;
    [Header("ギミックが元の位置に戻る単位")]
    [SerializeField] float returnSpeed = default!;
    [Header("ギミックが元の位置に戻り始めるまでの単位")]
    [SerializeField] float stopTime = default!;

    Vector3 initialPoint = Vector3.zero;

    //変数名変えろ
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