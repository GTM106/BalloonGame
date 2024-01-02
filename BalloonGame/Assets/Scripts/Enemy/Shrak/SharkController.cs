using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using TM.Easing;
using TM.Easing.Management;
using UnityEngine;
using UnityEngine.Serialization;

public interface ISharkState
{
    public enum E_State
    {
        Partrol,
        Discovery,
        Targeting,
        Chase,
        Fall,
        Down,
        Dead,

        MAX,

        Unchanged,
    }

    E_State Initialize(SharkController parent);
    E_State Update(SharkController parent);
    E_State FixedUpdate(SharkController parent);
}
public class SharkController : MonoBehaviour, IHittable
{
    [Header("追跡対象")]
    [SerializeField] GameObject player = default!;
    [Header("移動する範囲")]
    [SerializeField] GameObject partrolRange = default!;
    [Header("巡回時の最大移動距離")]
    [SerializeField, Min(0f)] float maxDuration = default!;
    [Header("巡回速度")]
    [SerializeField, Min(0f)] float patrolSpeed = default!;
    [Header("方向転換速度")]
    [SerializeField, Min(0f)] float rotationSpeed = default!;
    [Header("移動するスパン")]
    [SerializeField, Min(0f)] float movementStartSpan = default!;
    [Header("追跡開始時間")]
    [SerializeField, Min(0f)] float chaseStartTime = default!;
    [Header("追跡速度")]
    [SerializeField, Min(0f)] float chaseSpeed = default!;
    [Header("追跡時間")]
    [SerializeField, Min(0f)] float chaseTime = default!;
    [Header("ダウン時の再追跡までの時間")]
    [SerializeField, Min(0f)] float waitTimeDeah = default!;
    [Header("ダウン時間")]
    [SerializeField, Min(0f)] float downTime = default!;
    [Header("攻撃のリキャストタイム")]
    [SerializeField, Min(0f)] double attackRecastTime = default!;
    [Header("巡回時のイージングタイプ選択")]
    [SerializeField] EaseType partrolEaseType = EaseType.OutCubic;
    [SerializeField] float partrolOvershootOrAmplitude = default!;
    [SerializeField] float partrolPeriod = default!;

    [SerializeField] GameObject warpGate = default!;
    [SerializeField] Rigidbody rigidbody = default!;
    [SerializeField, Required] BreakBlockContorller breakBlockContorller = default!;
    [SerializeField, Required] AudioSource _chasingAudioSource = default!;
    [SerializeField, Required] AudioSource _discoveryAudioSource = default!;
    [SerializeField, Required] AudioSource _targetingAudioSource = default!;
    [SerializeField, Required] AudioSource _clashAudioSource = default!;
    [SerializeField, Required] AudioSource _downAudioSource = default!;
    [SerializeField, Required] AudioSource _deathAudioSource = default!;
    [SerializeField, Required] PlayerGameOverEvent _gameOverEvent = default!;
    [SerializeField] AnimationChanger<E_Shark> _animationChanger = default!;

    private bool _isHitPlayer = false;
    private bool _isHitNotBreakBlock = false;
    private bool chaseCheck = false;
    private bool fallStart = false;
    private bool warpStart = false;
    private bool discoveryStart = false;
    private bool chaseFinished = false;
    private bool deathFinished = false;
    private float deadFinished = 0f;
    private bool _isBeyondRange = false;
    private float partrolTimer = 0f;
    private float chaseTimer = 0f;
    private float elapsedLookTime = 0f;

    // 状態管理
    ISharkState.E_State _currentState = ISharkState.E_State.Partrol;
    static readonly ISharkState[] states = new ISharkState[(int)ISharkState.E_State.MAX]
    {
        new PartrolState(),
        new DiscoveryState(),
        new Targeting(),
        new ChaseState(),
        new FallState(),
        new DownState(),
        new DeadState(),
    };

    class PartrolState : ISharkState
    {
        public ISharkState.E_State Initialize(SharkController parent)
        {
            parent._animationChanger.ChangeAnimation(E_Shark.AN04_Swim);

            return ISharkState.E_State.Unchanged;
        }

        public ISharkState.E_State Update(SharkController parent)
        {
            if (parent.warpStart) return ISharkState.E_State.Unchanged;

            if (parent._isHitPlayer) return ISharkState.E_State.Unchanged;

            parent.partrolTimer += Time.deltaTime;

            if (parent.chaseCheck)
            {
                return ISharkState.E_State.Discovery;
            }

            //一定時間経過したらランダムな方向に移動
            if (parent.partrolTimer >= parent.rotationSpeed + parent.patrolSpeed)
            {
                parent.StartPartrol();
                parent.partrolTimer = 0.0f;
            }

            return ISharkState.E_State.Unchanged;
        }

