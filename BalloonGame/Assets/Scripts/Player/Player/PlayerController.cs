using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.VFX;

public interface IState
{
    public enum E_State
    {
        Control,
        Jumping,
        Falling,
        BoostDash,
        GameOver,
        Revive,

        MAX,

        Unchanged,
    }

    E_State Initialize(PlayerController parent);
    E_State Update(PlayerController parent);
    E_State FixedUpdate(PlayerController parent);
    E_State CalledBoostDashEvent(PlayerController parent, BoostDashData boostFrame);
    E_State RingconPush(PlayerController parent);
    E_State JumpButtonPressed(PlayerController parent);
}

public class PlayerController : MonoBehaviour
{
    [SerializeField] PlayerParameter _playerParameter = default!;
    [SerializeField, Required] BalloonController _balloonController = default!;
    [SerializeField, Required] GroundCheck _groundCheck = default!;
    [SerializeField, Required] InputActionReference _ringconPushAction = default!;
    [SerializeField, Required] Collider _playerCollider = default!;
    [SerializeField, Required] WaterEvent _waterEvent = default!;
    [SerializeField, Required] FunEvent _funEvent = default!;
    [SerializeField, Required] GameOverUIController _gameOverUIController = default!;
    [SerializeField, Required] PlayerGameOverEvent _playerGameOverEvent = default!;
    [SerializeField, Required] BoostDashEvent _boostDashEvent = default!;
    [SerializeField, Required] AudioSource _jumpingAudioSource = default!;
    [SerializeField, Required] AudioSource _landingAudioSource = default!;
    [SerializeField, Required] AudioSource _gameoverAudioSource = default!;
    [SerializeField, Required] VisualEffect _piyopiyoeff = default!;

    IPlayer _player;
    IPlayer _inflatablePlayer;
    IPlayer _deflatablePlayer;

    //BoostDashEventから渡されるデータの保存用
    BoostDashData _boostDashData = default!;

    readonly Dictionary<BalloonState, IPlayer> _playerPairs = new();

    #region State
    // 状態管理
    IState.E_State _currentState = IState.E_State.Control;
    static readonly IState[] states = new IState[(int)IState.E_State.MAX]
    {
        new ControlState(),
        new JumpingState(),
        new FallingState(),
        new BoostDashState(),
        new GameOverState(),
        new ReviveState(),
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
            parent._player.Dash(parent._currentState);
            if (parent._playerParameter.Rb.velocity.magnitude <= 0.01f)
            {
                parent._playerParameter.ChangeIdleAnimation();
            }

            if (!parent._groundCheck.IsGround(out _))
            {
                return IState.E_State.Falling;
            }

            return IState.E_State.Unchanged;
        }

        public IState.E_State CalledBoostDashEvent(PlayerController parent, BoostDashData boostFrame)
        {
            return IState.E_State.BoostDash;
        }

        public IState.E_State RingconPush(PlayerController parent)
        {
            return IState.E_State.Unchanged;
        }

