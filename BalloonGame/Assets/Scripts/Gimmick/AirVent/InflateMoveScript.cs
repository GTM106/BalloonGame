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
    [Header("�i�s�����̍��W")]
    [SerializeField] List<Transform> targetPoint = default!;
    [Header("�M�~�b�N�������ʒu�ɖ߂邩�ǂ���")]
    [SerializeField] bool rollReturn = default!;
    [Header("�M�~�b�N���ő�ړ��������猳�̈ʒu�ɖ߂邩�ǂ���")]
    [SerializeField] bool arrivalReturn = default!;
    [Header("�M�~�b�N�����̈ʒu�ɖ߂�P��")]
    [SerializeField] float returnSpeed = default!;
    [Header("�M�~�b�N�����̈ʒu�ɖ߂�n�߂�܂ł̒P��")]
    [SerializeField] float stopTime = default!;
    [Header("�M�~�b�N�̈ړ�����")]
    [SerializeField] float moveDuration = default!;
    [Header("�C�[�W���O�^�C�v�̑I��")]
    [SerializeField] EaseType easeType = EaseType.OutCubic;
    [SerializeField] float overshootOrAmplitude = default!;
    [SerializeField] float period = default!;

    readonly List<Vector3> targetPos = new();

    int currentTargetPoint = 0;
    float rollBackTime = 0f;
    const float MIN_DISTANCE = 0.01f;
    bool isInteractThisFrame = false;

    #region State
    // ��ԊǗ�
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

            //���̖ڕW�n�_�ɐi��
            parent.StartNextMove();

            return IInflateMoveState.E_State.Unchanged;
        }

        public IInflateMoveState.E_State Update(InflateMoveScript parent)
        {
            //�����Ă�Œ��͒ǉ��œ������Ȃ�
            if (parent.isInteractThisFrame)
            {
                parent.isInteractThisFrame = false;
            }

            return IInflateMoveState.E_State.Unchanged;
        }

        public IInflateMoveState.E_State FixedUpdate(InflateMoveScript parent)
        {
            //��������
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
            //�����Ō��̒n�_�܂ŏ��ɖ߂�
            parent.transform.position = Vector3.MoveTowards(parent.transform.position, parent.targetPos[parent.currentTargetPoint], parent.returnSpeed * Time.deltaTime);

            //�����n�_�܂Ŗ߂���������
            if (Vector3.Distance(parent.transform.position, parent.targetPos[0]) <= MIN_DISTANCE)
            {
                parent.rollBackTime = 0f;
                return IInflateMoveState.E_State.Normal;
            }

            //���p�n�_�ɂ��ǂ蒅�����炻�̎��ɐi��
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
            //���̏�ԂɑJ��
            _currentState = nextState;
            InitializeState();
        }
    }

    private void FixedUpdateState()
    {
        var nextState = states[(int)_currentState].FixedUpdate(this);

        if (nextState != IInflateMoveState.E_State.Unchanged)
        {
            //���̏�ԂɑJ��
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
        //�����������[���o�b�N��������Ă��Ȃ��Ȃ�߂�Ȃ�
        if (!rollReturn) return false;

        //�ŏI�n�_�ōŏI�n�_���B���̃��[���o�b�N��������Ă��Ȃ��Ȃ�߂�Ȃ�
        if (!arrivalReturn && currentTargetPoint == targetPos.Count - 1) return false;
        
        //�J�n�n�_�Ȃ�߂�Ȃ�
        if (currentTargetPoint == 0) return false;

        //�߂�܂ł̎��Ԍv��
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