using Cysharp.Threading.Tasks;
using System;
using System.Net;
using System.Security.Cryptography;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Serialization;

public interface ISharkState
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

    E_State Initialize(SharkController parent);
    E_State Update(SharkController parent);
    E_State FixedUpdate(SharkController parent);
}

public class SharkController : AirVentInteractable, IHittable
{
    [Header("�ǐՑΏ�")]
    [SerializeField, Required] GameObject player = default!;
    [Header("�ړ�����͈�")]
    [SerializeField, Required] GameObject partrolRange = default!;
    [Header("���񎞂̍ő�ړ�����")]
    [SerializeField, Min(0f)] float _maxPatrolDistance = default!;
    [Header("���񎞊�[sec]")]
    [SerializeField, Min(0f)] float _patrolDuration = default!;
    [Header("���񎞕����]������[sec]")]
    [SerializeField, Min(0f)] float _patrolRotationDuration = default!;
    [Header("�ǐՎ������]������[sec]")]
    [SerializeField, Min(0f)] float _chaseRotationDuration = default!;
    [Header("�ǐՊJ�n����[sec]")]
    [SerializeField, Min(0f)] float _chaseStartTime = default!;
    [Header("�ǐՑ��x")]
    [SerializeField, Min(0f)] float chaseSpeed = default!;
    [Header("�ǐՎ���")]
    [SerializeField, Min(0f)] float chaseTime = default!;
    [Header("�_�E������")]
    [SerializeField, Min(0f)] float downTime = default!;
    [Header("�U���̃��L���X�g�^�C��")]
    [SerializeField, Min(0f)] double attackRecastTime = default!;
    [Header("�^�[�Q�b�g�Ƀq�b�g�������ƁA�ēx�ǐՂł���܂ł̎���[sec]")]
    [SerializeField, Min(0f)] double _cooldownForReacquisitionDuration = default!;
    [Header("���C�t")]
    [SerializeField, Min(0)] int hpMAX = default!;
    [Header("�c����")]
    [SerializeField, Min(0)] int expansionValue = default!;
    [Header("�|�����ۂ̓��_")]
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
    Vector3 _sharkScale;
    // ��ԊǗ�
    ISharkState.E_State _currentState = ISharkState.E_State.BeforePartrol;
    static readonly ISharkState[] states = new ISharkState[(int)ISharkState.E_State.MAX]
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

    class BeforePartrolState : ISharkState
    {
        float _elapsedTime;

        public ISharkState.E_State Initialize(SharkController parent)
        {
            _elapsedTime = 0f;

            //�p�g���[�����s�\��ԂłȂ���΃��[�v������
            if (!parent.CanContinuePatrol()) return ISharkState.E_State.BeforeWarp;

            return ISharkState.E_State.Unchanged;
        }

        public ISharkState.E_State Update(SharkController parent)
        {
            return ISharkState.E_State.Unchanged;
        }

        public ISharkState.E_State FixedUpdate(SharkController parent)
        {
            _elapsedTime += Time.fixedDeltaTime;
            if (_elapsedTime >= parent._patrolRotationDuration) return ISharkState.E_State.Patrol;

            parent.RotateToMovePos(_elapsedTime);

            return ISharkState.E_State.Unchanged;
        }
    }

    class PartrolState : ISharkState
    {
        float _elapsedTime;

        public ISharkState.E_State Initialize(SharkController parent)
        {
            _elapsedTime = 0f;

            parent._animationChanger.ChangeAnimation(E_Shark.AN04_Swim);
            return ISharkState.E_State.Unchanged;
        }

        public ISharkState.E_State Update(SharkController parent)
        {
            return ISharkState.E_State.Unchanged;
        }

        public ISharkState.E_State FixedUpdate(SharkController parent)
        {
            //�v���C���[�Ƀq�b�g�������ƂłȂ�
            if (!parent._wasHitPlayer)
            {
                //�ǐՔ͈͓��Ȃ甭������
                if (parent._isInChaseRange) return ISharkState.E_State.Discovery;
            }

            _elapsedTime += Time.deltaTime;

            if (_elapsedTime >= parent._patrolDuration)
            {
                //���Ԍo�߂�����ēx�p�g���[��
                return ISharkState.E_State.BeforePartrol;
            }

            parent.Patrol();

            return ISharkState.E_State.Unchanged;
        }
    }

    class DiscoveryState : ISharkState
    {
        float _elapsedTime;

