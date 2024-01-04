using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Serialization;

public interface IRedSharkState
{
    public enum E_State
    {
        BeforePartrol,
        Patrol,
        Discovery,
        Targeting,
        Chase,
        Fall,
        Down,
        Death,
        BeforeWarp,
        Warp,

        MAX,

        Unchanged,
    }

    E_State Initialize(RedSharkController parent);
    E_State Update(RedSharkController parent);
    E_State FixedUpdate(RedSharkController parent);
}

public class RedSharkController : AirVentInteractable, IHittable
{
    [Header("追跡対象")]
    [SerializeField, Required] GameObject player = default!;
    [Header("移動する範囲")]
    [SerializeField, Required] GameObject partrolRange = default!;
    [Header("巡回時の最大移動距離")]
    [SerializeField, Min(0f)] float _maxPatrolDistance = default!;
    [Header("巡回時間[sec]")]
    [SerializeField, Min(0f)] float _patrolDuration = default!;
    [Header("巡回時方向転換時間[sec]")]
    [SerializeField, Min(0f)] float _patrolRotationDuration = default!;
    [Header("追跡時方向転換時間[sec]")]
    [SerializeField, Min(0f)] float _chaseRotationDuration = default!;
    [Header("追跡開始時間[sec]")]
    [SerializeField, Min(0f)] float _chaseStartTime = default!;
    [Header("追跡速度")]
    [SerializeField, Min(0f)] float chaseSpeed = default!;
    [Header("追跡時間")]
    [SerializeField, Min(0f)] float chaseTime = default!;
    [Header("ダウン時間")]
    [SerializeField, Min(0f)] float downTime = default!;
    [Header("攻撃のリキャストタイム")]
    [SerializeField, Min(0f)] double attackRecastTime = default!;
    [Header("ターゲットにヒットしたあと、再度追跡できるまでの時間[sec]")]
    [SerializeField, Min(0f)] double _cooldownForReacquisitionDuration = default!;
    [Header("ライフ")]
    [SerializeField, Min(0)] int hpMAX = default!;
    [Header("膨張率")]
    [SerializeField, Min(0)] int expansionValue = default!;
    [Header("倒した際の得点")]
    [SerializeField, Min(0)] int itemValue = default!;

    [SerializeField] GameObject movePos = default!;
    [SerializeField] SphereCollider airRange = default!;
    [SerializeField, Required] Rigidbody _rigidbody = default!;
    [SerializeField, Required] BreakBlockContorller breakBlockContorller = default!;
    [SerializeField, Required] CollectibleScript collectibleScript = default!;
    [SerializeField, Required] OnSetActive onSetActive = default!;
    [SerializeField, Required] AudioSource _chasingAudioSource = default!;
    [SerializeField, Required] AudioSource _discoveryAudioSource = default!;
    [SerializeField, Required] AudioSource _targetingAudioSource = default!;
    [SerializeField, Required] AudioSource _clashAudioSource = default!;
    [SerializeField, Required] AudioSource _downAudioSource = default!;
    [SerializeField, Required] AudioSource _deathAudioSource = default!;
    [SerializeField, Required] ParticleSystem _chargeBlueAttackEffect = default!;
    [SerializeField, Required] ParticleSystem _chargeStartBlueAttackEffect = default!;
    [SerializeField, Required] ParticleSystem _deathEffect = default!;
    [SerializeField, Required] PlayableDirector startAttackEffect = default!;
    [SerializeField, Required] PlayableDirector endAttackEffect = default!;
    [SerializeField, Required] PlayerGameOverEvent _gameOverEvent = default!;
    [SerializeField] AnimationChanger<E_Shark> _animationChanger = default!;

    private bool _wasHitPlayer = false;
    private bool _isHitNotBreakBlock = false;
    private bool _isInChaseRange = false;
    private bool pushCheck = false;
    private bool chaseFinished = false;
    private bool _isSharkInPatrolRange = false;
    private int damageCount = 0;

