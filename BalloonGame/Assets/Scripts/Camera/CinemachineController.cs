using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CinemachineController : MonoBehaviour
{
    [SerializeField] Cinemachine.CinemachineFreeLook _freeLook = default!;
    [SerializeField] JoyconHandler _joyconHandler = default!;

    [Header("������у_�b�V����̋@�\")]
    [SerializeField] bool _changeTopRigToggle = true;

    [Header("�W���C��")]
    [SerializeField] bool _gyroEnabled = true;
    [SerializeField] bool _gyroInversionX;
    [SerializeField] bool _gyroInversionY;
    [SerializeField, Min(0f)] Vector2 _gyroSpeed = new(0.5f, 0.5f);
    [SerializeField, Min(0f)] Vector2 _gyroDeadZone = new(0.1f, 0.1f);

    static readonly float topRigValue = 1f;
    static readonly float middleRigValue = 0.5f;

    //gyro�̃f�b�h�]�[���Ɋւ���ۑ��Ɏg�p
    Vector2 _prebGyroValue = new();

    private void Awake()
    {
        _joyconHandler.OnTriggerButtonPressed += OnTriggerButtonPressed;
    }

    private void OnTriggerButtonPressed()
    {
        //�J�������Z�b�g
        _freeLook.m_YAxis.Value = middleRigValue;
    }

    private void Update()
    {
        _freeLook.m_XAxis.m_InputAxisValue = _joyconHandler.Stick.x;
        _freeLook.m_YAxis.m_InputAxisValue = _joyconHandler.Stick.y;

        Gyro();
    }

    private void Gyro()
    {
        if (!_gyroEnabled) return;

        float x = _joyconHandler.Gyro.x;
        float y = _joyconHandler.Gyro.y;

        GyroX(x);
        GyroY(y);

        _prebGyroValue.Set(x, y);
    }

    private void GyroX(float x)
    {
        //���O�t���[���ƍ���̃t���[���̃W���C���l�̍��̐�Βl���f�b�h�]�[����菬�����Ȃ�W���C���͋@�\�����Ȃ�
        if (Mathf.Abs(x - _prebGyroValue.x) <= _gyroDeadZone.x) return;

        float gyroX = x * (_gyroInversionX ? -1f : 1f) * _gyroSpeed.x;

        //Cinemachine����InputManager,InputSystem���g���܂����A
        //�����Joycon��ǂݎ��`���ɓƎ��̌`���𗘗p���Ă��邽�ߒ��ړ��͒l�����������܂��B
        _freeLook.m_XAxis.m_InputAxisValue = _joyconHandler.Stick.x + gyroX;
    }

    private void GyroY(float y)
    {
        //���O�t���[���ƍ���̃t���[���̃W���C���l�̍��̐�Βl���f�b�h�]�[����菬�����Ȃ�W���C���͋@�\�����Ȃ�
        if (Mathf.Abs(y - _prebGyroValue.y) <= _gyroDeadZone.y) return;

        float gyroY = y * (_gyroInversionY ? -1f : 1f) * _gyroSpeed.y;

        //Cinemachine����InputManager,InputSystem���g���܂����A
        //�����Joycon��ǂݎ��`���ɓƎ��̌`���𗘗p���Ă��邽�ߒ��ړ��͒l�����������܂��B
        _freeLook.m_YAxis.m_InputAxisValue = _joyconHandler.Stick.y + gyroY;
    }

    public async void OnAfterBoostDash(int waitFrame)
    {
        //�J�����̋@�\�Ƃ���ONOFF���ł���悤�ɂ��Ă���B
        //�ݒ��ʂɏ��؂��邩���x���f�U�C���m��ō폜�𐄏��B
        if (!_changeTopRigToggle) return;

        var token = this.GetCancellationTokenOnDestroy();
        float time = 0f;
        float duration = Time.fixedDeltaTime * waitFrame;

        float offset = topRigValue - _freeLook.m_YAxis.Value;
        float firstValue = _freeLook.m_YAxis.Value;

        while (time <= duration)
        {
            await UniTask.Yield(token);
            time += Time.deltaTime;

            float progress = time / duration;

            _freeLook.m_YAxis.Value = firstValue + offset * progress;
        }

        _freeLook.m_YAxis.Value = topRigValue;
    }
}
