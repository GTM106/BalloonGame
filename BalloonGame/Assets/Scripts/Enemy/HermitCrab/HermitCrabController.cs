using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using UnityEngine.VFX;

public interface IHermitCrabState
{
    public enum E_State
    {
        Partrol,
        Discovery,
        Chase,

        MAX,

        Unchanged,
    }

    E_State Initialize(HermitCrabController parent);
    E_State Update(HermitCrabController parent);
    E_State FixedUpdate(HermitCrabController parent);
}

public class HermitCrabController : MonoBehaviour, IHittable
{
    [Header("Targetを追跡するかどうか")]
    [SerializeField] bool _enableChasing = true;
    [Header("追跡対象")]
    [SerializeField] Transform _target = default!;
    [Header("巡回する地点")]
    [SerializeField] Transform[] _wayPoints = default!;
    [Header("巡回速度")]
    [SerializeField, Min(0f)] float _patrolSpeed = default!;
    [Header("追跡を開始する範囲")]
    [SerializeField, Min(0f)] float _chaseRadius = default!;
    [Header("追跡速度")]
    [SerializeField, Min(0f)] float _chaseSpeed = default!;
    [Header("追跡を行う最大時間")]
    [SerializeField, Min(0f)] float _maxChaseDuration = default!;
    [Header("追跡をやめる距離")]
    [SerializeField, Min(0f)] float _maxChaseDistance = default!;
    [Header("追跡時間を超えた場合に再度追跡するまでのクールタイム")]
    [SerializeField] float _cooldownForReacquisitionDuration = default!;
    [SerializeField, Required] PlayerGameOverEvent _gameOverEvent = default!;
    [SerializeField, Required, FormerlySerializedAs("_audioSource")] AudioSource _discoveryAudioSource = default!;
    [SerializeField, Required] AudioSource _chasingAudioSource = default!;
    [SerializeField, Required] NavMeshAgent _navMeshAgent = default!;
    [SerializeField] AnimationChanger<E_Harmit> _animationChanger = default!;

    int _currentPatrolIndex = 0;
    float _elapsedChaseTime = 0f;
    float _elapsedCoolTime = 0f;

    //再追跡までのクールダウン中かどうか
    bool _isInCooldownForReacquisition = false;

    //発見アニメーション再生中かどうか
    bool _isPlayingDiscoveryAnimation = false;

    //プレイヤーにヒットしたかどうか
    bool _isHitPlayer = false;

    //キャッシュしたTransform
    Transform _transform = default!;

    [SerializeField] VisualEffect FindPlayerEff;

    #region State
    // 状態管理
    IHermitCrabState.E_State _currentState = IHermitCrabState.E_State.Partrol;
    static readonly IHermitCrabState[] states = new IHermitCrabState[(int)IHermitCrabState.E_State.MAX]
    {
        new PartrolState(),
        new DiscoveryState(),
        new ChaseState(),
    };

    class PartrolState : IHermitCrabState
    {
        public IHermitCrabState.E_State Initialize(HermitCrabController parent)
        {
            parent._navMeshAgent.speed = parent._patrolSpeed;
            parent._animationChanger.ChangeAnimation(E_Harmit.AN02_walk);

            return IHermitCrabState.E_State.Unchanged;
        }

        public IHermitCrabState.E_State Update(HermitCrabController parent)
        {
            return IHermitCrabState.E_State.Unchanged;
        }

        public IHermitCrabState.E_State FixedUpdate(HermitCrabController parent)
        {
            //目標地点に到達したか判定を行う
            parent.CheckDestinationArrival();

            //再追跡までのクールダウンを行う
            parent.WaitForCooldownForReacquisition();

            //一行にまとめられますが、可読性のためにこうします。
            if (parent.CanDiscoveryTarget())
            {
                //追跡可能なら追跡。その前に発見する
                return IHermitCrabState.E_State.Discovery;
            }

            return IHermitCrabState.E_State.Unchanged;
        }
    }

    class DiscoveryState : IHermitCrabState
    {
        public IHermitCrabState.E_State Initialize(HermitCrabController parent)
        {
            parent.StartDiscovery();
            return IHermitCrabState.E_State.Unchanged;
        }

        public IHermitCrabState.E_State Update(HermitCrabController parent)
        {
            return IHermitCrabState.E_State.Unchanged;
        }

        public IHermitCrabState.E_State FixedUpdate(HermitCrabController parent)
        {
            if (!parent._isPlayingDiscoveryAnimation) return IHermitCrabState.E_State.Chase;

            return IHermitCrabState.E_State.Unchanged;
        }
    }

    class ChaseState : IHermitCrabState
    {
        public IHermitCrabState.E_State Initialize(HermitCrabController parent)
        {
            parent.StartChasing();

            return IHermitCrabState.E_State.Unchanged;
        }

        public IHermitCrabState.E_State Update(HermitCrabController parent)
        {
            return IHermitCrabState.E_State.Unchanged;
        }

        public IHermitCrabState.E_State FixedUpdate(HermitCrabController parent)
        {
            parent.Chasing();

            if (parent.ShouldStopChasing())
            {
                parent.StopChasing();
                return IHermitCrabState.E_State.Partrol;
            }

            return IHermitCrabState.E_State.Unchanged;
        }
    }

