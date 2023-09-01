using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinemachineController : MonoBehaviour
{
    [SerializeField] Cinemachine.CinemachineFreeLook _freeLook = default!;
    [SerializeField] JoyconHandler _joyconHandler = default!;

    [Header("������у_�b�V����̋@�\")]
    [SerializeField] bool _changeTopRigToggle = true;
    [SerializeField, Min(0f)] float _changeTopRigDuration = 0.2f;

    static readonly float topRigValue = 1f;

    private void Update()
    {
        //Cinemachine����InputManager,InputSystem���g���܂����A
        //�����Joycon��ǂݎ��`���ɓƎ��̌`���𗘗p���Ă��邽�ߒ��ړ��͒l�����������܂��B
        _freeLook.m_XAxis.m_InputAxisValue = _joyconHandler.Stick.x;
        _freeLook.m_YAxis.m_InputAxisValue = _joyconHandler.Stick.y;
    }

    public async void OnAfterBoostDash()
    {
        //�J�����̋@�\�Ƃ���ONOFF���ł���悤�ɂ��Ă���B
        //�ݒ��ʂɏ��؂��邩���x���f�U�C���m��ō폜�𐄏��B
        if (!_changeTopRigToggle) return;

        var token = this.GetCancellationTokenOnDestroy();
        float time = 0f;

        float offset = topRigValue - _freeLook.m_YAxis.Value;
        float firstValue = _freeLook.m_YAxis.Value;

        while (time <= _changeTopRigDuration)
        {
            await UniTask.Yield(token);
            time += Time.deltaTime;

            float progress = time / _changeTopRigDuration;

            _freeLook.m_YAxis.Value = firstValue + offset * progress;
        }

        _freeLook.m_YAxis.Value = topRigValue;
    }
}