        public ISharkState.E_State FixedUpdate(SharkController parent)
        {
            return ISharkState.E_State.Unchanged;
        }
    }

    class DiscoveryState : ISharkState
    {
        public ISharkState.E_State Initialize(SharkController parent)
        {
            parent.discoveryStart = true;
            SoundManager.Instance.PlaySE(parent._discoveryAudioSource, SoundSource.SE062_SharkDiscovery);
            parent._animationChanger.ChangeAnimation(E_Shark.AN04_discovery);

            return ISharkState.E_State.Unchanged;
        }

        public ISharkState.E_State Update(SharkController parent)
        {
            return ISharkState.E_State.Unchanged;
        }
        public ISharkState.E_State FixedUpdate(SharkController parent)
        {
            if (parent.Discovery())
            {
                return ISharkState.E_State.Targeting;
            }

            return ISharkState.E_State.Unchanged;
        }
    }

    class Targeting : ISharkState
    {
        public ISharkState.E_State Initialize(SharkController parent)
        {
            //貯めSE
            parent.discoveryStart = false;
            SoundManager.Instance.PlaySE(parent._targetingAudioSource, SoundSource.SE061_SharkTargeting);
            return ISharkState.E_State.Unchanged;
        }

        public ISharkState.E_State Update(SharkController parent)
        {
            parent.chaseTimer += Time.deltaTime;

            if (parent.chaseTimer >= parent.chaseStartTime)
            {
                parent.chaseTimer = 0.0f;

                SoundManager.Instance.StopSE(parent._targetingAudioSource);

                return ISharkState.E_State.Chase;
            }
            return ISharkState.E_State.Unchanged;
        }

        public ISharkState.E_State FixedUpdate(SharkController parent)
        {
            return ISharkState.E_State.Unchanged;
        }
    }

    class ChaseState : ISharkState
    {
        public ISharkState.E_State Initialize(SharkController parent)
        {
            parent._animationChanger.ChangeAnimation(E_Shark.AN04_Attack);
            SoundManager.Instance.PlaySE(parent._chasingAudioSource, SoundSource.SE060_SharkAttack);

            parent.StartChase();

            return ISharkState.E_State.Unchanged;
        }

        public ISharkState.E_State Update(SharkController parent)
        {
            if (parent._isHitNotBreakBlock)
            {
                return ISharkState.E_State.Fall;
            }

            if (parent.ShouldStopChasing())
            {
                return ISharkState.E_State.Partrol;
            }

            if (parent.chaseFinished && !parent.chaseCheck)
            {
                parent.chaseFinished = false;
                return ISharkState.E_State.Partrol;
            }

            if (parent.chaseFinished)
            {
                parent.chaseFinished = false;

                return ISharkState.E_State.Discovery;
            }

            return ISharkState.E_State.Unchanged;
        }
        public ISharkState.E_State FixedUpdate(SharkController parent)
        {
            return ISharkState.E_State.Unchanged;
        }
    }

    class FallState : ISharkState
    {
        public ISharkState.E_State Initialize(SharkController parent)
        {
            parent.rigidbody.useGravity = true;
            parent.fallStart = true;
            parent._animationChanger.ChangeAnimation(E_Shark.AN04_Fall);

            return ISharkState.E_State.Unchanged;
        }

        public ISharkState.E_State Update(SharkController parent)
        {
            if (parent.Fall())
            {
                return ISharkState.E_State.Down;
            }

            return ISharkState.E_State.Unchanged;
        }
        public ISharkState.E_State FixedUpdate(SharkController parent)
        {
            return ISharkState.E_State.Unchanged;
        }
    }

    class DownState : ISharkState
    {
        public ISharkState.E_State Initialize(SharkController parent)
        {
            parent.fallStart = false;
            SoundManager.Instance.PlaySE(parent._downAudioSource, SoundSource.SE064_SharkDown);
            parent.DownStart();
            return ISharkState.E_State.Unchanged;
        }

        public ISharkState.E_State Update(SharkController parent)
        {
            if (parent.deathFinished)
            {
                parent.deathFinished = false;
                return ISharkState.E_State.Partrol;
            }

            return ISharkState.E_State.Unchanged;
        }

        public ISharkState.E_State FixedUpdate(SharkController parent)
        {
            return ISharkState.E_State.Unchanged;
        }
    }

