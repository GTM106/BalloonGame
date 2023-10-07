using Cinemachine;
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BalloonState
{
    Normal,
    Expands,
    ScaleAnimation,
    BoostDash,
    GameOver,
}

public class BalloonController : MonoBehaviour
{
    [SerializeField] WaterEvent _waterEvent = default!;
    [SerializeField] CinemachineTargetGroup _cinemachineTargetGroup = default!;
    [SerializeField] CinemachineController _cinemachineController = default!;
    [SerializeField] PlayerGameOverEvent _playerGameOverEvent = default!;

    [Header("�c���A�j���[�V�����̎�������")]
    [SerializeField, Min(0f)] float _scaleAnimationDuration = 0.1f;
    [Header("�ǂ̂��炢�c�����邩�B�X�P�[���P��")]
    [SerializeField, Min(0f)] float _scaleOffset = 0.5f;
    [Header("1�b�Ԃɂǂ̂��炢�X�P�[�����k�ނ�")]
    [SerializeField, Min(0f)] float _scaleAmountDeflatingPerSecond;
    [Header("���ɓ����Ă���Ƃ�1�b�Ԃɂǂ̂��炢�X�P�[�����k�ނ�")]
    [SerializeField, Min(0f)] float _scaleAmountDeflatingPerSecondInWater;
    [Header("������у_�b�V���̎������ԁBPlayerController�Ɠ����l��ݒ肵�Ă�������")]
    [SerializeField, Min(0)] int _boostFrame = default!;

    float _defaultScaleValue;

    //�v���p�e�B�̕����g�p���Ă�������
    BalloonState _state;
    public BalloonState State
    {
        get { return _state; }
        private set
        {
            if (_state != value) OnStateChanged?.Invoke(value);
            _state = value;
        }
    }

    public event Action<BalloonState> OnStateChanged;

    private void Awake()
    {
        _defaultScaleValue = transform.localScale.x;
        _waterEvent.OnStayAction += OnWaterStay;
        _playerGameOverEvent.OnGameOver += OnGameOver;
        _playerGameOverEvent.OnRevive += OnRevive;
    }

    private void OnRevive()
    {
        State = BalloonState.Normal;
    }

    private void OnGameOver()
    {
        //���D�̋�C�𔲂�
        ChangeScale(_defaultScaleValue);
        State = BalloonState.GameOver;
    }

    private void Update()
    {
        BalloonDeflation(_scaleAmountDeflatingPerSecond);
    }

    private void OnDestroy()
    {
        _waterEvent.OnStayAction -= OnWaterStay;
        _playerGameOverEvent.OnGameOver -= OnGameOver;
        _playerGameOverEvent.OnRevive -= OnRevive;
    }

    public void Expand()
    {
        if (State is not BalloonState.Normal and not BalloonState.Expands) return;

        ExpandScaleAnimation().Forget();
    }

    public async void OnRingconPull()
    {
        if (State != BalloonState.Expands) return;
        var token = this.GetCancellationTokenOnDestroy();

        State = BalloonState.BoostDash;

        //���̏���������ChangeScale�łȂ����ڏ���������B
        transform.localScale = Vector3.one * _defaultScaleValue;

        _cinemachineController.OnAfterBoostDash(_boostFrame);

        int currentFrame = 0;

        while (currentFrame <= _boostFrame)
        {
            await UniTask.Yield(PlayerLoopTiming.FixedUpdate, token);

            float progress = currentFrame / _boostFrame;

            //������у_�b�V�������͓���ŃX�P�[���𖳎����ăJ�����̎���p��ύX
            _cinemachineTargetGroup.m_Targets[0].radius = _defaultScaleValue * progress;

            currentFrame++;
        }

        State = BalloonState.Normal;
    }

    private async UniTask ExpandScaleAnimation()
    {
        if (State == BalloonState.ScaleAnimation) return;
        var token = this.GetCancellationTokenOnDestroy();
        float time = 0f;
        float startValue = transform.localScale.x;

        State = BalloonState.ScaleAnimation;

        while (time < _scaleAnimationDuration)
        {
            //�c��ݓr���ɃQ�[���I�[�o�[�ɂȂ����珈���I��
            if (State == BalloonState.GameOver) return;

            await UniTask.Yield(token);

            time += Time.deltaTime;
            float progress = Mathf.Clamp01(time / _scaleAnimationDuration);

            float scaleValue = startValue + _scaleOffset * progress;
            ChangeScale(scaleValue);
        }

        State = BalloonState.Expands;
    }

    private void BalloonDeflation(float scaleAmountDeflatingPerSecond)
    {
        if (State != BalloonState.Expands) return;

        float scaleDecrease = scaleAmountDeflatingPerSecond * Time.deltaTime;
        float scaleValue = Mathf.Max(transform.localScale.x - scaleDecrease, _defaultScaleValue);
        ChangeScale(scaleValue);

        if (Mathf.Approximately(scaleValue, _defaultScaleValue))
        {
            State = BalloonState.Normal;
        }
    }

    private void OnWaterStay()
    {
        if (State is not BalloonState.Expands and not BalloonState.ScaleAnimation) return;

        BalloonDeflation(_scaleAmountDeflatingPerSecondInWater);
    }

    private void ChangeScale(float newScale)
    {
        //�J�����̎���p��ύX
        _cinemachineTargetGroup.m_Targets[0].radius = newScale;
        transform.localScale = Vector3.one * newScale;
    }
}
