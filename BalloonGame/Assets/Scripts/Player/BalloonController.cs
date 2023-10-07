using Cinemachine;
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public enum BalloonState
{
    Normal,
    Expands,
    ScaleAnimation,
    BoostDash,
}

public class BalloonController : MonoBehaviour
{
    //���L��InputActionReference�́AHandler�̖����������܂�
    [SerializeField] InputActionReference _ringPushAction = default!;
    [SerializeField] InputActionReference _ringPullAction = default!;

    [SerializeField] WaterEvent _waterEvent = default!;
    [SerializeField] CinemachineTargetGroup _cinemachineTargetGroup = default!;
    [SerializeField] CinemachineController _cinemachineController = default!;

    [SerializeField] SkinnedMeshRenderer _skinnedMeshRenderer = default!;

    [Header("�c���A�j���[�V�����̎�������")]
    [SerializeField, Min(0f)] float _scaleAnimationDuration = 0.1f;
    [Header("1��v�b�V���łǂ̂��炢�c�����邩�B\nBrendShape�̒l���Q�l�ɂ��Ă�������")]
    [SerializeField, Min(0f)] float _scaleOffset = 10f;
    [Header("1�b�Ԃɂǂ̂��炢���D���k�ނ��B\nBrendShape�̒l���Q�l�ɂ��Ă�������")]
    [SerializeField, Min(0f)] float _scaleAmountDeflatingPerSecond;
    [Header("���ɓ����Ă���Ƃ�1�b�Ԃɂǂ̂��炢���D���k�ނ��B\nBrendShape�̒l���Q�l�ɂ��Ă�������")]
    [SerializeField, Min(0f)] float _scaleAmountDeflatingPerSecondInWater;
    [Header("CinemachineTargetGroup�ɂ�����radius�̍ő�l")]
    [SerializeField, Min(1f)] float cameraRadiusMax = 3.25f;
    [Header("������у_�b�V���̎������ԁBPlayerController�Ɠ����l��ݒ肵�Ă�������")]
    [SerializeField, Min(0)] int _boostFrame = default!;

    //���D�̖c��݋�̏����l�BAwake�ŏ��������Ă��܂�
    float _defaultBlendShapeWeight;

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

    static readonly float MaxBrandShapeValue = 100f;

    private void Awake()
    {
        _defaultBlendShapeWeight = _skinnedMeshRenderer.GetBlendShapeWeight(0);
        _waterEvent.OnStayAction += OnWaterStay;
        _ringPushAction.action.performed += OnRingconPushed;
        _ringPullAction.action.performed += OnRingconPulled;
    }

    private void Update()
    {
        BalloonDeflation(_scaleAmountDeflatingPerSecond);
    }

    private void OnDestroy()
    {
        _waterEvent.OnStayAction -= OnWaterStay;
        _ringPushAction.action.performed -= OnRingconPushed;
        _ringPullAction.action.performed -= OnRingconPulled;
    }

    private void OnRingconPushed(InputAction.CallbackContext obj)
    {
        Expand();
    }

    private void OnRingconPulled(InputAction.CallbackContext obj)
    {
        OnRingconPull();
    }

    private void Expand()
    {
        if (State is not BalloonState.Normal and not BalloonState.Expands) return;

        ExpandScaleAnimation().Forget();
    }

    private async void OnRingconPull()
    {
        if (State != BalloonState.Expands) return;
        var token = this.GetCancellationTokenOnDestroy();

        State = BalloonState.BoostDash;

        //���̏���������ChangeScale�łȂ����ڏ���������B
        _skinnedMeshRenderer.SetBlendShapeWeight(0, _defaultBlendShapeWeight);

        _cinemachineController.OnAfterBoostDash(_boostFrame);

        float startValue = _cinemachineTargetGroup.m_Targets[0].radius;
        int currentFrame = 0;

        while (currentFrame <= _boostFrame)
        {
            await UniTask.Yield(PlayerLoopTiming.FixedUpdate, token);

            float progress = Mathf.Clamp01(currentFrame / _boostFrame);

            //������у_�b�V�������͓���ŃX�P�[���𖳎����ăJ�����̎���p��ύX
            _cinemachineTargetGroup.m_Targets[0].radius = startValue * (1f - progress);

            currentFrame++;
        }

        State = BalloonState.Normal;
    }

    private async UniTask ExpandScaleAnimation()
    {
        if (State == BalloonState.ScaleAnimation) return;
        var token = this.GetCancellationTokenOnDestroy();
        float time = 0f;
        float startValue = _skinnedMeshRenderer.GetBlendShapeWeight(0);
        State = BalloonState.ScaleAnimation;

        while (time < _scaleAnimationDuration)
        {
            await UniTask.Yield(token);

            time += Time.deltaTime;
            float progress = Mathf.Clamp01(time / _scaleAnimationDuration);
            float scaleValue = Mathf.Min(startValue + _scaleOffset * progress, MaxBrandShapeValue);

            ChangeScale(scaleValue);

            //�ő�܂Ŗc��񂾂珈���c��݃A�j���[�V�������I��
            if (Mathf.Approximately(scaleValue, MaxBrandShapeValue)) break;
        }

        State = BalloonState.Expands;
    }

    private void BalloonDeflation(float scaleAmountDeflatingPerSecond)
    {
        if (State != BalloonState.Expands) return;

        float scaleDecrease = scaleAmountDeflatingPerSecond * Time.deltaTime;
        float scaleValue = Mathf.Max(_skinnedMeshRenderer.GetBlendShapeWeight(0) - scaleDecrease, _defaultBlendShapeWeight);
        ChangeScale(scaleValue);

        if (Mathf.Approximately(scaleValue, _defaultBlendShapeWeight))
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
        _skinnedMeshRenderer.SetBlendShapeWeight(0, newScale);

        //�J�����̎���p��ύX
        _cinemachineTargetGroup.m_Targets[0].radius = BlendShapeWeight2CameraRadius(_skinnedMeshRenderer.GetBlendShapeWeight(0));
    }

    private float BlendShapeWeight2CameraRadius(float blendShapeWeight)
    {
        //radious�̍Œ�l�B0~MaxBrandShapeValue �� Offset~cameraRadiusMax��
        //�������邽�߂ɁA�Œ�l�����킹�邽�߂�Offset
        const float Offset = 1f;

        //���݂̐i�s�x(�c��ݓx(0~MaxBrandShapeValue))��ϊ�
        float progress = blendShapeWeight / MaxBrandShapeValue * (cameraRadiusMax - Offset);

        return progress + Offset;
    }
}
