using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TM.Easing.Management;
using TM.Easing;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using UnityEngine.VFX;
using TMPro;
using System;
using Unity.VisualScripting;

public interface ISharkState
{
    public enum E_State
    {
        Partrol,
        Discovery,
        Targeting,
        Chase,
        Down,

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
    [Header("攻撃のリキャストタイム")]
    [SerializeField, Min(0f)] double attackRecastTime = default!;
    [Header("巡回時のイージングタイプ選択")]
    [SerializeField] EaseType partrolEaseType = EaseType.OutCubic;
    [SerializeField] float partrolOvershootOrAmplitude = default!;
    [SerializeField] float partrolPeriod = default!;
    [Header("追跡時のイージングタイプ選択")]
    [SerializeField] EaseType chaseEaseType = EaseType.OutCubic;
    [SerializeField] float chaseOvershootOrAmplitude = default!;
    [SerializeField] float chasePeriod = default!;

    [SerializeField, Required] BreakBlockContorller breakBlockContorller = default!;
    [SerializeField, Required] AudioSource _chasingAudioSource = default!;
    [SerializeField, Required] AudioSource _targetingAudioSource = default!;
    [SerializeField, Required] PlayerGameOverEvent _gameOverEvent = default!;
    [SerializeField, Required, FormerlySerializedAs("_audioSource")] AudioSource _discoveryAudioSource = default!;
    [SerializeField] AnimationChanger<E_Shark> _animationChanger = default!;


    private bool _isHitPlayer = false;
    private bool chaseCheck = false;
    private bool discoveryCheaker = false;
    private bool chaseFinished = false;
    private bool isBeyondRange = false;
    private float partrolTimer = 0.0f;
    private float chaseTimer = 0.0f;
    private float elapsedLookTime = 0f;

    // 状態管理
    ISharkState.E_State _currentState = ISharkState.E_State.Partrol;
    static readonly ISharkState[] states = new ISharkState[(int)ISharkState.E_State.MAX]
    {
        new PartrolState(),
        new DiscoveryState(),
        new Targeting(),
        new ChaseState(),
        new DownState(),
    };

    class PartrolState : ISharkState
    {
        public ISharkState.E_State Initialize(SharkController parent)
        {
            parent.discoveryCheaker = false;
            parent._animationChanger.ChangeAnimation(E_Shark.AN04_Swim);

            if(parent._isHitPlayer)
            {
                parent.NextAttackDelay(parent.destroyCancellationToken).Forget();
            }

            return ISharkState.E_State.Unchanged;
        }

        public ISharkState.E_State Update(SharkController parent)
        {
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
            parent._animationChanger.ChangeAnimation(E_Shark.AN04_discovery);

            return ISharkState.E_State.Unchanged;
        }

        public ISharkState.E_State Update(SharkController parent)
        {
            if (parent.ShouldStopChasing())
            {
                return ISharkState.E_State.Partrol;
            }

            if (parent.Discovery())
            {
                return ISharkState.E_State.Targeting;
            }

            return ISharkState.E_State.Unchanged;
        }
        public ISharkState.E_State FixedUpdate(SharkController parent)
        {
            return ISharkState.E_State.Unchanged;
        }

    }

    class Targeting : ISharkState
    {
        public ISharkState.E_State Initialize(SharkController parent)
        {
            //貯めSE
            SoundManager.Instance.PlaySE(parent._targetingAudioSource, SoundSource.SE061_SharkTargeting);
            return ISharkState.E_State.Unchanged;
        }

