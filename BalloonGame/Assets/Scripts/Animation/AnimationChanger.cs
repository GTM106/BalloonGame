using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AnimationChanger<T> where T : Enum
{
    [SerializeField] Animator _animator;
    [SerializeField] AnimatorStateData _stateData;

    T _currentState;

    public void ChangeAnimation(T state, float fadeTime = 0.2f)
    {
        //現在のステートが前回のステートと一致していたら変更しない
        if (_currentState.Equals(state)) return;

        _currentState = state;
        _animator.CrossFadeInFixedTime(_stateData.stateNames[(int)(object)state], fadeTime);
    }
}