        public IState.E_State JumpButtonPressed(PlayerController parent)
        {
            return IState.E_State.Jumping;
        }
    }

    class JumpingState : IState
    {
        public IState.E_State Initialize(PlayerController parent)
        {
            SoundManager.Instance.PlaySE(parent._jumpingAudioSource, SoundSource.SE002_PlayerJumping);
            parent._player.Jump(parent._playerParameter.Rb);
            return IState.E_State.Unchanged;
        }

        public IState.E_State Update(PlayerController parent)
        {
            return IState.E_State.Unchanged;
        }

        public IState.E_State FixedUpdate(PlayerController parent)
        {
            if (parent._playerParameter.Rb.velocity.y <= 0f)
            {
                return IState.E_State.Falling;
            }

            return IState.E_State.Unchanged;
        }

        public IState.E_State CalledBoostDashEvent(PlayerController parent, BoostDashData boostFrame)
        {
            return IState.E_State.BoostDash;
        }

        public IState.E_State RingconPush(PlayerController parent)
        {
            return IState.E_State.Unchanged;
        }

        public IState.E_State JumpButtonPressed(PlayerController parent)
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
            if (parent._groundCheck.IsGround(out _))
            {
                SoundManager.Instance.PlaySE(parent._landingAudioSource, SoundSource.SE003_PlayerLanding);
                return IState.E_State.Control;
            }

            parent._player.Fall();
            parent._player.Dash(parent._currentState);

            return IState.E_State.Unchanged;
        }

        public IState.E_State CalledBoostDashEvent(PlayerController parent, BoostDashData boostFrame)
        {
            return IState.E_State.BoostDash;
        }

        public IState.E_State RingconPush(PlayerController parent)
        {
            return IState.E_State.Unchanged;
        }

        public IState.E_State JumpButtonPressed(PlayerController parent)
        {
            return IState.E_State.Unchanged;
        }
    }

    class BoostDashState : IState
    {
        int _boostDashFrame;

        public IState.E_State Initialize(PlayerController parent)
        {
            _boostDashFrame = parent._boostDashData.Value;

            parent._player.BoostDash(parent._boostDashData);
            return IState.E_State.Unchanged;
        }

        public IState.E_State Update(PlayerController parent)
        {
            return IState.E_State.Unchanged;
        }

        public IState.E_State FixedUpdate(PlayerController parent)
        {
            parent._player.Dash(parent._currentState);

            //1行にまとめられますが、可読性のために長く書いています
            _boostDashFrame--;
            if (_boostDashFrame <= 0) return IState.E_State.Control;

            return IState.E_State.Unchanged;
        }

        public IState.E_State CalledBoostDashEvent(PlayerController parent, BoostDashData boostFrame)
        {
            return IState.E_State.Unchanged;
        }

        public IState.E_State RingconPush(PlayerController parent)
        {
            return IState.E_State.Unchanged;
        }

        public IState.E_State JumpButtonPressed(PlayerController parent)
        {
            return IState.E_State.Unchanged;
        }
    }

    class GameOverState : IState
    {
        int _currentPressCount;

        public IState.E_State Initialize(PlayerController parent)
        {
            _currentPressCount = 0;

            SoundManager.Instance.PlaySE(parent._gameoverAudioSource, SoundSource.SE006_PlayerDamaged);

            parent._playerParameter.AnimationChanger.ChangeAnimation(E_Atii.Down);

            parent._gameOverUIController.Enable();

            parent._piyopiyoeff.enabled = true;
            parent._piyopiyoeff.SendEvent("OnPlay");

            parent._playerParameter.BitSetEnvironmentStatus(EnvironmentStatus.GameOver);

            return IState.E_State.Unchanged;
        }

        public IState.E_State Update(PlayerController parent)
        {
            return IState.E_State.Unchanged;
        }

        public IState.E_State FixedUpdate(PlayerController parent)
        {
            parent._player.Fall();

            if (_currentPressCount >= parent._playerParameter.RequiredPushCount)
            {
                //復活通知を送る
                parent._playerGameOverEvent.Revive();

                return IState.E_State.Revive;
            }

            return IState.E_State.Unchanged;
        }

        public IState.E_State CalledBoostDashEvent(PlayerController parent, BoostDashData boostFrame)
        {
            return IState.E_State.Unchanged;
        }

        public IState.E_State RingconPush(PlayerController parent)
        {
            _currentPressCount++;

            //一度でもプッシュされたら表示する
            parent._gameOverUIController.Enable();

            return IState.E_State.Unchanged;
        }

        public IState.E_State JumpButtonPressed(PlayerController parent)
        {
            return IState.E_State.Unchanged;
        }
    }

    class ReviveState : IState
    {
        //アニメーションの待機フレーム。
        const int REVIVE_ANIMATION_WAIT_FRAME = 50;
        int _reviveFrame;

        public IState.E_State Initialize(PlayerController parent)
        {
            _reviveFrame = REVIVE_ANIMATION_WAIT_FRAME;
            parent._playerParameter.AnimationChanger.ChangeAnimation(E_Atii.Retry);

            parent._piyopiyoeff.enabled = false;
            parent._piyopiyoeff.SendEvent("StopPlay");

            parent._playerParameter.BitClearEnvironmentStatus(EnvironmentStatus.GameOver);

            return IState.E_State.Unchanged;
        }

        public IState.E_State Update(PlayerController parent)
        {
            return IState.E_State.Unchanged;
        }

        public IState.E_State FixedUpdate(PlayerController parent)
        {
            _reviveFrame--;
            if (_reviveFrame <= 0)
            {
                return IState.E_State.Control;
            }

            return IState.E_State.Unchanged;
        }

        public IState.E_State CalledBoostDashEvent(PlayerController parent, BoostDashData boostFrame)
        {
            return IState.E_State.Unchanged;
        }

        public IState.E_State RingconPush(PlayerController parent)
        {
            return IState.E_State.Unchanged;
        }

        public IState.E_State JumpButtonPressed(PlayerController parent)
        {
            return IState.E_State.Unchanged;
        }
    }

    //ステートを強制的に変える
    private void ForceChangeState(IState.E_State nextState)
    {
        if (nextState != IState.E_State.Unchanged)
        {
            _currentState = nextState;
            InitializeState();
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

    private void RingconPushState()
    {
        var nextState = states[(int)_currentState].RingconPush(this);

        if (nextState != IState.E_State.Unchanged)
        {
            //次の状態に遷移
            _currentState = nextState;
            InitializeState();
        }
    }

    private void OnBoostDashEvent(BoostDashData frame)
    {
        _boostDashData = frame;
        var nextState = states[(int)_currentState].CalledBoostDashEvent(this, frame);

        if (nextState != IState.E_State.Unchanged)
        {
            //次の状態に遷移
            _currentState = nextState;
            InitializeState();
        }
    }

    private void JoyconLeft_OnDownButtonPressed()
    {
        //接地状態じゃないと作動させない
        if (!_groundCheck.IsGround(out _)) return;

        var nextState = states[(int)_currentState].JumpButtonPressed(this);

        if (nextState != IState.E_State.Unchanged)
        {
            //次の状態に遷移
            _currentState = nextState;
            InitializeState();
        }
    }
    #endregion

    private void Awake()
    {
        _inflatablePlayer = new InflatablePlayer(_playerParameter);
        _deflatablePlayer = new DeflatablePlayer(_playerParameter);
        _player = _deflatablePlayer;

        SubscribeToEvents();

        _playerPairs.Add(BalloonState.Normal, _deflatablePlayer);
        _playerPairs.Add(BalloonState.Expands, _inflatablePlayer);
    }

    private void Start()
    {
        InitializeState();
    }

    private void Update()
    {
        UpdateState();
    }

    private void FixedUpdate()
    {
        FixedUpdateState();
        _player.AdjustingGravity();
        FrictionAdjustment();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void SubscribeToEvents()
    {
        _balloonController.OnStateChanged += OnBalloonStateChanged;
        _playerParameter.JoyconLeft.OnDownButtonPressed += JoyconLeft_OnDownButtonPressed;
        _ringconPushAction.action.performed += OnRingconPush;
        _waterEvent.OnEnterAction += OnWaterEnter;
        _waterEvent.OnStayAction += OnWaterStay;
        _waterEvent.OnExitAction += OnWaterExit;
        _funEvent.OnEnterAction += OnFunEnter;
        _funEvent.OnStayAction += OnFunStay;
        _funEvent.OnExitAction += OnFunExit;
        _playerGameOverEvent.OnGameOver += OnGameOver;
        _playerGameOverEvent.OnRevive += OnRevive;
        _boostDashEvent.OnBoostDash += OnBoostDashEvent;
    }

    private void UnsubscribeFromEvents()
    {
        _balloonController.OnStateChanged -= OnBalloonStateChanged;
        _playerParameter.JoyconLeft.OnDownButtonPressed -= JoyconLeft_OnDownButtonPressed;
        _ringconPushAction.action.performed -= OnRingconPush;
        _waterEvent.OnEnterAction -= OnWaterEnter;
        _waterEvent.OnStayAction -= OnWaterStay;
        _waterEvent.OnExitAction -= OnWaterExit;
        _funEvent.OnEnterAction -= OnFunEnter;
        _funEvent.OnStayAction -= OnFunStay;
        _funEvent.OnExitAction -= OnFunExit;
        _playerGameOverEvent.OnGameOver -= OnGameOver;
        _playerGameOverEvent.OnRevive -= OnRevive;
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Player接触処理のインターフェースを継承しているオブジェクトの処理を呼ぶ
        //何が実行されるかはインターフェースの継承先を参照してください。
        if (collision.gameObject.TryGetComponent(out IHittable hitObject))
        {
            hitObject.OnEnter(_playerCollider, _balloonController.State);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        //Player接触処理のインターフェースを継承しているオブジェクトの処理を呼ぶ
        //何が実行されるかはインターフェースの継承先を参照してください。
        if (collision.gameObject.TryGetComponent(out IHittable hitObject))
        {
            hitObject.OnStay(_playerCollider, _balloonController.State);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        //Player接触処理のインターフェースを継承しているオブジェクトの処理を呼ぶ
        //何が実行されるかはインターフェースの継承先を参照してください。
        if (collision.gameObject.TryGetComponent(out IHittable hitObject))
        {
            hitObject.OnExit(_playerCollider, _balloonController.State);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //Player接触処理のインターフェースを継承しているオブジェクトの処理を呼ぶ
        //何が実行されるかはインターフェースの継承先を参照してください。
        if (other.TryGetComponent(out IHittable hitObject))
        {
            hitObject.OnEnter(_playerCollider, _balloonController.State);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        //Player接触処理のインターフェースを継承しているオブジェクトの処理を呼ぶ
        //何が実行されるかはインターフェースの継承先を参照してください。
        if (other.TryGetComponent(out IHittable hitObject))
        {
            hitObject.OnStay(_playerCollider, _balloonController.State);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //Player接触処理のインターフェースを継承しているオブジェクトの処理を呼ぶ
        //何が実行されるかはインターフェースの継承先を参照してください。
        if (other.TryGetComponent(out IHittable hitObject))
        {
            hitObject.OnExit(_playerCollider, _balloonController.State);
        }
    }

    private void OnRingconPush(InputAction.CallbackContext obj)
    {
        RingconPushState();
    }

    private void OnBalloonStateChanged(BalloonState state)
    {
        if (_playerPairs.TryGetValue(state, out var player))
        {
            _player = player;
        }

        if (state == BalloonState.Expands)
        {
            _playerParameter.AnimationChanger.ChangeAnimation(E_Atii.AirIn);
        }
        if (state == BalloonState.Normal)
        {
            _playerParameter.AnimationChanger.ChangeAnimation(E_Atii.AirOut);
        }
    }

    private void OnWaterEnter()
    {
        _playerParameter.BitSetEnvironmentStatus(EnvironmentStatus.Underwater);
    }

    private void OnWaterStay()
    {
        _player.OnWaterStay();
    }

    private void OnWaterExit()
    {
        _playerParameter.BitClearEnvironmentStatus(EnvironmentStatus.Underwater);
    }

    private void OnFunEnter(Vector3 windVec)
    {
        _playerParameter.BitSetEnvironmentStatus(EnvironmentStatus.WindAffected);
        _playerParameter.AnimationChanger.ChangeAnimation(E_Atii.AN01_Wind);
    }

    private void OnFunStay(Vector3 windVec)
    {
        _player.OnWindStay(windVec);
    }

    private void OnFunExit(Vector3 windVec)
    {
        _playerParameter.BitClearEnvironmentStatus(EnvironmentStatus.WindAffected);
        _playerParameter.AnimationChanger.ChangeAnimation(E_Atii.AN01_Wind_default);
    }

    private void OnRevive()
    {
        _gameOverUIController.Disable();
    }

    private void OnGameOver()
    {
        ForceChangeState(IState.E_State.GameOver);
    }

    private void FrictionAdjustment()
    {
        //真下にオブジェクトがない場合、摩擦をなくす
        //壁に張り付く問題の対抗策
        _playerParameter.PhysicMaterial.dynamicFriction = GetGroundNormal() == Vector3.zero ? 0f : 1f;
    }

    private Vector3 GetGroundNormal()
    {
        float raycastDistance = 1.2f;

        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, raycastDistance))
        {
            return hit.normal;
        }

        return Vector3.zero;
    }
}
