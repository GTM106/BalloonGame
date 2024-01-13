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

    public void ChangeAnimation(T state, bool resetAnimation = false, float fadeTime = 0.2f)
    {
        //���݂̃X�e�[�g���O��̃X�e�[�g�ƈ�v���Ă�����ύX���Ȃ�
        if (_currentState.Equals(state) && !resetAnimation) return;

        _currentState = state;
        _animator.CrossFadeInFixedTime(_stateData.stateNames[(int)(object)state], fadeTime);
    }
}
