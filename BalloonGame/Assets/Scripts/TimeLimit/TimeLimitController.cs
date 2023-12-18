using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeLimitController : MonoBehaviour
{
    enum State
    {
        Playing,
        Paused,
        Stopped,
    }

    [SerializeField, Required] TimeLimitView _timeLimitView = default!;
    [Header("������������[sec]")]
    [SerializeField, Min(0)] float _initTimeLimit;

    TimeLimit _timeLimit = null;

    State _state = State.Stopped;

    public event Action OnTimeLimit;

    private void Awake()
    {
        DisableUI();
    }

    private void Update()
    {
        Reduce();

        if (_timeLimit != null)
        {
            _timeLimitView.UpdateView(_timeLimit.CurrentTimeLimitValue / _initTimeLimit);
        }
    }

    private void FixedUpdate()
    {
        if (_timeLimit != null) TimeLimitChecker();
    }

    private void Reduce()
    {
        //��ɃX�e�[�g�m�F������
        if (_state != State.Playing) return;
        if (_timeLimit == null) throw new ArgumentNullException("�������Ԃ���������Ă��܂���B");

        _timeLimit.DecreaseTimeDeltaTime();
    }

    private void TimeLimitChecker()
    {
        if (!_timeLimit.IsTimeLimitReached()) return;

        print("�������Ԃł�");

        OnTimeLimit?.Invoke();

        Stop();
    }

    public void AddTimeLimit(TimeLimit value)
    {
        _timeLimit = _timeLimit.AddTimeLimit(value);
    }

    public void ReduceTimeLimit(TimeLimit value)
    {
        _timeLimit = _timeLimit.ReduceTimeLimit(value);
    }

    /// <summary>
    /// �͂��߂��琧�����Ԃ��J�n����B
    /// </summary>
    /// <param name="forced">�ꎞ��~���ł��Đ����邩</param>
    public void Play(bool forced = false)
    {
        //�������s�łȂ�
        if (!forced)
        {
            //�|�[�Y���Ȃ�ĊJ���Ȃ�
            if (_state == State.Paused) return;
        }

        _state = State.Playing;
        _timeLimit = new(_initTimeLimit);
    }

    /// <summary>
    /// �������Ԃ̌������ĊJ����B
    /// </summary>
    public void Resume()
    {
        if (_state != State.Paused)
        {
            Debug.LogWarning("�|�[�Y���ȊO�͍ĊJ�R�}���h�����s���邱�Ƃ͏o���܂���");
            return;
        }

        _state = State.Playing;
    }

    /// <summary>
    /// �������Ԃ��ꎞ��~����B�ĊJ����ۂ�Resume()���Ă�ł��������B
    /// </summary>
    public void Paused()
    {
        //�v���C���ȊO��Pause�ł��Ȃ�
        if (_state != State.Playing)
        {
            Debug.LogWarning("�������ԍĐ����ȊO�͈ꎞ��~�ł��܂���");
            return;
        }

        _state = State.Paused;
    }

    /// <summary>
    /// �������Ԃ����S�Ɏ~�߂�B�I�������͍s���Ȃ����Ƃɒ��ӂ��Ă��������B
    /// </summary>
    public void Stop()
    {
        _state = State.Stopped;
        _timeLimit = null;
    }

    public void EnableUI()
    {
        _timeLimitView.Enable();
    }

    public void DisableUI()
    {
        _timeLimitView.Disable();
    }
}
