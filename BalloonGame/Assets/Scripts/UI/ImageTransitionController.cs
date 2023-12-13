using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct TrantisionData
{
    public enum TransitionType
    {
        In,
        Out,
    }

    [Min(0f)] public float duration;
    public Color backgroundColor;
    public TransitionType type;
}

public class ImageTransitionController : MonoBehaviour, ITransition
{
    [SerializeField] Material _material = default!;
    [SerializeField] TrantisionData _defaultTransitionData = default!;

    float _duration;
    float _elapsedTime;
    Color _backgroundColor;
    TrantisionData.TransitionType _type;

    private void Awake()
    {
        _material.SetFloat("_Alpha", 0f);
    }

    private void OnApplicationQuit()
    {
        _material.SetFloat("_Alpha", 0f);
    }

    public void StartTransition(TrantisionData trantisionData)
    {
        InitializeTransition(trantisionData);
        UpdateTransition();
    }

    public void StartTransition()
    {
        StartTransition(_defaultTransitionData);
    }

    private void InitializeTransition(TrantisionData trantisionData)
    {
        _elapsedTime = 0f;
        _backgroundColor = trantisionData.backgroundColor;
        _duration = trantisionData.duration;
        _material.SetColor("_Color", _backgroundColor);
        _type = trantisionData.type;
    }

    private async void UpdateTransition()
    {
        var token = this.GetCancellationTokenOnDestroy();

        while (!IsTransitionComplete())
        {
            await UniTask.Yield(token);
            _elapsedTime += Time.deltaTime;

            float progress = _elapsedTime / _duration;

            float alpha = _type switch
            {
                TrantisionData.TransitionType.In => progress,
                TrantisionData.TransitionType.Out => 1f - progress,
                _ => throw new NotImplementedException()
            };

            _material.SetFloat("_Alpha", alpha);
        }
    }

    public bool IsTransitionComplete()
    {
        return _elapsedTime >= _duration;
    }
}
