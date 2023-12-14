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
    [Header("Target��ǐՂ��邩�ǂ���")]
    [SerializeField] bool _enableChasing = true;
    [Header("�ǐՑΏ�")]
    [SerializeField] Transform _target = default!;
    [Header("���񂷂�n�_")]
    [SerializeField] Transform[] _wayPoints = default!;
    [Header("���񑬓x")]
    [SerializeField, Min(0f)] float _patrolSpeed = default!;
    [Header("�ǐՂ��J�n����͈�")]
    [SerializeField, Min(0f)] float _chaseRadius = default!;
    [Header("�ǐՑ��x")]
    [SerializeField, Min(0f)] float _chaseSpeed = default!;
    [Header("�ǐՂ��s���ő厞��")]
    [SerializeField, Min(0f)] float _maxChaseDuration = default!;
    [Header("�ǐՂ���߂鋗��")]
    [SerializeField, Min(0f)] float _maxChaseDistance = default!;
    [Header("�ǐՎ��Ԃ𒴂����ꍇ�ɍēx�ǐՂ���܂ł̃N�[���^�C��")]
    [SerializeField] float _cooldownForReacquisitionDuration = default!;
    [SerializeField, Required] PlayerGameOverEvent _gameOverEvent = default!;
    [SerializeField, Required, FormerlySerializedAs("_audioSource")] AudioSource _discoveryAudioSource = default!;
    [SerializeField, Required] AudioSource _chasingAudioSource = default!;
    [SerializeField, Required] NavMeshAgent _navMeshAgent = default!;
    [SerializeField] AnimationChanger<E_Harmit> _animationChanger = default!;

    int _currentPatrolIndex = 0;
    float _elapsedChaseTime = 0f;
    float _elapsedCoolTime = 0f;

    //�ĒǐՂ܂ł̃N�[���_�E�������ǂ���
    bool _isInCooldownForReacquisition = false;

    //�����A�j���[�V�����Đ������ǂ���
    bool _isPlayingDiscoveryAnimation = false;

    //�v���C���[�Ƀq�b�g�������ǂ���
    bool _isHitPlayer = false;

    //�L���b�V������Transform
    Transform _transform = default!;

    [SerializeField] VisualEffect FindPlayerEff;

    #region State
    // ��ԊǗ�
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
            //�ڕW�n�_�ɓ��B������������s��
            parent.CheckDestinationArrival();

            //�ĒǐՂ܂ł̃N�[���_�E�����s��
            parent.WaitForCooldownForReacquisition();

            //��s�ɂ܂Ƃ߂��܂����A�ǐ��̂��߂ɂ������܂��B
            if (parent.CanDiscoveryTarget())
            {
                //�ǐՉ\�Ȃ�ǐՁB���̑O�ɔ�������
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
            //���̏�ԂɑJ��
            _currentState = nextState;
            InitializeState();
        }
    }

    private void FixedUpdateState()
    {
        var nextState = states[(int)_currentState].FixedUpdate(this);

        if (nextState != IHermitCrabState.E_State.Unchanged)
        {
            //���̏�ԂɑJ��
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

        //�v���C���[�ɐG�ꂽ�珄�񃋁[�g�ɖ߂��Ă���
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
        //�N�[���_�E�����łȂ��Ȃ珈�����Ȃ�
        if (!_isInCooldownForReacquisition) return;

        //�N�[���_�E���o�߂܂őҋ@
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
        //���������ǐՂ�������Ă��Ȃ��Ȃ�ǐՂ��Ȃ�
        if (!_enableChasing) return false;

        //Target�̋����Ɋւ�炸�A�ĒǐՂ̃N�[���_�E�����Ȃ�ǐՂ��Ȃ�
        if (_isInCooldownForReacquisition) return false;

        //��苗���ȓ��Ȃ�ǐՂ���
        float distance = Vector3.Distance(_transform.position, _target.position);
        return distance <= _chaseRadius;
    }

    private void StartDiscovery()
    {
        _isPlayingDiscoveryAnimation = true;

        //�^�[�Q�b�g�̕�������������
        Vector3 directionToTarget = _target.position - _transform.position;
        directionToTarget.y = 0f;
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
        _transform.rotation = targetRotation;

        //������SE�̍Đ�
        SoundManager.Instance.PlaySE(_discoveryAudioSource, SoundSource.SE302_HarmitDiscovery);

        //�������A�j���[�V�����̍Đ��B�Đ��㎩���Ń_�b�V�����[�V�����ɂȂ�܂�
        _animationChanger.ChangeAnimation(E_Harmit.AN02_discovery);

        //�������̃G�t�F�N�g���Đ�
        FindPlayerEff.Play();

        //���̏�ŃA�j���[�V�������Đ�������
        _navMeshAgent.destination = _transform.position;
    }

    private void StartChasing()
    {
        _isHitPlayer = false;

        //�ǐ�SE�̃��[�v�Đ�
        SoundManager.Instance.PlaySE(_chasingAudioSource, SoundSource.SE011_Hermit_Chase);

        //�ǐՌo�ߎ��Ԃ�������
        _elapsedChaseTime = 0f;

        //�ǐՃX�s�[�h��ύX
        _navMeshAgent.speed = _chaseSpeed;
    }

    private void Chasing()
    {
        //��Ƀv���C���[��ǂ�������
        _navMeshAgent.destination = _target.position;

        //�ǐՂ��Ă��鎞�Ԃ����Z
        _elapsedChaseTime += Time.deltaTime;
    }

    private void StopChasing()
    {
        //�ǐ�SE�̃��[�v�Đ����~
        SoundManager.Instance.StopSE(_chasingAudioSource, 0.1f);

        //�ĒǐՉ\�܂ł̃N�[���_�E�����J�n����
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

    //�A�j���[�^�[����Event�Ƃ��ČĂ�ł��܂�
    public void OnFinishDiscoveryAnimation()
    {
        _isPlayingDiscoveryAnimation = false;
    }
}