    Transform _transform;
    Vector3  _sharkScale;
    // 状態管理
    IRedSharkState.E_State _currentState = IRedSharkState.E_State.BeforePartrol;
    static readonly IRedSharkState[] states = new IRedSharkState[(int)IRedSharkState.E_State.MAX]
    {
        new BeforePartrolState(),
        new PartrolState(),
        new DiscoveryState(),
        new TargetingState(),
        new ChaseState(),
        new FallState(),
        new DownState(),
        new DeathState(),
        new BeforeWarpState(),
        new WarpState(),
    };

    class BeforePartrolState : IRedSharkState
    {
        float _elapsedTime;

        public IRedSharkState.E_State Initialize(RedSharkController parent)
        {
            _elapsedTime = 0f;

            //パトロール続行可能状態でなければワープさせる
            if (!parent.CanContinuePatrol()) return IRedSharkState.E_State.BeforeWarp;

            return IRedSharkState.E_State.Unchanged;
        }

        public IRedSharkState.E_State Update(RedSharkController parent)
        {
            return IRedSharkState.E_State.Unchanged;
        }

        public IRedSharkState.E_State FixedUpdate(RedSharkController parent)
        {
            _elapsedTime += Time.fixedDeltaTime;
            if (_elapsedTime >= parent._patrolRotationDuration) return IRedSharkState.E_State.Patrol;

            parent.RotateToMovePos(_elapsedTime);

            return IRedSharkState.E_State.Unchanged;
        }
    }

    class PartrolState : IRedSharkState
    {
        float _elapsedTime;

        public IRedSharkState.E_State Initialize(RedSharkController parent)
        {
            _elapsedTime = 0f;

            parent._animationChanger.ChangeAnimation(E_Shark.AN04_Swim);
            return IRedSharkState.E_State.Unchanged;
        }

        public IRedSharkState.E_State Update(RedSharkController parent)
        {
            return IRedSharkState.E_State.Unchanged;
        }

        public IRedSharkState.E_State FixedUpdate(RedSharkController parent)
        {
            //プレイヤーにヒットしたあとでなく
            if (!parent._wasHitPlayer)
            {
                //追跡範囲内なら発見する
                if (parent._isInChaseRange) return IRedSharkState.E_State.Discovery;
            }

            _elapsedTime += Time.deltaTime;

            if (_elapsedTime >= parent._patrolDuration)
            {
                //時間経過したら再度パトロール
                return IRedSharkState.E_State.BeforePartrol;
            }

            parent.Patrol();

            return IRedSharkState.E_State.Unchanged;
        }
    }

    class DiscoveryState : IRedSharkState
    {
        float _elapsedTime;

        public IRedSharkState.E_State Initialize(RedSharkController parent)
        {
            _elapsedTime = 0f;

            SoundManager.Instance.PlaySE(parent._discoveryAudioSource, SoundSource.SE062_SharkDiscovery);
            parent._animationChanger.ChangeAnimation(E_Shark.AN04_discovery);

            return IRedSharkState.E_State.Unchanged;
        }

        public IRedSharkState.E_State Update(RedSharkController parent)
        {
            return IRedSharkState.E_State.Unchanged;
        }

        public IRedSharkState.E_State FixedUpdate(RedSharkController parent)
        {
            //発見時の回転時間分待機したら移行
            if (_elapsedTime >= parent._chaseRotationDuration) return IRedSharkState.E_State.Targeting;

            _elapsedTime += Time.fixedDeltaTime;
            parent.DiscoveryRotation(_elapsedTime);

            return IRedSharkState.E_State.Unchanged;
        }
    }

    class TargetingState : IRedSharkState
    {
        float _elapsedTime;