    class DeadState : ISharkState
    {
        public ISharkState.E_State Initialize(SharkController parent)
        {
            SoundManager.Instance.PlaySE(parent._deathAudioSource, SoundSource.SE065_SharkDeath);
            parent._animationChanger.ChangeAnimation(E_Shark.AN04_Dead);

            return ISharkState.E_State.Unchanged;
        }

        public ISharkState.E_State Update(SharkController parent)
        {
            parent.deadFinished += Time.deltaTime;

            if (parent.deadFinished > 1f)
            {
                Destroy(parent.transform);
            }

            return ISharkState.E_State.Unchanged;
        }
        public ISharkState.E_State FixedUpdate(SharkController parent)
        {
            return ISharkState.E_State.Unchanged;
        }
    }
    private void InitializeState()
    {
        var nextState = states[(int)_currentState].Initialize(this);

        if (nextState != ISharkState.E_State.Unchanged)
        {
            _currentState = nextState;
            InitializeState();
        }
    }

    private void UpdateState()
    {
        var nextState = states[(int)_currentState].Update(this);

        if (nextState != ISharkState.E_State.Unchanged)
        {
            //次の状態に遷移
            _currentState = nextState;
            InitializeState();
        }
    }

    private void FixedUpdateState()
    {
        var nextState = states[(int)_currentState].FixedUpdate(this);

        if (nextState != ISharkState.E_State.Unchanged)
        {
            //次の状態に遷移
            _currentState = nextState;
            InitializeState();
        }
    }

    void Awake()
    {
        InitializeState();
    }

    void Update()
    {
        UpdateState();
    }

