using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IState
{
    public enum E_State
    {
        Control,
        Jumping,
        Falling,

        MAX,

        Unchanged,
    }

    E_State Initialize(PlayerController parent);
    E_State Update(PlayerController parent);
    E_State FixedUpdate(PlayerController parent);
}

public class PlayerController : MonoBehaviour
{
    [SerializeField] PlayerParameter _playerParameter = default!;
    [SerializeField] BalloonController _balloonController = default!;
    [SerializeField] GroundCheck _groundCheck = default!;
    IPlayer _player;
    IPlayer _inflatablePlayer;
    IPlayer _deflatablePlayer;


    // 状態管理
    IState.E_State _currentState = IState.E_State.Control;
    static readonly IState[] states = new IState[(int)IState.E_State.MAX]
    {
        new ControlState(),
        new JumpingState(),
        new FallingState(),
    };

    class ControlState : IState
    {
        public IState.E_State Initialize(PlayerController parent)
        {
            return IState.E_State.Unchanged;
        }

        public IState.E_State Update(PlayerController parent)
        {
            return IState.E_State.Unchanged;
        }

        public IState.E_State FixedUpdate(PlayerController parent)
        {
            parent._player.Dash();
            return IState.E_State.Unchanged;
        }
    }

    class JumpingState : IState
    {
        public IState.E_State Initialize(PlayerController parent)
        {
            parent._player.Jump(parent._playerParameter.Rb);
            return IState.E_State.Unchanged;
        }

        public IState.E_State Update(PlayerController parent)
        {
            return IState.E_State.Unchanged;
        }

        public IState.E_State FixedUpdate(PlayerController parent)
        {
            return IState.E_State.Unchanged;
        }
    }

    class FallingState : IState
    {
        public IState.E_State Initialize(PlayerController parent)
        {
            return IState.E_State.Unchanged;
        }

        public IState.E_State Update(PlayerController parent)
        {
            return IState.E_State.Unchanged;
        }

        public IState.E_State FixedUpdate(PlayerController parent)
        {
            return IState.E_State.Unchanged;
        }
    }

    void InitializeState()
    {
        var nextState = states[(int)_currentState].Initialize(this);

        if (nextState != IState.E_State.Unchanged)
        {
            _currentState = nextState;
            InitializeState();
        }
    }

    void UpdateState()
    {
        var nextState = states[(int)_currentState].Update(this);

        if (nextState != IState.E_State.Unchanged)
        {
            //次の状態に遷移
            _currentState = nextState;
            InitializeState();
        }
    }

    void FixedUpdateState()
    {
        var nextState = states[(int)_currentState].FixedUpdate(this);

        if (nextState != IState.E_State.Unchanged)
        {
            //次の状態に遷移
            _currentState = nextState;
            InitializeState();
        }
    }

    private void Awake()
    {
        _inflatablePlayer = new InflatablePlayer(_playerParameter);
        _deflatablePlayer = new DeflatablePlayer(_playerParameter);

        InitializeState();
        _balloonController.OnStateChanged += OnBalloonStateChanged;
        _playerParameter.JoyconLeft.OnDownButtonPressed += JoyconLeft_OnDownButtonPressed;
    }

    private void OnBalloonStateChanged(BalloonState state)
    {
        _player = state == BalloonState.Normal ? _deflatablePlayer : _inflatablePlayer;
    }

    private void JoyconLeft_OnDownButtonPressed()
    {
        //ステート関係なしにひとまず行います。
        //ステートパターンに当てはめる作業は後ほど行います。
        if (_groundCheck.IsGround(out _))
        {
            _player.Jump(_playerParameter.Rb);
        }
    }

    private void Update()
    {
        UpdateState();
    }

    private void FixedUpdate()
    {
        FixedUpdateState();
        _player.AdjustingGravity();
    }

    private void OnDestroy()
    {
        _balloonController.OnStateChanged -= OnBalloonStateChanged;
        _playerParameter.JoyconLeft.OnDownButtonPressed -= JoyconLeft_OnDownButtonPressed;
    }
}
