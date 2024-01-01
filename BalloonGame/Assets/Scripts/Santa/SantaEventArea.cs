using Cinemachine;
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SantaEventArea : MonoBehaviour, IHittable
{
    private const int HighPriority = 20;
    [SerializeField, Required] SunglassController _playerSunglassController = default!;
    [SerializeField] AnimationChanger<E_Santa> _animationChanger = default!;
    [SerializeField, Required] CinemachineVirtualCamera _virtualCamera = default!;
    [SerializeField, Required] CinemachineVirtualCamera _virtualCamera2cam = default!;
    [SerializeField, Required] CinemachineVirtualCamera _virtualCamera3cam = default!;
    [SerializeField, Required] InputSystemManager _inputSystemManager = default!;
    [SerializeField, Required] SunglassController _santaSunglassController = default!;
    [SerializeField, Required] TimeLimitController _timeLimitController = default!;

    [SerializeField, Required] Transform _hand;

    bool _hasSunglass = true;

    private void Start()
    {
        _santaSunglassController.Enable();
    }

    public void OnEnter(Collider playerCollider, BalloonState balloonState)
    {
        if (!_hasSunglass) return;
        _hasSunglass = false;

        //カットシーンもどきの開始
        StartCutScene();
    }

    private async void StartCutScene()
    {
        //制限時間を一時停止する
        _timeLimitController.Paused();

        _animationChanger.ChangeAnimation(E_Santa.AN05_GiveSunglasses);

        _inputSystemManager.ChangeMaps(InputSystemManager.ActionMaps.UI);

        _virtualCamera.Priority = HighPriority;
        await UniTask.Delay(TimeSpan.FromSeconds(0.4d));
        _virtualCamera.Priority = 0;
        _virtualCamera2cam.Priority = HighPriority;
        OnHand();
        await UniTask.Delay(TimeSpan.FromSeconds(1.6d));
        _virtualCamera2cam.Priority = 0;
        _virtualCamera3cam.Priority = HighPriority;
        _playerSunglassController.Enable();
        _santaSunglassController.Disable();
        await UniTask.Delay(TimeSpan.FromSeconds(1.0d));
        _virtualCamera3cam.Priority = 0;

        _inputSystemManager.ChangeMaps(InputSystemManager.ActionMaps.Player);

        //制限時間を再開する
        _timeLimitController.Resume();
    }

    public void OnExit(Collider playerCollider, BalloonState balloonState)
    {
    }

    public void OnStay(Collider playerCollider, BalloonState balloonState)
    {
    }

    private void OnHand()
    {
        _santaSunglassController.transform.parent = _hand;
        _santaSunglassController.transform.localPosition = new Vector3(0.014f, -0.079f, -0.006f);
        _santaSunglassController.transform.localRotation = new Quaternion(-0.101377673f, 0.67903924f, -0.716659009f, -0.122589655f);
    }
}