        public IRedSharkState.E_State Initialize(RedSharkController parent)
        {
            _elapsedTime = 0f;

            //エフェクト時間の設定
            var main = parent._chargeBlueAttackEffect.main;
            main.duration = parent._chaseStartTime;
            main = parent._chargeStartBlueAttackEffect.main;
            main.duration = parent._chaseStartTime;

            //エフェクトの再生
            parent._chargeBlueAttackEffect.Play();
            parent._chargeStartBlueAttackEffect.Play();

            //貯めSE
            SoundManager.Instance.PlaySE(parent._targetingAudioSource, SoundSource.SE061_SharkTargeting);
            return IRedSharkState.E_State.Unchanged;
        }

        public IRedSharkState.E_State Update(RedSharkController parent)
        {
            return IRedSharkState.E_State.Unchanged;
        }

        public IRedSharkState.E_State FixedUpdate(RedSharkController parent)
        {
            if (parent._wasHitPlayer)
            {
                parent._chargeBlueAttackEffect.Stop();
                parent._chargeStartBlueAttackEffect.Stop();
                SoundManager.Instance.StopSE(parent._targetingAudioSource);

                return IRedSharkState.E_State.BeforePartrol;
            }

            _elapsedTime += Time.deltaTime;

            if (_elapsedTime >= parent._chaseStartTime)
            {
                parent._chargeBlueAttackEffect.Stop();
                parent._chargeStartBlueAttackEffect.Stop();
                SoundManager.Instance.StopSE(parent._targetingAudioSource);

                return IRedSharkState.E_State.Chase;
            }

            return IRedSharkState.E_State.Unchanged;
        }
    }

    class ChaseState : IRedSharkState
    {
        public IRedSharkState.E_State Initialize(RedSharkController parent)
        {
            parent.StartChase();

            return IRedSharkState.E_State.Unchanged;
        }

        public IRedSharkState.E_State Update(RedSharkController parent)
        {
            return IRedSharkState.E_State.Unchanged;
        }

        public IRedSharkState.E_State FixedUpdate(RedSharkController parent)
        {
            if (parent._isHitNotBreakBlock)
            {
                parent.startAttackEffect.Stop();
                SoundManager.Instance.StopSE(parent._chasingAudioSource);

                return IRedSharkState.E_State.Fall;
            }

            if (parent.chaseFinished)
            {
                SoundManager.Instance.StopSE(parent._chasingAudioSource);
                parent.chaseFinished = false;

                //チェイス終了時、チェイス範囲内にいるなら発見
                //そうでないなら巡回する
                return parent._isInChaseRange ? IRedSharkState.E_State.Discovery : IRedSharkState.E_State.BeforePartrol;
            }

            if (parent._wasHitPlayer) return IRedSharkState.E_State.BeforePartrol;

            return IRedSharkState.E_State.Unchanged;
        }
    }

    class FallState : IRedSharkState
    {
        public IRedSharkState.E_State Initialize(RedSharkController parent)
        {
            parent._rigidbody.useGravity = true;
            parent._animationChanger.ChangeAnimation(E_Shark.AN04_Fall);

            return IRedSharkState.E_State.Unchanged;
        }

        public IRedSharkState.E_State Update(RedSharkController parent)
        {
            return IRedSharkState.E_State.Unchanged;
        }

        public IRedSharkState.E_State FixedUpdate(RedSharkController parent)
        {
            if (parent.Fall())
            {
                return IRedSharkState.E_State.Down;
            }

            return IRedSharkState.E_State.Unchanged;
        }
    }

    class DownState : IRedSharkState
    {
        float _elapsedTime;

        public IRedSharkState.E_State Initialize(RedSharkController parent)
        {
            _elapsedTime = 0f;
            parent.StartDown();
            return IRedSharkState.E_State.Unchanged;
        }

        public IRedSharkState.E_State Update(RedSharkController parent)
        {
            if (parent.pushCheck)
            {
                SoundManager.Instance.StopSE(parent._downAudioSource);
                return IRedSharkState.E_State.Death;
            }

            return IRedSharkState.E_State.Unchanged;
        }