    private void FixedUpdate()
    {
        FixedUpdateState();
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject == partrolRange)
        {
            _isBeyondRange = true;
        }
    }

    public void OnEnter(Collider playerCollider, BalloonState balloonState)
    {
        _gameOverEvent.GameOver();
        _isHitPlayer = true;
    }

    public void OnStay(Collider playerCollider, BalloonState balloonState)
    {
        //DoNothing
    }

    public void OnExit(Collider playerCollider, BalloonState balloonState)
    {
        _isHitPlayer = false;
    }

    public void OnChaseCheckRangeCollsionEnter()
    {
        chaseCheck = true;
    }

    public void OnChaseCheckRangeCollsionExit()
    {
        chaseCheck = false;
    }

    public void SetHitNotBreakBlock()
    {
        if (_currentState == ISharkState.E_State.Chase)
        {
            _isHitNotBreakBlock = true;
            SoundManager.Instance.PlaySE(_clashAudioSource, SoundSource.SE063_SharkClash);
        }
    }

    public void StartBlockBreak(Collider other)
    {
        if (_currentState == ISharkState.E_State.Chase)
        {
            breakBlockContorller.BlockBreak(other);
        }
    }

    private void StartPartrol()
    {
        Partrol(destroyCancellationToken).Forget();
    }

    private void StartWarp()
    {
        Warp(destroyCancellationToken).Forget();
    }

    private void StartChase()
    {
        Chase(destroyCancellationToken).Forget();
    }

    private void DownStart()
    {
        Down(destroyCancellationToken).Forget();
    }

    private void StartNextAttackDelay()
    {
        SoundManager.Instance.StopSE(_chasingAudioSource);
        NextAttackDelay(destroyCancellationToken).Forget();
    }

    async UniTask Partrol(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        int wrapCount = 0;
        float elapsedLookTime = 0f;
        float elapsedMoveTime = 0f;
        bool isObstacleAhead = true;
        Vector3 targetPosition = Vector3.zero;
        Vector3 randomAngle = Vector3.zero;
        while (isObstacleAhead)
        {
            float angle = UnityEngine.Random.Range(0f, 360f);

            randomAngle = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Tan(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * maxDuration;
            targetPosition = transform.position + randomAngle;
            Vector3 direction;

            if (_isBeyondRange)
            {
                direction = targetPosition - transform.position;
            }
            else
            {
                direction = partrolRange.transform.position - transform.position;
            }

            RaycastHit hit;

            if (Physics.CapsuleCast(transform.position, randomAngle, 3.0f, randomAngle, out hit, maxDuration))
            {
                if (hit.collider.gameObject != player)
                {
                    wrapCount++;
                    isObstacleAhead = true;
                }
            }

            if (wrapCount == 100)
            {
                warpStart = true;
                StartWarp();
                return;
            }

            if (discoveryStart) return;
        }

        _isBeyondRange = false;

        while (elapsedLookTime < rotationSpeed)
        {
            await UniTask.Yield(PlayerLoopTiming.LastFixedUpdate, token);

            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(randomAngle - transform.position), elapsedLookTime);

            elapsedLookTime += Time.deltaTime;

            if (discoveryStart) return;
        }

        Vector3 firstPos = transform.position;
        Vector3 offset = targetPosition - transform.position;

        while (elapsedMoveTime < patrolSpeed)
        {
            await UniTask.Yield(PlayerLoopTiming.LastFixedUpdate, token);

            float progress = EasingManager.EaseProgress(easeType: partrolEaseType, elapsedMoveTime, patrolSpeed, partrolOvershootOrAmplitude, partrolPeriod);

            transform.position = firstPos + offset * progress;

            elapsedMoveTime += Time.deltaTime;

            if (discoveryStart) return;
        }
    }

    async UniTask Warp(CancellationToken token)
    {
        while (true)
        {
            await UniTask.Yield(PlayerLoopTiming.LastFixedUpdate, token);

            Vector3 currentScale = transform.localScale;

            currentScale -= new Vector3(0.5f * Time.deltaTime, 0.5f * Time.deltaTime, 0.5f * Time.deltaTime);

            if (currentScale.x <= 0.1f)
            {
                transform.position = partrolRange.transform.position;
                break;
            }

            transform.localScale = currentScale;
        }

        transform.localRotation = Quaternion.Euler(0f, 0f, 0f); ;

        while (true)
        {
            await UniTask.Yield(PlayerLoopTiming.LastFixedUpdate, token);

            Vector3 currentScale = transform.localScale;

            currentScale += new Vector3(0.3f * Time.deltaTime, 0.3f * Time.deltaTime, 0.3f * Time.deltaTime);

            if (currentScale.x >= 1.0f)
            {
                transform.localScale = new Vector3(1f, 1f, 1f);
                warpStart = false;
                break;
            }

            transform.localScale = currentScale;
        }
    }
    private bool Discovery()
    {
        if (elapsedLookTime < rotationSpeed)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(player.transform.position - transform.position), elapsedLookTime);
            elapsedLookTime += Time.deltaTime;

            return false;
        }

        elapsedLookTime = 0;

        return true;
    }

    private bool ShouldStopChasing()
    {
        if (_isHitPlayer)
        {
            StartNextAttackDelay();

            return true;
        }
        return false;
    }

    private bool Fall()
    {
        Vector3 downPos = transform.position;
        downPos.y -= 1f;

        if (Physics.CapsuleCast(transform.position, downPos, 6.0f, Vector3.down, 1.0f))
        {
            return true;
        }

        return false;
    }

    async UniTask Down(CancellationToken token)
    {
        float currentDownTime = 0;

        _animationChanger.ChangeAnimation(E_Shark.AN04_Down);

        while (currentDownTime < downTime)
        {
            await UniTask.Yield(PlayerLoopTiming.LastFixedUpdate, token);
            currentDownTime += Time.deltaTime;
        }

        rigidbody.useGravity = false;
        _isHitNotBreakBlock = false;
        deathFinished = true;

        _animationChanger.ChangeAnimation(E_Shark.AN04_Swim);
    }

    async UniTask Chase(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        float elapsedChaseTime = 0f;

        while (elapsedChaseTime < chaseTime)
        {
            await UniTask.Yield(PlayerLoopTiming.LastFixedUpdate, token);

            rigidbody.AddForce(transform.forward * chaseSpeed, ForceMode.Force);

            elapsedChaseTime += Time.deltaTime;

            if (fallStart) return;
        }

        SoundManager.Instance.StopSE(_chasingAudioSource);
        _animationChanger.ChangeAnimation(E_Shark.AN04_Swim);
        await UniTask.Delay(TimeSpan.FromSeconds(attackRecastTime), false, PlayerLoopTiming.FixedUpdate, token);

        chaseFinished = true;
    }

    async UniTask NextAttackDelay(CancellationToken token)
    {
        float currentWaitTimeDeah = 0f;

        Vector3 direction = (player.transform.position - transform.position).normalized;

        while (currentWaitTimeDeah < waitTimeDeah)
        {
            await UniTask.Yield(PlayerLoopTiming.LastFixedUpdate, token);

            float distanceToMove = patrolSpeed * Time.deltaTime;

            transform.Translate(direction * distanceToMove, Space.World);

            currentWaitTimeDeah += Time.deltaTime;
        }

        _isHitPlayer = false;
    }
}