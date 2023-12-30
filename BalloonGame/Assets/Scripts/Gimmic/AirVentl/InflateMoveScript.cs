using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using TM.Easing;
using TM.Easing.Management;
using UnityEngine;
using UnityEngine.ProBuilder;

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
    [Header("プレイヤー")]
    [SerializeField] GameObject player = default!;
    [Header("進行方向の座標")]
    [SerializeField] List<Transform> targetPoint = default!;
    [Header("ギミックが初期位置に戻るかどうか")]
    [SerializeField] bool rollReturn = default!;
    [Header("ギミックが最大移動距離から元の位置に戻るかどうか")]
    [SerializeField] bool arrivalReturn = default!;
    [Header("プッシュ時に進む単位")]
    [SerializeField, Min(0f)] float moveSpeed = default!;
    [Header("ギミックが元の位置に戻る単位")]
    [SerializeField] float returnSpeed = default!;
    [Header("ギミックが元の位置に戻り始めるまでの単位")]
    [SerializeField] float stopTime = default!;
    [Header("ギミックの移動時間")]
    [SerializeField] float moveDuration = default!;
    [Header("ギミック移動における最後の移動時間")]
    [SerializeField] float lastDuration = default!;
    [Header("イージングタイプの選択")]
    [SerializeField] EaseType easeType = EaseType.OutCubic;
    [SerializeField] float overshootOrAmplitude = default!;
    [SerializeField] float period = default!;

    List<Vector3> targetPos = new List<Vector3>();

    int currentTargetPoint = 1;
    float rollBackTime = 0f;
    const float MIN_DISTANCE = 0.01f;
    bool interactChecker = false;

    #region State
    // 状態管理
    IInflateMoveState.E_State _currentState = IInflateMoveState.E_State.Normal;

    static readonly IInflateMoveState[] states = new IInflateMoveState[(int)IInflateMoveState.E_State.MAX]
    {
        new NormalState(),
        new MoveState(),
        new RollReturn(),
    };
    class NormalState : IInflateMoveState
    {
        public IInflateMoveState.E_State FixedUpdate(InflateMoveScript parent)
        {
            return IInflateMoveState.E_State.Unchanged;
        }

        public IInflateMoveState.E_State Initialize(InflateMoveScript parent)
        {
            return IInflateMoveState.E_State.Unchanged;
        }

        public IInflateMoveState.E_State Update(InflateMoveScript parent)
        {
            if (parent.interactChecker)
            {
                parent.interactChecker = false;
                return IInflateMoveState.E_State.Move;
            }

            return IInflateMoveState.E_State.Unchanged;
        }
    }

    class MoveState : IInflateMoveState
    {
        public IInflateMoveState.E_State Initialize(InflateMoveScript parent)
        {
            parent.Move(parent.destroyCancellationToken, parent.currentTargetPoint);

            if (Vector3.Distance(parent.transform.position, parent.targetPos[parent.targetPos.Count - 1]) <= MIN_DISTANCE) return IInflateMoveState.E_State.Unchanged;

            if (parent.transform.position == parent.targetPos[parent.currentTargetPoint])
            {
                parent.currentTargetPoint++;
            }

            Debug.Log(parent.currentTargetPoint);
            return IInflateMoveState.E_State.Unchanged;
        }

        public IInflateMoveState.E_State Update(InflateMoveScript parent)
        {
            if (parent.interactChecker)
            {
                parent.interactChecker = false;
                return IInflateMoveState.E_State.Move;
            }

            return IInflateMoveState.E_State.Unchanged;
        }

        public IInflateMoveState.E_State FixedUpdate(InflateMoveScript parent)
        {
            if (parent.RollReturnChecker())
            {
                return IInflateMoveState.E_State.RollReturn;
            }

            return IInflateMoveState.E_State.Unchanged;
        }
    }

    class RollReturn : IInflateMoveState
    {
        public IInflateMoveState.E_State Initialize(InflateMoveScript parent)
        {
            return IInflateMoveState.E_State.Unchanged;
        }

        public IInflateMoveState.E_State Update(InflateMoveScript parent)
        {
            if (parent.interactChecker)
            {
                parent.interactChecker = false;
                return IInflateMoveState.E_State.Move;
            }

            return IInflateMoveState.E_State.Unchanged;
        }

        public IInflateMoveState.E_State FixedUpdate(InflateMoveScript parent)
        {
            parent.transform.position = Vector3.MoveTowards(parent.transform.position, parent.targetPos[parent.currentTargetPoint - 1], parent.returnSpeed * Time.deltaTime);

            if (Vector3.Distance(parent.transform.position, parent.targetPos[0]) <= MIN_DISTANCE)
            {
                parent.rollBackTime = 0f;
                return IInflateMoveState.E_State.Normal;
            }

            if (parent.transform.position == parent.targetPos[parent.currentTargetPoint - 1])
            {
                parent.currentTargetPoint--;
                return IInflateMoveState.E_State.RollReturn;
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
    }

    private void Update()
    {
        UpdateState();
    }

    private void FixedUpdate()
    {
        FixedUpdateState();
    }

    private bool RollReturnChecker()
    {
        //戻るまでの時間計測
        rollBackTime += Time.fixedDeltaTime;

        if (rollBackTime > stopTime)
        {
            return true;
        }

        return false;
    }

    public void AttachChild()
    {
        player.transform.parent = transform;
    }

    public void DetachParent()
    {
        player.transform.parent = null;
    }

    public override async void Interact()
    {
        var token = this.GetCancellationTokenOnDestroy();
        rollBackTime = 0f;
        interactChecker = true;
    }

    async UniTask Move(CancellationToken token, int currentTarget)
    {
        token.ThrowIfCancellationRequested();
        float elapsedTime = 0f;

        while (elapsedTime < moveDuration)
        {
            await UniTask.Yield(PlayerLoopTiming.LastFixedUpdate, token);

            float progress = EasingManager.EaseProgress(easeType: easeType, elapsedTime, moveDuration, overshootOrAmplitude, period);

            transform.position = Vector3.MoveTowards(transform.position, targetPos[currentTarget], progress * Time.deltaTime);

            elapsedTime += Time.deltaTime;
        }
    }
}