using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class BalloonPlantController : MonoBehaviour, IHittable
{
    [SerializeField, Required] InputActionReference _pushAction = default!;
    [SerializeField, Required] List<SkinnedMeshRenderer> _ballonPlantSkinnedMeshRenderers = default!;

    [Header("�c���A�j���[�V�����̎�������")]
    [SerializeField, Min(0f)] float _scaleAnimationDuration = 0.1f;
    [Header("1��v�b�V���łǂ̂��炢�c�����邩�B\nBrendShape�̒l���Q�l�ɂ��Ă�������")]
    [SerializeField, Min(0f)] float _scaleOffset = 50f;
    [Header("1�b�Ԃɂǂ̂��炢���D���k�ނ��B\nBrendShape�̒l���Q�l�ɂ��Ă�������")]
    [SerializeField, Min(0f)] float _scaleAmountDeflatingPerSecond;

    [Header("���D�̃}�e���A����Smoothness�l�̍ő�l")]
    [SerializeField, Range(0.4f, 1f)] float _smoothnessMax = 1f;

    bool isNearbyPlayer = false;
    private bool _isScaleAnimation;

    static readonly float MaxBrandShapeValue = 200f;

    private void Reset()
    {
        _ballonPlantSkinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>().ToList();
    }

    private void Awake()
    {
        _pushAction.action.performed += OnPushAction;
    }

    private void Update()
    {
        BalloonDeflation(_scaleAmountDeflatingPerSecond);
    }

    private void OnPushAction(InputAction.CallbackContext obj)
    {
        if (!isNearbyPlayer) { return; }

        ExpandScaleAnimation().Forget();
    }

    public void OnEnter(Collider playerCollider, BalloonState balloonState)
    {
        isNearbyPlayer = true;
    }

    public void OnExit(Collider playerCollider, BalloonState balloonState)
    {
        isNearbyPlayer = false;
    }

    public void OnStay(Collider playerCollider, BalloonState balloonState)
    {
    }

    private async UniTask ExpandScaleAnimation()
    {
        if (_isScaleAnimation) return;
        var token = this.GetCancellationTokenOnDestroy();
        float time = 0f;
        float startValue = GetBalloonBlendShapesValue();
        _isScaleAnimation = true;

        while (time < _scaleAnimationDuration)
        {
            await UniTask.Yield(token);

            time += Time.deltaTime;
            float progress = Mathf.Clamp01(time / _scaleAnimationDuration);
            float scaleValue = Mathf.Min(startValue + _scaleOffset * progress, MaxBrandShapeValue);

            ChangeWeight(scaleValue);

            //�ő�܂Ŗc��񂾂珈���c��݃A�j���[�V�������I��
            if (Mathf.Approximately(scaleValue, MaxBrandShapeValue)) break;
        }

        _isScaleAnimation = false;
    }

    private void BalloonDeflation(float scaleAmountDeflatingPerSecond)
    {
        if (Mathf.Approximately(GetBalloonBlendShapesValue(), 0f)) return;

        float scaleDecrease = scaleAmountDeflatingPerSecond * Time.deltaTime;
        float scaleValue = Mathf.Max(GetBalloonBlendShapesValue() - scaleDecrease, 0f);
        ChangeWeight(scaleValue);
    }

    private void ChangeWeight(float weight)
    {
        SetBalloonBlendShapesValue(weight);

        //�X�y�L�����[��ύX
        foreach (var skinnedMeshRenderers in _ballonPlantSkinnedMeshRenderers)
        {
            skinnedMeshRenderers.material.SetFloat("_Smoothness", BlendShapeWeight2Smoothness(weight));
        }
    }

    private float BlendShapeWeight2Smoothness(float blendShapeWeight)
    {
        //radious�̍Œ�l�B0~Max �� Offset~Max+Offset��
        //�������邽�߂ɁA�Œ�l�����킹�邽�߂�Offset
        const float Offset = 0.4f;

        //���݂̐i�s�x(�c��ݓx(0~MaxBrandShapeValue))��ϊ�
        float progress = blendShapeWeight / MaxBrandShapeValue * (_smoothnessMax - Offset);

        return progress + Offset;
    }

    private float GetBalloonBlendShapesValue()
    {
        //���ׂẴu�����h�V�F�C�v�̒l�͓���
        return _ballonPlantSkinnedMeshRenderers[0].GetBlendShapeWeight(0);
    }

    private void SetBalloonBlendShapesValue(float value)
    {
        //���ׂẴu�����h�V�F�C�v�̒l�͓���
        foreach (var ballonPlant in _ballonPlantSkinnedMeshRenderers)
        {
            ballonPlant.SetBlendShapeWeight(0, value);
        }
    }
}