        public IRedSharkState.E_State FixedUpdate(RedSharkController parent)
        {
            _elapsedTime += Time.fixedDeltaTime;

            if (_elapsedTime >= parent.downTime)
            {
                parent.FinishDown();
                return IRedSharkState.E_State.BeforePartrol;
            }

            return IRedSharkState.E_State.Unchanged;
        }
    }

    class DeathState : IRedSharkState
    {
        const float DeadAnimationTime = 1.5f;
        float _elapsedTime;
        float _airRangeRadius;
        bool damage;


        public IRedSharkState.E_State Initialize(RedSharkController parent)
        {
            _elapsedTime = 0f;

            damage = parent.DamageHP();
            parent._animationChanger.ChangeAnimation(E_Shark.AN04_Dead);

            _airRangeRadius = parent.airRange.radius;
            //オブジェクト破壊前に確実に空気栓の範囲外にする
            parent.airRange.radius = 0;

            return IRedSharkState.E_State.Unchanged;
        }

        public IRedSharkState.E_State Update(RedSharkController parent)
        {
            return IRedSharkState.E_State.Unchanged;
        }

        public IRedSharkState.E_State FixedUpdate(RedSharkController parent)
        {
            _elapsedTime += Time.fixedDeltaTime;

            if (_elapsedTime > DeadAnimationTime && damage)
            {
                SoundManager.Instance.PlaySE(parent._deathAudioSource, SoundSource.SE065_SharkDeath);
                parent.SharkDestroy();
                parent.collectibleScript.Add(parent.itemValue);
                parent.onSetActive.OnObjectTrue();
            }

            if (_elapsedTime < DeadAnimationTime && !damage)
            {
                parent.SharkExpansion();
                parent.airRange.radius = _airRangeRadius;
                return IRedSharkState.E_State.Patrol;
            }

            return IRedSharkState.E_State.Unchanged;
        }
    }

    class BeforeWarpState : IRedSharkState
    {
        public IRedSharkState.E_State Initialize(RedSharkController parent)
        {
            return IRedSharkState.E_State.Unchanged;
        }

        public IRedSharkState.E_State Update(RedSharkController parent)
        {
            return IRedSharkState.E_State.Unchanged;
        }

        public IRedSharkState.E_State FixedUpdate(RedSharkController parent)
        {
            if (parent.IsCompleteAnimationBeforeWarp()) return IRedSharkState.E_State.Warp;

            parent.AnimationBeforeWarp();
            return IRedSharkState.E_State.Unchanged;
        }
    }

    class WarpState : IRedSharkState
    {
        public IRedSharkState.E_State Initialize(RedSharkController parent)
        {
            //ワープさせる
            parent.StartWarp();

            return IRedSharkState.E_State.Unchanged;
        }

        public IRedSharkState.E_State Update(RedSharkController parent)
        {
            return IRedSharkState.E_State.Unchanged;
        }

        public IRedSharkState.E_State FixedUpdate(RedSharkController parent)
        {
            if (parent.IsCompleteAnimationAfterWarp())
            {
                parent._transform.localScale = parent._sharkScale;
                parent._isSharkInPatrolRange = true;
                return IRedSharkState.E_State.BeforePartrol;
            }

            parent.AnimationAfterWarp();
            return IRedSharkState.E_State.Unchanged;
        }
    }

    private void InitializeState()
    {
        var nextState = states[(int)_currentState].Initialize(this);

        if (nextState != IRedSharkState.E_State.Unchanged)
        {
            _currentState = nextState;
            InitializeState();
        }
    }

    private void UpdateState()
    {
        var nextState = states[(int)_currentState].Update(this);

        if (nextState != IRedSharkState.E_State.Unchanged)
        {
            //次の状態に遷移
            _currentState = nextState;
            InitializeState();
        }
    }

