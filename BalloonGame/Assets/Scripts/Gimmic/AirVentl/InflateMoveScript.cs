using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using TM.Easing;
using TM.Easing.Management;
using UnityEngine;

public interface IInflateMoveState
{
    public enum E_State
    {
        Normal,
        Move,
        RollReturn,

        MAX,

        Unchanged,
    }

    E_State Initialize(InflateMoveScript parent);
    E_State Update(InflateMoveScript parent);
    E_State FixedUpdate(InflateMoveScript parent);
}

public class InflateMoveScript : AirVentInteractable
{
    [Header("進行方向の座標")]
    [SerializeField] List<Transform> targetPoint = default!;
    [Header("ギミックが初期位置に戻るかどうか")]
    [SerializeField] bool rollReturn = default!;
    [Header("ギミックが最大移動距離から元の位置に戻るかどうか")]
    [SerializeField] bool arrivalReturn = default!;
    [Header("ギミックが元の位置に戻る単位")]
    [SerializeField] float returnSpeed = default!;
    [Header("ギミックが元の位置に戻り始めるまでの単位")]
    [SerializeField] float stopTime = default!;
    [Header("ギミックの移動時間")]
    [SerializeField] float moveDuration = default!;
    [Header("イージングタイプの選択")]
    [SerializeField] EaseType easeType = EaseType.OutCubic;
    [SerializeField] float overshootOrAmplitude = default!;
    [SerializeField] float period = default!;

    readonly List<Vector3> targetPos = new();

    int currentTargetPoint = 0;
    float rollBackTime = 0f;
    const float MIN_DISTANCE = 0.01f;
    bool isInteractThisFrame = false;

    #region State
    // 状態管理
    IInflateMoveState.E_State _currentState = IInflateMoveState.E_State.Normal;

    static readonly IInflateMoveState[] states = new IInflateMoveState[(int)IInflateMoveState.E_State.MAX]
    {
        new NormalState(),
        new MoveState(),
        new RollReturnState(),
    };

    class NormalState : IInflateMoveState
    {
        public IInflateMoveState.E_State Initialize(InflateMoveScript parent)
        {
            parent.rollBackTime = 0f;

            return IInflateMoveState.E_State.Unchanged;
        }

        public IInflateMoveState.E_State Update(InflateMoveScript parent)
        {
            if (parent.isInteractThisFrame)
            {
                parent.isInteractThisFrame = false;
                return IInflateMoveState.E_State.Move;
            }

            return IInflateMoveState.E_State.Unchanged;
        }

        public IInflateMoveState.E_State FixedUpdate(InflateMoveScript parent)
        {
            if (parent.IsReachedRoleBackTime())
            {
                return IInflateMoveState.E_State.RollReturn;
            }

            return IInflateMoveState.E_State.Unchanged;
        }
    }

    class MoveState : IInflateMoveState
    {
        float _movingTime;

        public IInflateMoveState.E_State Initialize(InflateMoveScript parent)
        {
            _movingTime = 0f;

            //次の目標地点に進む
            parent.StartNextMove();

            return IInflateMoveState.E_State.Unchanged;
        }

        public IInflateMoveState.E_State Update(InflateMoveScript parent)
        {
            //動いてる最中は追加で動かさない
            if (parent.isInteractThisFrame)
            {
                parent.isInteractThisFrame = false;
            }

            return IInflateMoveState.E_State.Unchanged;
        }

        public IInflateMoveState.E_State FixedUpdate(InflateMoveScript parent)
        {
            //動く時間
            _movingTime += Time.fixedDeltaTime;
            if (_movingTime >= parent.moveDuration)
            {
                return IInflateMoveState.E_State.Normal;
            }

            return IInflateMoveState.E_State.Unchanged;
        }
    }

    class RollReturnState : IInflateMoveState
    {
        public IInflateMoveState.E_State Initialize(InflateMoveScript parent)
        {
            parent.currentTargetPoint--;

            return IInflateMoveState.E_State.Unchanged;
        }

        public IInflateMoveState.E_State Update(InflateMoveScript parent)
        {
            if (parent.isInteractThisFrame)
            {
                parent.isInteractThisFrame = false;
                return IInflateMoveState.E_State.Move;
            }

            return IInflateMoveState.E_State.Unchanged;
        }

        public IInflateMoveState.E_State FixedUpdate(InflateMoveScript parent)
        {
            //等速で元の地点まで順に戻る
            parent.transform.position = Vector3.MoveTowards(parent.transform.position, parent.targetPos[parent.currentTargetPoint], parent.returnSpeed * Time.deltaTime);

            //初期地点まで戻ったか判定
            if (Vector3.Distance(parent.transform.position, parent.targetPos[0]) <= MIN_DISTANCE)
            {
                parent.rollBackTime = 0f;
                return IInflateMoveState.E_State.Normal;
            }

            //中継地点にたどり着いたらその次に進む
            if (parent.transform.position == parent.targetPos[parent.currentTargetPoint])
            {
                parent.currentTargetPoint--;
            }

            return IInflateMoveState.E_State.Unchanged;
        }
    }

    private void InitializeState()
    {
        var nextState = states[(int)_currentState].Initialize(this);

        if (nextState != IInflateMoveState.E_State.Unchanged)
        {
            _currentState = nextState;
            InitializeState();
        }
    }

    private void UpdateState()
    {
        var nextState = states[(int)_currentState].Update(this);

        if (nextState != IInflateMoveState.E_State.Unchanged)
        {
            //次の状態に遷移
            _currentState = nextState;
            InitializeState();
        }
    }

    private void FixedUpdateState()
    {
        var nextState = states[(int)_currentState].FixedUpdate(this);

        if (nextState != IInflateMoveState.E_State.Unchanged)
        {
            //次の状態に遷移
            _currentState = nextState;
            InitializeState();
        }
    }
    #endregion

    private void Awake()
    {
        targetPos.Add(transform.position);

        foreach (var pos in targetPoint)
        {
            targetPos.Add(pos.position);
        }

        InitializeState();
    }

    private void Update()
    {
        UpdateState();
    }

    private void FixedUpdate()
    {
        FixedUpdateState();
    }

    private bool IsReachedRoleBackTime()
    {
        //そもそもロールバックが許可されていないなら戻らない
        if (!rollReturn) return false;

        //最終地点で最終地点到達時のロールバックが許可されていないなら戻らない
        if (!arrivalReturn && currentTargetPoint == targetPos.Count - 1) return false;
        
        //開始地点なら戻らない
        if (currentTargetPoint == 0) return false;

        //戻るまでの時間計測
        rollBackTime += Time.fixedDeltaTime;

        return rollBackTime > stopTime;
    }

    public override void Interact()
    {
        isInteractThisFrame = true;
    }

    private void StartNextMove()
    {
        currentTargetPoint = Mathf.Min(currentTargetPoint + 1, targetPos.Count - 1);

        StartMove(destroyCancellationToken, currentTargetPoint).Forget();
    }

    private async UniTask StartMove(CancellationToken token, int currentTarget)
    {
        token.ThrowIfCancellationRequested();
        float elapsedTime = 0f;
        Vector3 firstPos = transform.position;
        Vector3 offset = targetPos[currentTarget] - transform.position;

        while (elapsedTime < moveDuration)
        {
            await UniTask.Yield(PlayerLoopTiming.LastFixedUpdate, token);

            float progress = EasingManager.EaseProgress(easeType: easeType, elapsedTime, moveDuration, overshootOrAmplitude, period);

            transform.position = firstPos + offset * progress;

            elapsedTime += Time.deltaTime;
        }

        transform.position = targetPos[currentTarget];
    }
}