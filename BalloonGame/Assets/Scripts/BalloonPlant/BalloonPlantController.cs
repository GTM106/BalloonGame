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

    [Header("膨張アニメーションの持続時間")]
    [SerializeField, Min(0f)] float _scaleAnimationDuration = 0.1f;
    [Header("1回プッシュでどのくらい膨張するか。\nBrendShapeの値を参考にしてください")]
    [SerializeField, Min(0f)] float _scaleOffset = 50f;
    [Header("1秒間にどのくらい風船が縮むか。\nBrendShapeの値を参考にしてください")]
    [SerializeField, Min(0f)] float _scaleAmountDeflatingPerSecond;

    [Header("風船のマテリアルのSmoothness値の最大値")]
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

            //最大まで膨らんだら処理膨らみアニメーションを終了
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

        //スペキュラーを変更
        foreach (var skinnedMeshRenderers in _ballonPlantSkinnedMeshRenderers)
        {
            skinnedMeshRenderers.material.SetFloat("_Smoothness", BlendShapeWeight2Smoothness(weight));
        }
    }

    private float BlendShapeWeight2Smoothness(float blendShapeWeight)
    {
        //radiousの最低値。0~Max を Offset~Max+Offsetに
        //調整するために、最低値をあわせるためのOffset
        const float Offset = 0.4f;

        //現在の進行度(膨らみ度(0~MaxBrandShapeValue))を変換
        float progress = blendShapeWeight / MaxBrandShapeValue * (_smoothnessMax - Offset);

        return progress + Offset;
    }

    private float GetBalloonBlendShapesValue()
    {
        //すべてのブレンドシェイプの値は同じ
        return _ballonPlantSkinnedMeshRenderers[0].GetBlendShapeWeight(0);
    }

    private void SetBalloonBlendShapesValue(float value)
    {
        //すべてのブレンドシェイプの値は同じ
        foreach (var ballonPlant in _ballonPlantSkinnedMeshRenderers)
        {
            ballonPlant.SetBlendShapeWeight(0, value);
        }
    }
}