    private void FixedUpdateState()
    {
        var nextState = states[(int)_currentState].FixedUpdate(this);

        if (nextState != IRedSharkState.E_State.Unchanged)
        {
            //次の状態に遷移
            _currentState = nextState;
            InitializeState();
        }
    }

    private void Awake()
    {
        _transform = transform;
        _sharkScale = transform.localScale;

        InitializeState();
    }

    private void Update()
    {
        UpdateState();
        Debug.Log(_currentState);
    }

    private void FixedUpdate()
    {
        FixedUpdateState();
    }

    public void OnEnter(Collider playerCollider, BalloonState balloonState)
    {
        //一行にまとめられますが、変更が容易なためこうします
        if (_currentState == IRedSharkState.E_State.Fall) return;
        if (_currentState == IRedSharkState.E_State.Down) return;
        if (_currentState == IRedSharkState.E_State.Death) return;

        //ヒット時のクールダウンを開始
        StartCooldownForReacquisition();

        _gameOverEvent.GameOver();
    }

    public void OnStay(Collider playerCollider, BalloonState balloonState)
    {
    }

    public void OnExit(Collider playerCollider, BalloonState balloonState)
    {
    }

    public void OnChaseCheckRangeCollisionEnter()
    {
        _isInChaseRange = true;
    }

    public void OnChaseCheckRangeCollisionExit()
    {
        _isInChaseRange = false;
    }

    public void OnRangeCheckRangeCollisionEnter()
    {
        _isSharkInPatrolRange = true;
    }

    public void OnRangeCheckRangeCollisionExit()
    {
        _isSharkInPatrolRange = false;
    }

    public void SetHitNotBreakBlock()
    {
        if (_currentState == IRedSharkState.E_State.Chase)
        {
            _isHitNotBreakBlock = true;
            SoundManager.Instance.PlaySE(_clashAudioSource, SoundSource.SE063_SharkClash);
        }
    }

    public void StartBlockBreak(GameObject breakableBlock)
    {
        if (_currentState == IRedSharkState.E_State.Chase)
        {
            breakBlockContorller.BlockBreak(breakableBlock);
        }
    }

    private async void StartCooldownForReacquisition()
    {
        var token = this.GetCancellationTokenOnDestroy();

        _wasHitPlayer = true;

        await UniTask.Delay(TimeSpan.FromSeconds(_cooldownForReacquisitionDuration), false, PlayerLoopTiming.FixedUpdate, token);

        _wasHitPlayer = false;
    }

    private void StartWarp()
    {
        Warp();
    }

    private void StartChase()
    {
        _animationChanger.ChangeAnimation(E_Shark.AN04_Attack);
        SoundManager.Instance.PlaySE(_chasingAudioSource, SoundSource.SE060_SharkAttack);
        startAttackEffect.Play();

        Chase(destroyCancellationToken).Forget();
    }

    private void StartDown()
    {
        SoundManager.Instance.PlaySE(_downAudioSource, SoundSource.SE064_SharkDown);

        _animationChanger.ChangeAnimation(E_Shark.AN04_Down);
    }

    private void FinishDown()
    {
        SoundManager.Instance.StopSE(_downAudioSource);

        _rigidbody.useGravity = false;
        _isHitNotBreakBlock = false;

        _animationChanger.ChangeAnimation(E_Shark.AN04_Swim);
    }

    private bool CanContinuePatrol()
    {
        if (!_isSharkInPatrolRange) return false;

        int wrapCount = 0;
        bool isObstacleAhead = true;

        while (isObstacleAhead)
        {
            //発見するまでfalseにする
            isObstacleAhead = false;

            Vector3 randomPosition = _transform.position + UnityEngine.Random.onUnitSphere * _maxPatrolDistance;

            movePos.transform.position = randomPosition;

            Vector3 direction = randomPosition - _transform.position;

            if (Physics.CapsuleCast(_transform.position, randomPosition, 1.0f, direction, out RaycastHit hit))
            {
                if (hit.collider.gameObject != player)
                {
                    wrapCount++;
                    isObstacleAhead = true;
                }
            }

            //ミス回数が500回程度が処理回数的に良いです
            if (wrapCount == 500)
            {
                return false;
            }
        }

        return true;
    }

