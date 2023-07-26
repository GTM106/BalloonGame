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
    IPlayer _player;

    // ó‘ÔŠÇ—
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
            throw new System.NotImplementedException();
        }

        public IState.E_State Update(PlayerController parent)
        {
            throw new System.NotImplementedException();
        }

        public IState.E_State FixedUpdate(PlayerController parent)
        {
            throw new System.NotImplementedException();
        }
    }

    class FallingState : IState
    {
        public IState.E_State Initialize(PlayerController parent)
        {
            throw new System.NotImplementedException();
        }

        public IState.E_State Update(PlayerController parent)
        {
            throw new System.NotImplementedException();
        }
        public IState.E_State FixedUpdate(PlayerController parent)
        {
            throw new System.NotImplementedException();
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
            //Ÿ‚Ìó‘Ô‚É‘JˆÚ
            _currentState = nextState;
            InitializeState();
        }
    }

    void FixedUpdateState()
    {
        var nextState = states[(int)_currentState].FixedUpdate(this);

        if (nextState != IState.E_State.Unchanged)
        {
            //Ÿ‚Ìó‘Ô‚É‘JˆÚ
            _currentState = nextState;
            InitializeState();
        }
    }

    private void Awake()
    {
        //TODO:•—‘D‚Ìˆ—‚ğ’Ç‰Á‚·‚éÛ‚ÉInflatablePlayer‚Æ“ü‚ê‘Ö‚¦‚éˆ—‚ğ’Ç‰Á‚·‚é
        _player = new DeflatablePlayer(_playerParameter);
    }

    private void Update()
    {
        UpdateState();
    }

    private void FixedUpdate()
    {
        FixedUpdateState();
    }
}