        public ISharkState.E_State Update(SharkController parent)
        {
            parent.chaseTimer += Time.deltaTime;

            if (parent.ShouldStopChasing())
            {
                return ISharkState.E_State.Partrol;
            }

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

    class DownState : ISharkState
    {
        public ISharkState.E_State FixedUpdate(SharkController parent)
        {
            throw new NotImplementedException();
        }

        public ISharkState.E_State Initialize(SharkController parent)
        {
            throw new NotImplementedException();
        }

        public ISharkState.E_State Update(SharkController parent)
        {
            throw new NotImplementedException();
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
        //Debug.Log(_currentState);
    }

    private void FixedUpdate()
    {
        FixedUpdateState();
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
        //DoNothing
    }

    private void StartPartrol()
    {
        Partrol(destroyCancellationToken).Forget();
    }

    private void StartChase()
    {
        Chase(destroyCancellationToken).Forget();
    }

    private void StartNextAttackDelay()
    {
        SoundManager.Instance.StopSE(_chasingAudioSource);
        SoundManager.Instance.StopSE(_discoveryAudioSource);
    }

    public void OnChaseCheckRangeCollsionEnter()
    {
        chaseCheck = true;
    }

    public void OnChaseCheckRangeCollsionExit()
    {
        chaseCheck = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "BreakBlock" && _currentState == ISharkState.E_State.Chase)
        {
            breakBlockContorller.BlockBreak(collision);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject == partrolRange)
        {
            isBeyondRange = true;
        }
    }

    async UniTask Partrol(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        float elapsedLookTime = 0f;
        float elapsedMoveTime = 0f;
        bool isObstacleAhead = true;
        Vector3 targetPosition = Vector3.zero;

        while (isObstacleAhead)
        {
            float angle = UnityEngine.Random.Range(0f, 360f);

            Vector3 randomAngle = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Tan(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * maxDuration;
            targetPosition = transform.position + randomAngle;
            Vector3 direction;

            if (isBeyondRange)
            {
                direction = targetPosition - transform.position;
            }
            else
            {
                direction = partrolRange.transform.position - transform.position;
                Debug.Log("戻る");
            }

            RaycastHit hit;
            isObstacleAhead = false;

            if (Physics.Raycast(transform.position, direction, out hit, maxDuration))
            {
                if (hit.collider.gameObject != player)
                {
                    isObstacleAhead = true;
                }
            }
        }

        isBeyondRange = false;

        while (elapsedLookTime < rotationSpeed)
        {
            await UniTask.Yield(PlayerLoopTiming.LastFixedUpdate, token);

            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetPosition - transform.position), elapsedLookTime);

            elapsedLookTime += Time.deltaTime;

            if (discoveryCheaker) return;
        }

        Vector3 firstPos = transform.position;
        Vector3 offset = targetPosition - transform.position;

        while (elapsedMoveTime < patrolSpeed)
        {
            await UniTask.Yield(PlayerLoopTiming.LastFixedUpdate, token);

            float progress = EasingManager.EaseProgress(easeType: partrolEaseType, elapsedMoveTime, patrolSpeed, partrolOvershootOrAmplitude, partrolPeriod);

            transform.position = firstPos + offset * progress;

            elapsedMoveTime += Time.deltaTime;

            if (discoveryCheaker) return;
        }
    }

    bool Discovery()
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

    async UniTask Chase(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        float elapsedChaseTime = 0f;
        Vector3 firstPos = transform.position;
        Vector3 offset = player.transform.position - transform.position;

        Vector3 direction = (player.transform.position - transform.position).normalized;

        while (elapsedChaseTime < chaseTime)
        {
            await UniTask.Yield(PlayerLoopTiming.LastFixedUpdate, token);

            float distanceToMove = chaseSpeed * Time.deltaTime;

            transform.Translate(direction * distanceToMove, Space.World);

            elapsedChaseTime += Time.deltaTime;
        }

        SoundManager.Instance.StopSE(_chasingAudioSource);
        _animationChanger.ChangeAnimation(E_Shark.AN04_Swim);
        await UniTask.Delay(TimeSpan.FromSeconds(attackRecastTime), false, PlayerLoopTiming.FixedUpdate, token);
        chaseFinished = true;
    }

    async UniTask NextAttackDelay(CancellationToken token)
    {
        float currentWaitTimeDeah = 0f;
        Vector3 offset = player.transform.position - transform.position;

        Vector3 direction = (player.transform.position - transform.position).normalized;

        while (currentWaitTimeDeah < waitTimeDeah)
        {
            float distanceToMove = patrolSpeed * Time.deltaTime;

            transform.Translate(direction * distanceToMove, Space.World);

            currentWaitTimeDeah += Time.deltaTime;
        }
        Debug.Log("再開");
        _isHitPlayer = false;
    }
}