    private void RotateToMovePos(float elapsedTime)
    {
        _transform.rotation = Quaternion.Slerp(_transform.rotation, Quaternion.LookRotation(movePos.transform.position - _transform.position), elapsedTime);
    }

    private void Patrol()
    {
        _transform.position = Vector3.MoveTowards(_transform.position, movePos.transform.position, _maxPatrolDistance * Time.deltaTime);
    }

    private bool IsCompleteAnimationBeforeWarp()
    {
        return _transform.localScale.x < 0.1f;
    }

    private void AnimationBeforeWarp()
    {
        Vector3 currentScale = _transform.localScale;

        float scaleDownSize = 0.5f * Time.fixedDeltaTime;

        currentScale -= Vector3.one * scaleDownSize;

        _transform.localScale = currentScale;
    }

    private bool IsCompleteAnimationAfterWarp()
    {
        return _transform.localScale.x >= _sharkScale.x;
    }

    private void AnimationAfterWarp()
    {
        Vector3 currentScale = _transform.localScale;

        float scaleUpSize = 0.7f * Time.deltaTime;

        currentScale += Vector3.one * scaleUpSize;

        _transform.localScale = currentScale;
    }

    private void Warp()
    {
        _transform.position = partrolRange.transform.position;
        _transform.localRotation = Quaternion.identity;
    }

    private void DiscoveryRotation(float elapsedTime)
    {
        _transform.rotation = Quaternion.Slerp(_transform.rotation, Quaternion.LookRotation(player.transform.position - _transform.position), elapsedTime);
    }

    private bool Fall()
    {
        Vector3 downPos = _transform.position;
        downPos.y -= 1f;

        Vector3 forward = Vector3.forward * 0.5f;

        return Physics.CapsuleCast(_transform.position + forward, _transform.position - forward, 6.0f, Vector3.down);
    }

    private async UniTask Chase(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        float elapsedChaseTime = 0f;

        while (elapsedChaseTime < chaseTime)
        {
            await UniTask.Yield(PlayerLoopTiming.LastFixedUpdate, token);

            _rigidbody.AddForce(_transform.forward * chaseSpeed, ForceMode.Force);

            elapsedChaseTime += Time.deltaTime;

            if (_isHitNotBreakBlock)
            {
                startAttackEffect.Stop();
                _rigidbody.velocity = Vector3.zero;
                return;
            }
        }
        _rigidbody.velocity = Vector3.zero;
        startAttackEffect.Stop();

        endAttackEffect.Play();
        SoundManager.Instance.StopSE(_chasingAudioSource);
        _animationChanger.ChangeAnimation(E_Shark.AN04_Swim);
        await UniTask.Delay(TimeSpan.FromSeconds(attackRecastTime), false, PlayerLoopTiming.FixedUpdate, token);

        endAttackEffect.Stop();

        chaseFinished = true;
    }

    private void SharkDestroy()
    {
        foreach (Transform child in _transform)
        {
            Destroy(child.gameObject);
        }

        gameObject.SetActive(false);
    }

    private bool DamageHP()
    {
        damageCount++;

        if(damageCount == hpMAX)
        {
            return true;
        }

        return false;
    }

    private void SharkExpansion()
    {
        Vector3 currentScale = _transform.localScale;

        float scaleDownSize = expansionValue * Time.fixedDeltaTime;

        currentScale -= Vector3.one * scaleDownSize;

        _transform.localScale = currentScale;

        Debug.Log(transform.localScale);
    }

    public override void Interact()
    {
        if (_currentState == IRedSharkState.E_State.Down)
        {
            pushCheck = true;
        }
    }
}