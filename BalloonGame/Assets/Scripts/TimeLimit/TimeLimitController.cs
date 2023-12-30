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
    [Header("初期制限時間[sec]")]
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
        //先にステート確認をする
        if (_state != State.Playing) return;
        if (_timeLimit == null) throw new ArgumentNullException("制限時間が生成されていません。");

        _timeLimit.DecreaseTimeDeltaTime();
    }

    private void TimeLimitChecker()
    {
        if (!_timeLimit.IsTimeLimitReached()) return;

        print("制限時間です");

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
    /// はじめから制限時間を開始する。
    /// </summary>
    /// <param name="forced">一時停止中でも再生するか</param>
    public void Play(bool forced = false)
    {
        //強制実行でなく
        if (!forced)
        {
            //ポーズ中なら再開しない
            if (_state == State.Paused) return;
        }

        _state = State.Playing;
        _timeLimit = new(_initTimeLimit);
    }

    /// <summary>
    /// 制限時間の減少を再開する。
    /// </summary>
    public void Resume()
    {
        if (_state != State.Paused)
        {
            Debug.LogWarning("ポーズ中以外は再開コマンドを実行することは出来ません");
            return;
        }

        _state = State.Playing;
    }

    /// <summary>
    /// 制限時間を一時停止する。再開する際はResume()を呼んでください。
    /// </summary>
    public void Paused()
    {
        //プレイ中以外はPauseできない
        if (_state != State.Playing)
        {
            Debug.LogWarning("制限時間再生中以外は一時停止できません");
            return;
        }

        _state = State.Paused;
    }

    /// <summary>
    /// 制限時間を完全に止める。終了処理は行われないことに注意してください。
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
