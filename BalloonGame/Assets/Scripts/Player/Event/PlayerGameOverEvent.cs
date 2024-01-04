using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class PlayerGameOverEvent : MonoBehaviour
{
    public event Action OnGameOver = default!;
    public event Action OnRevive = default!;

    [SerializeField, Min(0f)] float _invincibleDuration = 3f;
    [SerializeField, Min(0f)] float _meshFlashInterval = 0.2f;
    [SerializeField, Required] GameObject _atii = default!;
    [SerializeField, Required] TimeLimitController _timeLimitController = default!;

    //無敵かどうか
    bool _isInvincible = false;

    SkinnedMeshRenderer[] skinnedMeshRenderers = null;
    MeshRenderer[] meshRenderers = null;

    private void Awake()
    {
        _atii = _atii != null ? _atii : GameObject.Find("Atii");
        skinnedMeshRenderers = _atii.GetComponentsInChildren<SkinnedMeshRenderer>();
        meshRenderers = _atii.GetComponentsInChildren<MeshRenderer>();
        _timeLimitController ??= FindAnyObjectByType<TimeLimitController>();
        _timeLimitController.OnTimeLimit += OnTimeLimit;
    }

    private void OnTimeLimit()
    {
        //制限時間になったら無敵に
        _isInvincible = true;
    }

    public void GameOver()
    {
        if (_isInvincible) return;

        OnGameOver?.Invoke();

        //ゲームオーバー中のオーバーキル防止
        _isInvincible = true;
    }

    public void Revive()
    {
        OnRevive?.Invoke();

        _isInvincible = true;
        StartInvincibleTimer();
    }

    private async void StartInvincibleTimer()
    {
        var token = this.GetCancellationTokenOnDestroy();

        StartFlashMesh(token);
        await UniTask.Delay(TimeSpan.FromSeconds(_invincibleDuration), false, PlayerLoopTiming.FixedUpdate, token);

        _isInvincible = false;
    }

    private async void StartFlashMesh(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        float elapsedTime = 0f;

        while (elapsedTime <= _invincibleDuration)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(_meshFlashInterval), false, PlayerLoopTiming.Update, token);
            elapsedTime += _meshFlashInterval;

            foreach (var rend in skinnedMeshRenderers)
            {
                rend.enabled = !rend.enabled;
            }
            foreach (var rend in meshRenderers)
            {
                rend.enabled = !rend.enabled;
            }
        }

        foreach (var rend in skinnedMeshRenderers)
        {
            rend.enabled = true;
        }        
        foreach (var rend in meshRenderers)
        {
            rend.enabled = true;
        }
    }
}
