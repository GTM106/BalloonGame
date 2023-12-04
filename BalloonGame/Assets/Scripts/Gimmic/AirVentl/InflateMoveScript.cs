using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InflateMoveScript : AirVentInteractable
{
    [Header("進行方向の座標")]
    [SerializeField] Transform targetPoint;
    [Header("ギミックが元の位置に戻るかどうか")]
    [SerializeField] bool rollBack = default!;
    [Header("ギミックが最大移動距離から元の位置に戻るかどうか")]
    [SerializeField] bool arrivalBack = default!;
    [Header("プッシュ時に進む単位")]
    [SerializeField, Min(0f)] float moveSpeed = default!;
    [Header("ギミックが元の位置に戻る単位")]
    [SerializeField] float returnSpeed = default!;
    [Header("ギミックが元の位置に戻り始めるまでの単位")]
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

        //戻るまでの時間計測
        rollBackTime += Time.fixedDeltaTime;
    }

    private void RollBack()
    {
        //初期地点まで移動
        transform.position = Vector3.MoveTowards(transform.position, initialPoint, returnSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, initialPoint) <= MIN_DISTANCE)
        {
            hasMovedFromStartPosition = false;
            rollBackTime = 0f;
        }
    }

    public override void Interact()
    {
        //指定地点へ移動
        transform.position = Vector3.MoveTowards(transform.position, targetPoint.position, moveSpeed * Time.deltaTime);

        hasMovedFromStartPosition = true;
        rollBackTime = 0f;

        if (Vector3.Distance(transform.position, targetPoint.position) <= MIN_DISTANCE)
        {
            if (!arrivalBack)
            {
                //ギミックが最大移動距離に到達した場合RollBackをオフにする
                rollBack = false;
            }
        }
    }
}