        public ISharkState.E_State Initialize(SharkController parent)
        {
            _elapsedTime = 0f;

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
            //�������̉�]���ԕ��ҋ@������ڍs
            if (_elapsedTime >= parent._chaseRotationDuration) return ISharkState.E_State.Targeting;

            _elapsedTime += Time.fixedDeltaTime;
            parent.DiscoveryRotation(_elapsedTime);

            return ISharkState.E_State.Unchanged;
        }
    }

    class TargetingState : ISharkState
    {
        float _elapsedTime;

        public ISharkState.E_State Initialize(SharkController parent)
        {
            _elapsedTime = 0f;

            //�G�t�F�N�g���Ԃ̐ݒ�
            var main = parent._chargeBlueAttackEffect.main;
            main.duration = parent._chaseStartTime;
            main = parent._chargeStartBlueAttackEffect.main;
            main.duration = parent._chaseStartTime;

            //�G�t�F�N�g�̍Đ�
            parent._chargeBlueAttackEffect.Play();
            parent._chargeStartBlueAttackEffect.Play();

            //����SE
            SoundManager.Instance.PlaySE(parent._targetingAudioSource, SoundSource.SE061_SharkTargeting);
            return ISharkState.E_State.Unchanged;
        }

        public ISharkState.E_State Update(SharkController parent)
        {
            return ISharkState.E_State.Unchanged;
        }

        public ISharkState.E_State FixedUpdate(SharkController parent)
        {
            if (parent._wasHitPlayer)
            {
                parent._chargeBlueAttackEffect.Stop();
                parent._chargeStartBlueAttackEffect.Stop();
                SoundManager.Instance.StopSE(parent._targetingAudioSource);

                return ISharkState.E_State.BeforePartrol;
            }

            _elapsedTime += Time.deltaTime;

            if (_elapsedTime >= parent._chaseStartTime)
            {
                parent._chargeBlueAttackEffect.Stop();
                parent._chargeStartBlueAttackEffect.Stop();
                SoundManager.Instance.StopSE(parent._targetingAudioSource);

                return ISharkState.E_State.Chase;
            }

            return ISharkState.E_State.Unchanged;
        }
    }

    class ChaseState : ISharkState
    {
        public ISharkState.E_State Initialize(SharkController parent)
        {
            parent.StartChase();

            return ISharkState.E_State.Unchanged;
        }

        public ISharkState.E_State Update(SharkController parent)
        {
            return ISharkState.E_State.Unchanged;
        }

        public ISharkState.E_State FixedUpdate(SharkController parent)
        {
            if (parent._isHitNotBreakBlock)
            {
                parent.startAttackEffect.Stop();
                SoundManager.Instance.StopSE(parent._chasingAudioSource);

                return ISharkState.E_State.Fall;
            }

            if (parent.chaseFinished)
            {
                SoundManager.Instance.StopSE(parent._chasingAudioSource);
                parent.chaseFinished = false;

                //�`�F�C�X�I�����A�`�F�C�X�͈͓��ɂ���Ȃ甭��
                //�����łȂ��Ȃ珄�񂷂�
                return parent._isInChaseRange ? ISharkState.E_State.Discovery : ISharkState.E_State.BeforePartrol;
            }

            if (parent._wasHitPlayer) return ISharkState.E_State.BeforePartrol;

            return ISharkState.E_State.Unchanged;
        }
    }

    class FallState : ISharkState
    {
        public ISharkState.E_State Initialize(SharkController parent)
        {
            parent._rigidbody.useGravity = true;
            parent._animationChanger.ChangeAnimation(E_Shark.AN04_Fall);

            return ISharkState.E_State.Unchanged;
        }

        public ISharkState.E_State Update(SharkController parent)
        {
            return ISharkState.E_State.Unchanged;
        }

        public ISharkState.E_State FixedUpdate(SharkController parent)
        {
            if (parent.Fall())
            {
                return ISharkState.E_State.Down;
            }

            return ISharkState.E_State.Unchanged;
        }
    }

    class DownState : ISharkState
    {
        float _elapsedTime;

        public ISharkState.E_State Initialize(SharkController parent)
        {
            _elapsedTime = 0f;
            parent.StartDown();
            return ISharkState.E_State.Unchanged;
        }

        public ISharkState.E_State Update(SharkController parent)
        {
            if (parent.pushCheck)
            {
                SoundManager.Instance.StopSE(parent._downAudioSource);
                return ISharkState.E_State.Death;
            }

            return ISharkState.E_State.Unchanged;
        }

