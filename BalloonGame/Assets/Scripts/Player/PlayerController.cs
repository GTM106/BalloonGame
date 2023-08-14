using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public interface IState
{
    public enum E_State
    {
        Control,
        Jumping,
        Falling,
        BoostDash,

        MAX,

        Unchanged,
    }

    E_State Initialize(PlayerController parent);
    E_State Update(PlayerController parent);
    E_State FixedUpdate(PlayerController parent);
    E_State RingconPull(PlayerController parent);
}

public class PlayerController : MonoBehaviour
{
    [SerializeField] PlayerParameter _playerParameter = default!;
    [SerializeField] BalloonController _balloonController = default!;
    [SerializeField] GroundCheck _groundCheck = default!;
    [SerializeField] InputActionReference _ringconPullAction = default!;
    IPlayer _player;
    IPlayer _inflatablePlayer;
    IPlayer _deflatablePlayer;

    readonly Dictionary<BalloonState, IPlayer> _playerPairs = new();

    // 状態管理
    IState.E_State _currentState = IState.E_State.Control;
    static readonly IState[] states = new IState[(int)IState.E_State.MAX]
    {
        new ControlState(),
        new JumpingState(),
        new FallingState(),
        new BoostDashState(),
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

        public IState.E_State RingconPull(PlayerController parent)
        {
            return IState.E_State.BoostDash;
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

        public IState.E_State RingconPull(PlayerController parent)
        {
            return IState.E_State.BoostDash;
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

        public IState.E_State RingconPull(PlayerController parent)
        {
            return IState.E_State.BoostDash;
        }
    }

    class BoostDashState : IState
    {
        int _boostDashFrame;

        public IState.E_State Initialize(PlayerController parent)
        {
            _boostDashFrame = 0;
            parent._player.BoostDash();
            return IState.E_State.Unchanged;
        }

        public IState.E_State Update(PlayerController parent)
        {
            return IState.E_State.Unchanged;
        }

        public IState.E_State FixedUpdate(PlayerController parent)
        {
            //1行にまとめられますが、可読性のために長く書いています
            _boostDashFrame++;
            if (_boostDashFrame >= parent._playerParameter.BoostFlame) return IState.E_State.Control;

            return IState.E_State.Unchanged;
        }

        public IState.E_State RingconPull(PlayerController parent)
        {
            return IState.E_State.Unchanged;
        }
    }

    private void InitializeState()
    {
        var nextState = states[(int)_currentState].Initialize(this);

        if (nextState != IState.E_State.Unchanged)
        {
            _currentState = nextState;
            InitializeState();
        }
    }

    private void UpdateState()
    {
        var nextState = states[(int)_currentState].Update(this);

        if (nextState != IState.E_State.Unchanged)
        {
            //次の状態に遷移
            _currentState = nextState;
            InitializeState();
        }
    }

    private void FixedUpdateState()
    {
        var nextState = states[(int)_currentState].FixedUpdate(this);

        if (nextState != IState.E_State.Unchanged)
        {
            //次の状態に遷移
            _currentState = nextState;
            InitializeState();
        }
    }

    private void RingconPullState()
    {
        var nextState = states[(int)_currentState].RingconPull(this);

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
        _player = _deflatablePlayer;

        InitializeState();

        _balloonController.OnStateChanged += OnBalloonStateChanged;
        _playerParameter.JoyconLeft.OnDownButtonPressed += JoyconLeft_OnDownButtonPressed;
        _ringconPullAction.action.performed += OnRingconPull;

        _playerPairs.Add(BalloonState.Normal, _deflatablePlayer);
        _playerPairs.Add(BalloonState.Expands, _inflatablePlayer);
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
        _ringconPullAction.action.performed -= OnRingconPull;
    }

    private void OnRingconPull(InputAction.CallbackContext obj)
    {
        RingconPullState();
    }

    private void OnBalloonStateChanged(BalloonState state)
    {
        if (_playerPairs.TryGetValue(state, out var player))
        {
            _player = player;
        }
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
}