    private void InitializeState()
    {
        var nextState = states[(int)_currentState].Initialize(this);

        if (nextState != IHermitCrabState.E_State.Unchanged)
        {
            _currentState = nextState;
            InitializeState();
        }
    }

    private void UpdateState()
    {
        var nextState = states[(int)_currentState].Update(this);

        if (nextState != IHermitCrabState.E_State.Unchanged)
        {
            //次の状態に遷移
            _currentState = nextState;
            InitializeState();
        }
    }

    private void FixedUpdateState()
    {
        var nextState = states[(int)_currentState].FixedUpdate(this);

        if (nextState != IHermitCrabState.E_State.Unchanged)
        {
            //次の状態に遷移
            _currentState = nextState;
            InitializeState();
        }
    }
    #endregion

    void Awake()
    {
        _navMeshAgent.SetDestination(_wayPoints[0].position);
        _transform = transform;

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

    public void OnEnter(Collider playerCollider, BalloonState balloonState)
    {
        _gameOverEvent.GameOver();

        //プレイヤーに触れたら巡回ルートに戻っていく
        _isHitPlayer = true;
    }

    public void OnExit(Collider playerCollider, BalloonState balloonState)
    {
        //DoNothing
    }

    public void OnStay(Collider playerCollider, BalloonState balloonState)
    {
        //DoNothing
    }

    private void CheckDestinationArrival()
    {
        if (_navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance)
        {
            ArrivedAtDestination();
        }
    }

    private void ArrivedAtDestination()
    {
        SetNextDestination();
    }

    private void SetNextDestination()
    {
        _currentPatrolIndex = (_currentPatrolIndex + 1) % _wayPoints.Length;

        _navMeshAgent.SetDestination(_wayPoints[_currentPatrolIndex].position);
    }

    private void StartCooldownForReacquisition()
    {
        _isInCooldownForReacquisition = true;
        _elapsedCoolTime = 0f;
    }

    private void WaitForCooldownForReacquisition()
    {
        //クールダウン中でないなら処理しない
        if (!_isInCooldownForReacquisition) return;

        //クールダウン経過まで待機
        _elapsedCoolTime += Time.fixedDeltaTime;
        if (_elapsedCoolTime >= _cooldownForReacquisitionDuration)
        {
            FinishCooldownForReacquisition();
        }
    }

    private void FinishCooldownForReacquisition()
    {
        _isInCooldownForReacquisition = false;
    }

    private bool CanDiscoveryTarget()
    {
        //そもそも追跡が許可されていないなら追跡しない
        if (!_enableChasing) return false;

        //Targetの距離に関わらず、再追跡のクールダウン中なら追跡しない
        if (_isInCooldownForReacquisition) return false;

        //一定距離以内なら追跡する
        float distance = Vector3.Distance(_transform.position, _target.position);
        return distance <= _chaseRadius;
    }

    private void StartDiscovery()
    {
        _isPlayingDiscoveryAnimation = true;

        //ターゲットの方向を向かせる
        Vector3 directionToTarget = _target.position - _transform.position;
        directionToTarget.y = 0f;
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
        _transform.rotation = targetRotation;

        //発見時SEの再生
        SoundManager.Instance.PlaySE(_discoveryAudioSource, SoundSource.SE302_HarmitDiscovery);

        //発見時アニメーションの再生。再生後自動でダッシュモーションになります
        _animationChanger.ChangeAnimation(E_Harmit.AN02_discovery);

        //発見時のエフェクトを再生
        FindPlayerEff.Play();

        //その場でアニメーションを再生させる
        _navMeshAgent.destination = _transform.position;
    }

    private void StartChasing()
    {
        _isHitPlayer = false;

        //追跡SEのループ再生
        SoundManager.Instance.PlaySE(_chasingAudioSource, SoundSource.SE011_Hermit_Chase);

        //追跡経過時間を初期化
        _elapsedChaseTime = 0f;

        //追跡スピードを変更
        _navMeshAgent.speed = _chaseSpeed;
    }

    private void Chasing()
    {
        //常にプレイヤーを追い続ける
        _navMeshAgent.destination = _target.position;

        //追跡している時間を加算
        _elapsedChaseTime += Time.deltaTime;
    }

    private void StopChasing()
    {
        //追跡SEのループ再生を停止
        SoundManager.Instance.StopSE(_chasingAudioSource, 0.1f);

        //再追跡可能までのクールダウンを開始する
        StartCooldownForReacquisition();
    }

    private bool ShouldStopChasing()
    {
        return HasChasedForDuration() || HasExceededChaseDistance() || _isHitPlayer;
    }

    private bool HasChasedForDuration()
    {
        return _elapsedChaseTime >= _maxChaseDuration;
    }

    private bool HasExceededChaseDistance()
    {
        float distance = Vector3.Distance(_transform.position, _target.position);
        return distance >= _maxChaseDistance;
    }

    //アニメーターからEventとして呼んでいます
    public void OnFinishDiscoveryAnimation()
    {
        _isPlayingDiscoveryAnimation = false;
    }
}