        public ISharkState.E_State FixedUpdate(SharkController parent)
        {
            _elapsedTime += Time.fixedDeltaTime;

            if (_elapsedTime >= parent.downTime)
            {
                parent.FinishDown();
                return ISharkState.E_State.BeforePartrol;
            }

            return ISharkState.E_State.Unchanged;
        }
    }

    class DeathState : ISharkState
    {
        const float DeadAnimationTime = 1.5f;
        float _elapsedTime;

        public ISharkState.E_State Initialize(SharkController parent)
        {
            _elapsedTime = 0f;

            parent._animationChanger.ChangeAnimation(E_Shark.AN04_Dead);

            //�I�u�W�F�N�g�j��O�Ɋm���ɋ�C���͈̔͊O�ɂ���
            parent.airRange.radius = 0;

            return ISharkState.E_State.Unchanged;
        }

        public ISharkState.E_State Update(SharkController parent)
        {
            return ISharkState.E_State.Unchanged;
        }

        public ISharkState.E_State FixedUpdate(SharkController parent)
        {
            _elapsedTime += Time.fixedDeltaTime;

            if (_elapsedTime > DeadAnimationTime)
            {
                SoundManager.Instance.PlaySE(parent._deathAudioSource, SoundSource.SE065_SharkDeath);
                parent.SharkDestroy();
                parent.collectibleScript.Add(parent.itemValue);
                parent.onSetActive.OnObjectTrue();
            }

            return ISharkState.E_State.Unchanged;
        }
    }

    class BeforeWarpState : ISharkState
    {
        public ISharkState.E_State Initialize(SharkController parent)
        {
            return ISharkState.E_State.Unchanged;
        }

        public ISharkState.E_State Update(SharkController parent)
        {
            return ISharkState.E_State.Unchanged;
        }

        public ISharkState.E_State FixedUpdate(SharkController parent)
        {
            if (parent.IsCompleteAnimationBeforeWarp()) return ISharkState.E_State.Warp;

            parent.AnimationBeforeWarp();
            return ISharkState.E_State.Unchanged;
        }
    }

    class WarpState : ISharkState
    {
        public ISharkState.E_State Initialize(SharkController parent)
        {
            //���[�v������
            parent.StartWarp();

            return ISharkState.E_State.Unchanged;
        }

        public ISharkState.E_State Update(SharkController parent)
        {
            return ISharkState.E_State.Unchanged;
        }

        public ISharkState.E_State FixedUpdate(SharkController parent)
        {
            if (parent.IsCompleteAnimationAfterWarp())
            {
                parent._transform.localScale = parent._sharkScale;
                parent._isSharkInPatrolRange = true;
                return ISharkState.E_State.BeforePartrol;
            }

            parent.AnimationAfterWarp();
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
            //���̏�ԂɑJ��
            _currentState = nextState;
            InitializeState();
        }
    }

    private void FixedUpdateState()
    {
        var nextState = states[(int)_currentState].FixedUpdate(this);

        if (nextState != ISharkState.E_State.Unchanged)
        {
            //���̏�ԂɑJ��
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
    }

    private void FixedUpdate()
    {
        FixedUpdateState();
    }

    public void OnEnter(Collider playerCollider, BalloonState balloonState)
    {
        //��s�ɂ܂Ƃ߂��܂����A�ύX���e�ՂȂ��߂������܂�
        if (_currentState == ISharkState.E_State.Fall) return;
        if (_currentState == ISharkState.E_State.Down) return;
        if (_currentState == ISharkState.E_State.Death) return;

        //�q�b�g���̃N�[���_�E�����J�n
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
        if (_currentState == ISharkState.E_State.Chase)
        {
            _isHitNotBreakBlock = true;
            SoundManager.Instance.PlaySE(_clashAudioSource, SoundSource.SE063_SharkClash);
        }
    }

    public void StartBlockBreak(GameObject breakableBlock)
    {
        if (_currentState == ISharkState.E_State.Chase)
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
            //��������܂�false�ɂ���
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

            //�~�X�񐔂�500����x�������񐔓I�ɗǂ��ł�
            if (wrapCount == 100)
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
        Debug.DrawRay(transform.position, Vector3.down * 6.0f, Color.red, 0.1f);
        Debug.DrawRay(transform.position, Vector3.forward * 2.0f, Color.red, 0.1f);
        return Physics.Raycast(transform.position, Vector3.down, 6.0f);
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

        if (damageCount == hpMAX)
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
    }
    public override void Interact()
    {
        if (_currentState == ISharkState.E_State.Down)
        {
            pushCheck = true;
        }
    }
}