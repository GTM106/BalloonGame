using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinemachineController : MonoBehaviour
{
    [SerializeField] Cinemachine.CinemachineFreeLook _freeLook = default!;
    [SerializeField] JoyconHandler _joyconHandler = default!;

    private void Update()
    {
        //Cinemachine����InputManager,InputSystem���g���܂����A
        //�����Joycon��ǂݎ��`���ɓƎ��̌`���𗘗p���Ă��邽�ߒ��ړ��͒l�����������܂��B
        _freeLook.m_XAxis.m_InputAxisValue = _joyconHandler.Stick.x;
        _freeLook.m_YAxis.m_InputAxisValue = _joyconHandler.Stick.y;
    }
}
